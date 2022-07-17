using System.Net.Http.Json;
using traderui.Shared;
using traderui.Shared.Requests;

namespace traderui.Client.Services;

public class BrokerService : IBrokerService
{
    private readonly HttpClient _httpClient;
    private string _connectionId;

    public BrokerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task GetTicker(string name, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/ticker/{name}/{_connectionId}", cancellationToken);
    }

    public async Task GetTickerPrice(string name, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/ticker/{name}/price/{_connectionId}", cancellationToken);
    }

    public async Task GetHistoricalBarData(string name, int requestId, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/ticker/{name}/historicbardata/{requestId}/{_connectionId}", cancellationToken);
    }

    public async Task BuyOrder(string name, PlaceOrderRequest orderRequest, CancellationToken cancellationToken)
    {
        await _httpClient.PostAsJsonAsync($"api/broker/ticker/{name}/buy", orderRequest, cancellationToken);
    }

    public async Task GetAccountSummary(bool stopRequest, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/account/summary/{stopRequest}", cancellationToken);
    }

    public async Task GetPositions(CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/account/positions", cancellationToken);
    }

    public async Task GetPnL(string account, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/account/pnl/{account}", cancellationToken);
    }

    public async Task GetTickerPnL(string account, int conId, bool active, CancellationToken cancellationToken)
    {
        try
        {
            await _httpClient.GetAsync($"api/broker/account/pnl/{account}/{conId}/{active}", cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task CancelSubscriptions(CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/cancelSubscriptions", cancellationToken);
    }

    public void SetConnectionId(string connectionId)
    {
        _connectionId = connectionId;
    }
}
