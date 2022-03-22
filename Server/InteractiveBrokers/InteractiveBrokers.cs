using IBApi;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Serilog;
using traderui.Server.Hubs;
using traderui.Shared;
using Action = traderui.Shared.Action;

namespace traderui.Server.IBKR;

public class InteractiveBrokers
{
    public IHubContext<BrokerHub> _brokerHub;

    private EClientSocket _client;

    private EWrapperImplementation _impl;

    private Random _random = new Random();
    private readonly ServerOptions _serverOptions;
    public Dictionary<string, int> CurrentRequestStack { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> GetPriceRequestStack { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// The instance related to the InteractiveBrokers API
    /// </summary>
    /// <param name="brokerHub"></param>
    /// <param name="serverOptions">Server configuration</param>
    public InteractiveBrokers(IHubContext<BrokerHub> brokerHub, IOptions<ServerOptions> serverOptions)
    {
        _serverOptions = serverOptions.Value;
        _brokerHub = brokerHub;
        Connect();
    }

    private void Connect()
    {
        _impl = new EWrapperImplementation(_brokerHub);
        _client = _impl.ClientSocket;
        var readerSignal = _impl.Signal;

        if (_client.IsConnected())
        {
            return;
        }

        try
        {
            while (!_client.IsConnected())
            {
                _client.eConnect(_serverOptions.Server, _serverOptions.Port, _serverOptions.ClientId);
                Thread.Sleep(1000);
                Log.Information("Waiting for connection to TWS");
            }

            var reader = new EReader(_client, readerSignal);
            reader.Start();

            new Thread(() =>
            {
                while (_client.IsConnected())
                {
                    readerSignal.waitForSignal();
                    reader.processMsgs();
                }
            })
            {
                IsBackground = true
            }.Start();
        }
        catch (Exception e)
        {
            Log.Error(e, "Connection failed");
            throw;
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
            GetPriceRequestStack.Remove(requestKey);
        }
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
        if (!_client.IsConnected())
        {
            Connect();
        }

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
        if (!_client.IsConnected())
        {
            Connect();
        }

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
        if (!_client.IsConnected())
        {
            Connect();
        }

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

        _client.placeOrder(order.OrderId, webOrder.Contract, order);
        if (webOrder.Action == Action.BUY)
        {
            Order stopLoss = new Order
            {
                Action = "SELL",
                ParentId = order.OrderId,
                OrderId = _impl.NextOrderId + 1,
                OrderType = "STP",
                TotalQuantity = webOrder.Qty,
                AuxPrice = webOrder.StopLossAt,
                Transmit = webOrder.Transmit,
                Tif = "GTC" // Good til canceled
            };
            _client.placeOrder(stopLoss.OrderId, webOrder.Contract, stopLoss);
        }

        _client.reqIds(-1); // Need to to this so we're up on the correct number on serverside.
    }

    public void GetPositions()
    {
        _client.reqPositions();
    }

    public void GetAccountSummary(bool stopRequest)
    {
        if (!_client.IsConnected())
        {
            Connect();
        }

        _client.cancelAccountSummary(2000);

        if (!stopRequest)
        {
            _client.reqAccountSummary(2000, "All", "NetLiquidation,AvailableFunds");
        }
    }
}
