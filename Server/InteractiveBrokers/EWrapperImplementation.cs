using IBApi;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Text.Json;
using traderui.Server.Hubs;
using traderui.Server.IBKR;
using traderui.Shared.Events;

public class EWrapperImplementation : EWrapper
{
    public int NextOrderId { get; set; }
    public EClientSocket ClientSocket { get; set; }

    public readonly EReaderSignal Signal;
    public IHubContext<BrokerHub> _brokerHub;
    private readonly IInteractiveBrokers _broker;

    public EWrapperImplementation(IHubContext<BrokerHub> brokerHub, IInteractiveBrokers broker)
    {
        _brokerHub = brokerHub;
        _broker = broker;
        Signal = new EReaderMonitorSignal();
        ClientSocket = new EClientSocket(this, Signal);
    }

    public void error(Exception e)
    {
        _brokerHub.Clients.All.SendAsync("error", e.Message);
        throw new Exception(e.Message, innerException: e);
    }

    public void error(string str)
    {
        Log.Debug("{message}", str);
    }

    public void error(int id, int errorCode, string message)
    {
        ErrorCodeMessage errorCodeMessage = new ErrorCodeMessage
        {
            Id = id,
            ErrorCode = errorCode,
            ErrorMessage = message,
        };

        if (id == -1 && errorCode <= 0)
        {
            Log.Information("{message}", message);
            _brokerHub.Clients.All.SendAsync("log", $"Info: {message}");
            _brokerHub.Clients.All.SendAsync(nameof(ErrorCodeMessage), errorCodeMessage);
            return;
        }
        else
        {
            Log.Warning("{errorCode} - {message}",errorCode, message);
            _brokerHub.Clients.All.SendAsync("log", $"{errorCode} {message}");
        }

        switch (errorCode)
        {
            case -1:
                _brokerHub.Clients.All.SendAsync(nameof(ErrorCodeMessage), errorCodeMessage);
                break;

            case 200:
                _brokerHub.Clients.All.SendAsync(nameof(ErrorCodeMessage), errorCodeMessage);
                break;

            case 504:
                _broker.Connect();
                break;
        }
    }

    public void currentTime(long time)
    {
    }

    public void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
    {
        _brokerHub.Clients.All.SendAsync(nameof(TickPriceMessage), new TickPriceMessage
        {
            TickerId = tickerId,
            Field = field,
            Price = price,
            TickerAttrib = attribs,
        });
    }

    public void tickSize(int tickerId, int field, int size)
    {
    }

    public void tickString(int tickerId, int field, string value)
    {
    }

    public void tickGeneric(int tickerId, int field, double value)
    {
    }

    public void tickEFP(int tickerId, int tickType, double basisPoints, string formattedBasisPoints, double impliedFuture, int holdDays, string futureLastTradeDate, double dividendImpact, double dividendsToLastTradeDate)
    {
    }

    public void deltaNeutralValidation(int reqId, DeltaNeutralContract deltaNeutralContract)
    {
    }

    public void tickOptionComputation(int tickerId, int field, double impliedVolatility, double delta, double optPrice, double pvDividend, double gamma, double vega, double theta, double undPrice)
    {
    }

    public void tickSnapshotEnd(int tickerId)
    {
    }

    public void nextValidId(int orderId)
    {
        NextOrderId = orderId;
    }

    public void managedAccounts(string accountsList)
    {
    }

    public void connectionClosed()
    {
        Log.Information("Connection to TWS closed");
        _brokerHub.Clients.All.SendAsync(nameof(TWSDisconnectedMessage), new TWSDisconnectedMessage
        {
            Message = $"Broker Connected",
        });
    }

    public void accountSummary(int reqId, string account, string tag, string value, string currency)
    {

        _brokerHub.Clients.All.SendAsync(nameof(AccountSummaryMessage), new AccountSummaryMessage
        {
            Account = account,
            Tag = tag,
            Value = value,
            Currency = currency,
        });
    }

    public void accountSummaryEnd(int reqId)
    {
    }

    public void bondContractDetails(int reqId, ContractDetails contract)
    {
    }

    public void updateAccountValue(string key, string value, string currency, string accountName)
    {
    }

    public void updatePortfolio(Contract contract, double position, double marketPrice, double marketValue, double averageCost, double unrealizedPNL, double realizedPNL, string accountName)
    {
    }

    public void updateAccountTime(string timestamp)
    {
    }

    public void accountDownloadEnd(string account)
    {
    }

