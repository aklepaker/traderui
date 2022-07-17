using IBApi;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Serilog;
using System.ComponentModel.DataAnnotations;
using traderui.Server.Hubs;
using traderui.Shared;
using traderui.Shared.Events;

namespace traderui.Server.IBKR
{
    public class InteractiveBrokers : IInteractiveBrokers
    {
        private IHubContext<BrokerHub> _brokerHub;

        private EClientSocket _client;

        private EWrapperImplementation _impl;

        private Random _random = new Random();
        private readonly ServerOptions _serverOptions;
        private CancellationTokenSource _connectionToken;
        private int _marketDataType { get; set; }
        private Dictionary<string, int> CurrentRequestStack { get; set; } = new Dictionary<string, int>();
        private Dictionary<string, int> GetPriceRequestStack { get; set; } = new Dictionary<string, int>();

        private Dictionary<int, string> RequestIdToSymbol { get; set; } = new Dictionary<int, string>();

        private Dictionary<int, Order> OrderToModify { get; set; } = new();
        private Dictionary<int, Contract> ContractOfOrdersToModify { get; set; } = new();
        private bool ConnectionInProgress { get; set; } = false;

        /// <summary>
        /// The instance related to the InteractiveBrokers API
        /// </summary>
        /// <param name="brokerHub"></param>
        /// <param name="serverOptions">Server configuration</param>
        public InteractiveBrokers(IHubContext<BrokerHub> brokerHub, IOptions<ServerOptions> serverOptions)
        {
            _serverOptions = serverOptions.Value;
            _brokerHub = brokerHub;
            _impl = new EWrapperImplementation(_brokerHub, this);
            _client = _impl.ClientSocket;

            /*
                1 (real-time) disables frozen, delayed and delayed-frozen market data
                3 (delayed) enables delayed and disables delayed-frozen market data
            */
            Log.Information("Realtime Market Datata is {Status}", _serverOptions.UseRealtimeMarketData ? "enabled" : "disabled");
            _marketDataType = _serverOptions.UseRealtimeMarketData ? 1 : 3;

            Connect();
        }

