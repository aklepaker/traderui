using MediatR;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using traderui.Server.Hubs;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetTickerCommandHandler : IRequestHandler<GetTickerCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;
        private readonly BrokerHubService _brokerHubService;

        public GetTickerCommandHandler(IInteractiveBrokers broker, BrokerHubService brokerHubService)
        {
            _broker = broker;
            _brokerHubService = brokerHubService;
        }

        public Task<Unit> Handle(GetTickerCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Requesting Symbol information for {Name}", request.Symbol);
            _brokerHubService.AddToGroup(request.ConnectionId, request.Symbol);
            _broker.GetTicker(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
