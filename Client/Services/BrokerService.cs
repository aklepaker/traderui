using System.Net.Http.Json;
using traderui.Shared;
using traderui.Shared.Models;

namespace traderui.Client.Services;

public class BrokerService : IBrokerService
{
    private readonly HttpClient _httpClient;

    public BrokerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task GetTicker(string name, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/ticker/{name}", cancellationToken);
    }

    public async Task GetTickerPrice(string name, CancellationToken cancellationToken)
    {
        await _httpClient.GetFromJsonAsync<Ticker>($"api/broker/ticker/{name}/price", cancellationToken);
    }

    public async Task GetHistoricalBarData(string name, int requestId, CancellationToken cancellationToken)
    {
        await _httpClient.GetAsync($"api/broker/ticker/{name}/historicbardata/{requestId}", cancellationToken);
    }

    public async Task BuyOrder(string name, WebOrder webOrder, CancellationToken cancellationToken)
    {
        await _httpClient.PostAsJsonAsync($"api/broker/ticker/{name}/buy", webOrder, cancellationToken);
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
}
