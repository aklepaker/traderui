using IBApi;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Serilog;
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
        private Dictionary<string, int> CurrentRequestStack { get; set; } = new Dictionary<string, int>();
        private Dictionary<string, int> GetPriceRequestStack { get; set; } = new Dictionary<string, int>();

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

            String queryTime = $"{DateTime.Now.ToString("yyyyMMdd")} 23:59:59";
            _client.reqHistoricalData(_random.Next(), contract, String.Empty, "1 D", "1 day", "TRADES", 1, 1, true, null);
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
            _client.reqIds(-1);
            _client.reqContractDetails(newRequestId, contract);
        }

        public void GetTickerPrice(string name)
        {
            _client.reqMarketDataType(1);
            var requestId = _random.Next();

            CancelSubscriptions();

            Contract contract = new Contract
            {
                // PrimaryExch = "NASDAQ",
                PrimaryExch = "ISLAND",
                Currency = "USD",
                Symbol = name,
                SecType = "STK",
                Exchange = "SMART"
            };

            GetPriceRequestStack.Add(name, requestId);

            Log.Information("Requested market data for {name}", name);
            _client.reqMktData(requestId, contract, "221", false, false, null);
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

            _client.placeOrder(order.OrderId, webOrder.ContractDetails.Contract, order);

            var numberOfDecimals = webOrder.StopLossAt is > 0 and < 1 ? 4 : 2;

            if (webOrder.Action == MarketAction.BUY)
            {
                Order stopLoss = new Order
                {
                    Action = "SELL",
                    ParentId = order.OrderId,
                    OrderId = _impl.NextOrderId + 1,
                    OrderType = "STP",
                    TotalQuantity = webOrder.Qty,
                    AuxPrice = Math.Round(webOrder.StopLossAt, numberOfDecimals, MidpointRounding.ToZero),
                    Transmit = webOrder.Transmit,
                    Tif = "GTC" // Good til canceled
                };
                _client.placeOrder(stopLoss.OrderId, webOrder.ContractDetails.Contract, stopLoss);
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