        /// <summary>
        /// Initiate the TWS reader with a <see cref="CancellationToken" /> so we don't create
        /// multiple instances of the reader on reconnect
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void StartTwsReader(CancellationToken cancellationToken)
        {
            var readerSignal = _impl.Signal;
            var reader = new EReader(_client, readerSignal);
            reader.Start();

            new Thread(() =>
            {
                try
                {
                    while (_client.IsConnected() && !cancellationToken.IsCancellationRequested)
                    {
                        readerSignal.waitForSignal();
                        reader.processMsgs();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            })

            {
                IsBackground = true,
            }.Start();
        }

        /// <summary>
        /// Initiate a reconnection handler requesting server time each second, when this request
        /// fail a Connect() call will trigger and the cancellationToken will be canceled, so we
        /// don't spam the endpoint
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void StartReconnectionThread(CancellationToken cancellationToken)
        {
            new Thread(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _client.reqCurrentTime();
                    Thread.Sleep(1000);
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        public bool IsConnected()
        {
            if (_client is null)
            {
                return false;
            }

            return _client.IsConnected();
        }

        public void Connect()
        {
            if (_client.IsConnected())
            {
                return;
            }

            // Since this class is a single instance, and we don't want to
            // spawn multiple connection attempts we keep track so there is
            // only one attempt at a time.
            if (!ConnectionInProgress)
            {
                CancellationToken cancellationToken = new();
                if (_connectionToken is not null)
                {
                    _connectionToken.Cancel();
                }

                ConnectionInProgress = true;

                // The connection attempt blocks the current process
                // so we need to run it in a separate thread.
                Task.Run(() =>
                {
                    try
                    {
                        while (!_client.IsConnected())
                        {
                            try
                            {
                                _client.eConnect(_serverOptions.Server, _serverOptions.Port, _serverOptions.ClientId);

                                // Create new CancellationToken
                                _connectionToken = new CancellationTokenSource();
                                cancellationToken = _connectionToken.Token;

                                StartReconnectionThread(cancellationToken);

                                _brokerHub.Clients.All.SendAsync(nameof(TWSConnectedMessage), new TWSConnectedMessage());
                            }
                            catch (Exception e)
                            {
                                _brokerHub.Clients.All.SendAsync(nameof(TWSDisconnectedMessage), new TWSDisconnectedMessage
                                {
                                    Message = "Not connceted to TWS backend.",
                                });

                                Log.Error("Connection to TWS backend failed. Verify the TWS is running. Error: {Message}", e.Message);
                            }

                            Thread.Sleep(1000);
                        }

                        StartTwsReader(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Connection to TWS backend failed");
                        throw;
                    }

                    ConnectionInProgress = false;

                    return Task.CompletedTask;
                });
            }
        }

        /// <summary>
        /// Modify the "ProfitTaker" order with half of the filled position.
        /// </summary>
        /// <param name="orderId">Parent orderId of the ProfitTaker order</param>
        /// <param name="filled">The filled amount of main order</param>
        public void ModifyProfitOrderWithCorrectQuantity(int orderId, double filled)
        {
            if (OrderToModify.TryGetValue(orderId, out Order order)
                && ContractOfOrdersToModify.TryGetValue(orderId, out Contract contract))
            {
                if (order.ParentId == orderId)
                {
                    // I want half Eddie!
                    var half = Math.Floor(filled / 2);

                    // Change the configuration
                    order.TotalQuantity = half;
                    order.Transmit = true;

                    // Place the orders
                    _client.placeOrder(order.OrderId, contract, order);
                    Log.Information("Modified {OrderType} on {Symbol} for {NumberOfOrders} at {Price}", order.OrderType, contract.Symbol, order.TotalQuantity, order.AuxPrice);

                    // Remove the orders from the modify dictionary
                    ContractOfOrdersToModify.Remove(orderId);
                    OrderToModify.Remove(orderId);
                }
            }
        }

        public string GetSymbolNameFromRequestId(int requestId)
        {
            // Log.Information("Symbol for {requestId} = {symbol}", requestId, RequestIdToSymbol[requestId]);
            return RequestIdToSymbol[requestId];
        }

        public void GetTickerPnL(string account, int conId, bool active)
        {
            try
            {
                _client.reqPnLSingle(conId, account, "", conId);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed loading TickerPnL for contractId: {ConId}", conId);
            }
        }

        public void GetPnL(string account)
        {
            _client.reqPnL(_random.Next(), account, "");
        }

        public void CancelSubscriptions()
        {
            foreach (var requestKey in GetPriceRequestStack.Keys)
            {
                var value = GetPriceRequestStack[requestKey];
                Log.Information("Stopped market data for {requestKey}", requestKey);
                _client.cancelMktData(value);
                _client.cancelHistoricalData(value);
            }

            GetPriceRequestStack.Clear();
        }

        public void CancelSubscriptions(int requestId)
        {
            _client.cancelMktData(requestId);
            _client.cancelHistoricalData(requestId);
        }

        public void GetHistoricPrice(string name)
        {
            Contract contract = new Contract
            {
                // PrimaryExch = "NASDAQ",
                PrimaryExch = "ISLAND",
                Currency = "USD",
                Symbol = name,
                SecType = "STK",
                Exchange = "SMART"
            };

            var requestId = _random.Next();
            RequestIdToSymbol.Add(requestId, name);
            String queryTime = $"{DateTime.Now.ToString("yyyyMMdd")} 23:59:59";
            _client.reqHistoricalData(requestId, contract, String.Empty, "1 D", "1 day", "TRADES", 1, 1, true, null);
        }

        public void GetTicker(string name)
        {
            Contract contract = new Contract
            {
                // PrimaryExch = "NASDAQ",
                PrimaryExch = "ISLAND",
                Currency = "USD",
                Symbol = name,
                SecType = "STK",
                Exchange = "SMART"
            };
            var newRequestId = _random.Next();

            if (RequestIdToSymbol.ContainsValue(name))
            {
                foreach (var kv in RequestIdToSymbol)
                {
                    if (kv.Value == name)
                    {
                        newRequestId = kv.Key;
                    }
                }
            }
            else
            {
                RequestIdToSymbol.Add(newRequestId, name);
            }

            _client.reqIds(-1);
            _client.reqContractDetails(newRequestId, contract);
        }

        public void GetTickerPrice(string name)
        {
            _client.reqMarketDataType(_marketDataType);
            var requestId = _random.Next();

            // CancelSubscriptions();

            Contract contract = new Contract
            {
                // PrimaryExch = "NASDAQ",
                PrimaryExch = "ISLAND",
                Currency = "USD",
                Symbol = name,
                SecType = "STK",
                Exchange = "SMART"
            };

            if (!GetPriceRequestStack.ContainsKey(name))
            {
                GetPriceRequestStack.Add(name, requestId);
            }

            if (!RequestIdToSymbol.ContainsKey(requestId))
            {
                RequestIdToSymbol.Add(requestId, name);
            }

            Log.Information("Requested market data for {name}", name);
            _client.reqMktData(requestId, contract, "165, 221,233,295,595", false, false, null);
            _client.reqHistoricalData(requestId, contract, String.Empty, "1 D", "1 day", "TRADES", 1, 1, true, null);
        }

        public void GetHistoricBarData(string name, int requestId)
        {
            Contract contract = new Contract
            {
                // PrimaryExch = "NASDAQ",
                PrimaryExch = "ISLAND",
                Currency = "USD",
                Symbol = name,
                SecType = "STK",
                Exchange = "SMART"
            };
            // String queryTime = DateTime.Now.AddMonths(-1).ToString("yyyyMMdd HH:mm:ss");
            if (!RequestIdToSymbol.ContainsKey(requestId))
            {
                RequestIdToSymbol.Add(requestId, name);
            }

            _client.reqHistoricalData(requestId, contract, String.Empty, "20 D", "1 day", "TRADES", 1, 1, false, null);
        }

        public void GetSymbol(string name)
        {
            _client.reqIds(-1);
            _client.reqMatchingSymbols(_impl.NextOrderId, name);
        }

        public void PlaceOrder(WebOrder webOrder)
        {
            _client.reqIds(-1);
            var oneCancelAllGroup = $"{webOrder.ContractDetails.Contract.Symbol}_{_impl.NextOrderId}";
            webOrder.Qty = Math.Floor(webOrder.Qty / 2) * 2;

            Order order = new Order
            {
                Action = webOrder.Action.ToString(),
                OrderId = _impl.NextOrderId,
                OrderType = webOrder.OrderType.ToString(),
                TotalQuantity = webOrder.Qty,
                LmtPrice = Math.Round(webOrder.LmtPrice, 2, MidpointRounding.AwayFromZero),
                Transmit = false, // should be false so we don't transmitt the order before the stoploss order is ready
                Tif = "DAY",

                // Disable this if you don't want to use adaptive
                // AlgoStrategy = "Adaptive",
                // AlgoParams = new List<TagValue>
                // {
                //     new TagValue("adaptivePriority", "Normal")
                // }
            };

            if (webOrder.OrderType == OrderType.MKT_CONDITIONAL)
            {
                order.OrderType = OrderType.MKT.ToString();

                PriceCondition priceCondition = (PriceCondition)OrderCondition.Create(OrderConditionType.Price);
                priceCondition.ConId = webOrder.ContractDetails.Contract.ConId;
                priceCondition.Exchange = webOrder.ContractDetails.Contract.Exchange;
                priceCondition.IsMore = true;
                priceCondition.Price = Math.Round(webOrder.LmtPrice, 2, MidpointRounding.AwayFromZero);
                order.Conditions.Add(priceCondition);
            }

            _client.placeOrder(order.OrderId, webOrder.ContractDetails.Contract, order);
            Log.Information("Placed {OrderType} buy order on {Symbol} for {NumberOfOrders} at {Price}", order.OrderType, webOrder.ContractDetails.Contract.Symbol, order.TotalQuantity, order.LmtPrice);

            if (webOrder.TakeProfitAndUpdateStoploss)
            {
                var numberOfDecimals = webOrder.TakeProfitAt is > 0 and < 1 ? 4 : 2;
                var numberOfOrdersToSell = Math.Ceiling(webOrder.Qty / 2);

                _impl.NextOrderId++;
                Order profit = new Order
                {
                    Action = "SELL",
                    ParentId = order.OrderId,
                    OrderId = _impl.NextOrderId,
                    OrderType = "LMT",
                    TotalQuantity = numberOfOrdersToSell,
                    LmtPrice = Math.Round(webOrder.TakeProfitAt, numberOfDecimals, MidpointRounding.ToZero),
                    Transmit = false,
                    Tif = "GTC" // Good til canceled
                };

                ContractOfOrdersToModify[order.OrderId] = webOrder.ContractDetails.Contract;
                OrderToModify[order.OrderId] = profit;

                _client.placeOrder(profit.OrderId, webOrder.ContractDetails.Contract, profit);
                Log.Information(" | Placed {OrderType} partial profit order on {Symbol} for {NumberOfOrders} at {Price}", profit.OrderType, webOrder.ContractDetails.Contract.Symbol, profit.TotalQuantity, profit.LmtPrice);

                _impl.NextOrderId++;
                Order profitStopLoss = new Order
                {
                    Action = "SELL",
                    ParentId = profit.OrderId,
                    OrderId = _impl.NextOrderId,
                    OrderType = "STP",
                    OcaGroup = oneCancelAllGroup,
                    OcaType = 2,
                    TotalQuantity = webOrder.Qty - numberOfOrdersToSell,
                    AuxPrice = Math.Round(webOrder.Price, numberOfDecimals, MidpointRounding.ToZero),
                    LmtPrice = Math.Round(webOrder.Price, numberOfDecimals, MidpointRounding.ToZero),
                    Transmit = false,
                    Tif = "GTC" // Good til canceled
                };
                _client.placeOrder(profitStopLoss.OrderId, webOrder.ContractDetails.Contract, profitStopLoss);
                Log.Information(" | - Placed {OrderType} partial stop order on {Symbol} for {NumberOfOrders} at {Price}", profitStopLoss.OrderType, webOrder.ContractDetails.Contract.Symbol, profitStopLoss.TotalQuantity, profitStopLoss.LmtPrice);
            }

            if (webOrder.Action == MarketAction.BUY)
            {
                var numberOfDecimals = webOrder.StopLossAt is > 0 and < 1 ? 4 : 2;
                _impl.NextOrderId++;
                Order stopLoss = new Order
                {
                    Action = "SELL",
                    ParentId = order.OrderId,
                    OrderId = _impl.NextOrderId,
                    OrderType = "STP",
                    OcaGroup = oneCancelAllGroup,

                    // Remaining orders are proportionately reduced in size with no block.
                    // This is the only way we get the main SL-order to cancle when the profit-taker
                    // activates.
                    OcaType = 3,
                    TotalQuantity = webOrder.Qty,
                    AuxPrice = Math.Round(webOrder.StopLossAt, numberOfDecimals, MidpointRounding.ToZero),
                    Transmit = webOrder.Transmit,
                    Tif = "GTC" // Good til canceled
                };
                _client.placeOrder(stopLoss.OrderId, webOrder.ContractDetails.Contract, stopLoss);
                Log.Information("Placed {OrderType} on {Symbol} for {NumberOfOrders} at {Price}", stopLoss.OrderType, webOrder.ContractDetails.Contract.Symbol, stopLoss.TotalQuantity, stopLoss.AuxPrice);
            }

            _client.reqIds(-1); // Need to to this so we're up on the correct number on serverside.
        }

        public void GetPositions()
        {
            _client.reqPositions();
        }

        public void GetAccountSummary(bool stopRequest)
        {
            _client.cancelAccountSummary(2000);

            if (!stopRequest)
            {
                _client.reqAccountSummary(2000, "All", "NetLiquidation,AvailableFunds");
            }
        }
    }
}
