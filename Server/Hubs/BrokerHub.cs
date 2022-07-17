using Microsoft.AspNetCore.SignalR;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using traderui.Server.IBKR;
using traderui.Shared.Events;

namespace traderui.Server.Hubs;

public class BrokerHub : Hub
{
    private IInteractiveBrokers _broker;
    private readonly BrokerHubService _brokerHubService;

    public BrokerHub(IInteractiveBrokers broker, BrokerHubService brokerHubService)
    {
        _broker = broker;
        _brokerHubService = brokerHubService;
    }

    public override Task OnConnectedAsync()
    {
        if (!_broker.IsConnected())
        {
            _broker.Connect();
            Clients.Client(Context.ConnectionId).SendAsync(nameof(TWSDisconnectedMessage), new TWSDisconnectedMessage());
        }
        else
        {
            Clients.Client(Context.ConnectionId).SendAsync(nameof(TWSConnectedMessage), new TWSConnectedMessage
            {
                Version = FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).ProductVersion
            });
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
         // _broker.CancelSubscriptions();
        _brokerHubService.RemoveFromGroup(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public void Disconnect()
    {
    }
}
