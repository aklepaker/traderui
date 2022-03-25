using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using traderui.Server.IBKR;
using traderui.Shared.Events;

namespace traderui.Server.Hubs;

public class BrokerHub : Hub
{
    private IInteractiveBrokers _broker;

    public BrokerHub(IInteractiveBrokers broker)
    {
        _broker = broker;
    }

    public override Task OnConnectedAsync()
    {
        if (!_broker.IsConnected())
        {
            Clients.All.SendAsync(nameof(TWSDisconnectedMessage), new TWSDisconnectedMessage());
        }
        else
        {
            Clients.All.SendAsync(nameof(TWSConnectedMessage), new TWSConnectedMessage());
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        _broker.CancelSubscriptions();
        return base.OnDisconnectedAsync(exception);
    }

    public void Disconnect()
    {
    }
}
