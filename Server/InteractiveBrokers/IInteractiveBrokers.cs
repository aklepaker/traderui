using traderui.Shared;

namespace traderui.Server.IBKR
{
    public interface IInteractiveBrokers
    {
        void GetTickerPnL(string account, int conId, bool active);
        void GetPnL(string account);
        void CancelSubscriptions();
        void GetHistoricPrice(string name);
        void GetTicker(string name);
        void GetTickerPrice(string name);
        void GetHistoricBarData(string name, int requestId);
        void GetSymbol(string name);
        void PlaceOrder(WebOrder webOrder);
        void GetPositions();
        void GetAccountSummary(bool stopRequest);
        bool IsConnected();
        void Connect();
    }
}
