using traderui.Shared;

namespace traderui.Client.Services;

public interface IBrokerService
{
    Task GetTicker(string name, CancellationToken cancellationToken);
    Task GetTickerPrice(string name, CancellationToken cancellationToken);
    Task BuyOrder(string name, WebOrder webOrder, CancellationToken cancellationToken);
    Task GetAccountSummary(bool stopRequest, CancellationToken cancellationToken);
    Task GetPositions(CancellationToken cancellationToken);
    Task GetPnL(string account, CancellationToken cancellation);
    Task GetTickerPnL(string account, int conId, bool active, CancellationToken cancellation);
    Task CancelSubscriptions(CancellationToken cancellation);
    Task GetHistoricalBarData(string name, int requestId, CancellationToken cancellationToken);
}