    public void orderStatus(int orderId, string status, double filled, double remaining, double avgFillPrice, int permId, int parentId, double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
    {
        // When the order is filled, we check to see if there are any
        // child orders that should be updated with the correct size
        // due to Bracket orders default to full position.
        if (status.Equals("Filled"))
        {
            _broker.ModifyProfitOrderWithCorrectQuantity(orderId, filled);
        }

        _brokerHub.Clients.All.SendAsync(nameof(OrderStatusMessage), new OrderStatusMessage
        {
            OrderId = orderId,
            Status = status,
            Filled = filled,
            Remaining = remaining,
            AvgFillPrice = avgFillPrice,
            PermId = permId,
            ParentId = parentId,
            LastFillPrice = lastFillPrice,
            ClientId = clientId,
            WhyHeld = whyHeld,
            MarketCapPrice = mktCapPrice,
        });
    }

    public void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
    {
        _brokerHub.Clients.All.SendAsync(nameof(OpenOrderMessage), new OpenOrderMessage {Contract = contract, Order = order, OrderState = orderState,});
    }

    public void openOrderEnd()
    {
    }

    public void contractDetails(int reqId, ContractDetails contractDetails)
    {
        _brokerHub.Clients.All.SendAsync(nameof(ContractDetailsMessage), new ContractDetailsMessage {RequestId = reqId, ContractDetails = contractDetails});
    }

    public void contractDetailsEnd(int reqId)
    {
    }

    public void execDetails(int reqId, Contract contract, Execution execution)
    {
    }

    public void execDetailsEnd(int reqId)
    {
    }

    public void commissionReport(CommissionReport commissionReport)
    {
    }

    public void fundamentalData(int reqId, string data)
    {
    }

    public void historicalData(int reqId, Bar bar)
    {
        _brokerHub.Clients.All.SendAsync(nameof(HistoricalDataMessage), new HistoricalDataMessage {RequestId = reqId, Bar = bar,});
    }

    public void historicalDataUpdate(int reqId, Bar bar)
    {
        _brokerHub.Clients.All.SendAsync(nameof(HistoricalDataUpdateMessage), new HistoricalDataUpdateMessage {RequestId = reqId, Bar = bar,});
    }

    public void historicalDataEnd(int reqId, string start, string end)
    {
        _brokerHub.Clients.All.SendAsync(nameof(HistoricalDataEndMessage), new HistoricalDataEndMessage {RequestId = reqId, Start = start, End = end,});
    }

    public void marketDataType(int reqId, int marketDataType)
    {
    }

    public void updateMktDepth(int tickerId, int position, int operation, int side, double price, int size)
    {
    }

    public void updateMktDepthL2(int tickerId, int position, string marketMaker, int operation, int side, double price, int size, bool isSmartDepth)
    {
    }

    public void updateNewsBulletin(int msgId, int msgType, string message, string origExchange)
    {
    }

    public void position(string account, Contract contract, double pos, double avgCost)
    {
        _brokerHub.Clients.All.SendAsync(nameof(PositionMessage), new PositionMessage
        {
            Account = account,
            Contract = contract,
            Pos = pos,
            AvgCost = avgCost,
        });
    }

    public void positionEnd()
    {
    }

    public void realtimeBar(int reqId, long date, double open, double high, double low, double close, long volume, double WAP, int count)
    {
    }

    public void scannerParameters(string xml)
    {
    }

    public void scannerData(int reqId, int rank, ContractDetails contractDetails, string distance, string benchmark, string projection, string legsStr)
    {
    }

    public void scannerDataEnd(int reqId)
    {
    }

    public void receiveFA(int faDataType, string faXmlData)
    {
    }

    public void verifyMessageAPI(string apiData)
    {
    }

    public void verifyCompleted(bool isSuccessful, string errorText)
    {
    }

    public void verifyAndAuthMessageAPI(string apiData, string xyzChallenge)
    {
    }

    public void verifyAndAuthCompleted(bool isSuccessful, string errorText)
    {
    }

    public void displayGroupList(int reqId, string groups)
    {
    }

    public void displayGroupUpdated(int reqId, string contractInfo)
    {
    }

    public void connectAck()
    {
        _brokerHub.Clients.All.SendAsync(nameof(ConnectAckMessage), new ConnectAckMessage
        {
            Message = $"Broker Connected",
        });
    }

    public void positionMulti(int requestId, string account, string modelCode, Contract contract, double pos, double avgCost)
    {
    }

    public void positionMultiEnd(int requestId)
    {
    }

    public void accountUpdateMulti(int requestId, string account, string modelCode, string key, string value, string currency)
    {
    }

    public void accountUpdateMultiEnd(int requestId)
    {
    }

    public void securityDefinitionOptionParameter(int reqId, string exchange, int underlyingConId, string tradingClass, string multiplier, HashSet<string> expirations, HashSet<double> strikes)
    {
    }

    public void securityDefinitionOptionParameterEnd(int reqId)
    {
    }

    public void softDollarTiers(int reqId, SoftDollarTier[] tiers)
    {
    }

    public void familyCodes(FamilyCode[] familyCodes)
    {
    }

    public void symbolSamples(int reqId, ContractDescription[] contractDescriptions)
    {
        _brokerHub.Clients.All.SendAsync("contractDetails", JsonSerializer.Serialize(contractDescriptions, new JsonSerializerOptions {WriteIndented = true}));
    }

    public void mktDepthExchanges(DepthMktDataDescription[] depthMktDataDescriptions)
    {
    }

    public void tickNews(int tickerId, long timeStamp, string providerCode, string articleId, string headline, string extraData)
    {
    }

    public void smartComponents(int reqId, Dictionary<int, KeyValuePair<string, char>> theMap)
    {
    }

    public void tickReqParams(int tickerId, double minTick, string bboExchange, int snapshotPermissions)
    {
    }

    public void newsProviders(NewsProvider[] newsProviders)
    {
    }

    public void newsArticle(int requestId, int articleType, string articleText)
    {
    }

    public void historicalNews(int requestId, string time, string providerCode, string articleId, string headline)
    {
    }

    public void historicalNewsEnd(int requestId, bool hasMore)
    {
    }

    public void headTimestamp(int reqId, string headTimestamp)
    {
    }

    public void histogramData(int reqId, HistogramEntry[] data)
    {
    }

    public void rerouteMktDataReq(int reqId, int conId, string exchange)
    {
    }

    public void rerouteMktDepthReq(int reqId, int conId, string exchange)
    {
    }

    public void marketRule(int marketRuleId, PriceIncrement[] priceIncrements)
    {
    }

    public void pnl(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
    {
        _brokerHub.Clients.All.SendAsync(nameof(PnlMessage), new PnlMessage
        {
            RequestId = reqId, DailyPnl = dailyPnL, UnrealizedPnl = unrealizedPnL, RealizedPnl = realizedPnL,
        });
    }

    public void pnlSingle(int reqId, int pos, double dailyPnL, double unrealizedPnL, double realizedPnL, double value)
    {
        _brokerHub.Clients.All.SendAsync(nameof(PnlSingleMessage), new PnlSingleMessage
        {
            RequestId = reqId,
            Pos = pos,
            DailyPnl = dailyPnL.Equals(Double.MaxValue) ? 0 : dailyPnL,
            UnrealizedPnl = unrealizedPnL.Equals(Double.MaxValue) ? 0 : unrealizedPnL,
            RealizedPnl = realizedPnL.Equals(Double.MaxValue) ? 0 : realizedPnL,
            Value = value,
        });
    }

    public void historicalTicks(int reqId, HistoricalTick[] ticks, bool done)
    {
    }

    public void historicalTicksBidAsk(int reqId, HistoricalTickBidAsk[] ticks, bool done)
    {
    }

    public void historicalTicksLast(int reqId, HistoricalTickLast[] ticks, bool done)
    {
    }

    public void tickByTickAllLast(int reqId, int tickType, long time, double price, int size, TickAttribLast tickAttriblast, string exchange, string specialConditions)
    {
    }

    public void tickByTickBidAsk(int reqId, long time, double bidPrice, double askPrice, int bidSize, int askSize, TickAttribBidAsk tickAttribBidAsk)
    {
        _brokerHub.Clients.All.SendAsync(nameof(TickByTickBidAskMessage), new TickByTickBidAskMessage
        {
            RequestId = reqId,
            Time = time,
            BidPrice = bidPrice,
            AskPrice = askPrice,
            BidSize = bidSize,
            AskSize = askSize,
            TickAttribBidAsk = tickAttribBidAsk,
        });
    }

    public void tickByTickMidPoint(int reqId, long time, double midPoint)
    {
    }

    public void orderBound(long orderId, int apiClientId, int apiOrderId)
    {
    }

    public void completedOrder(Contract contract, Order order, OrderState orderState)
    {
    }

    public void completedOrdersEnd()
    {
    }
}
