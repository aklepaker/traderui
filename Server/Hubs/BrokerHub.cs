using Microsoft.AspNetCore.SignalR;
using traderui.Server.IBKR;

namespace traderui.Server.Hubs;

public class BrokerHub : Hub
{
    private InteractiveBrokers _broker;

    public BrokerHub(InteractiveBrokers broker)
    {
        _broker = broker;
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
