using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.Hubs;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetTickerPriceCommandHandler : IRequestHandler<GetTickerPriceCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;
        private readonly BrokerHubService _brokerHubService;

        public GetTickerPriceCommandHandler(IInteractiveBrokers broker, BrokerHubService brokerHubService)
        {
            _broker = broker;
            _brokerHubService = brokerHubService;
        }

        public Task<Unit> Handle(GetTickerPriceCommand request, CancellationToken cancellationToken)
        {
            _brokerHubService.AddToGroup(request.ConnectionId, request.Symbol);
            _broker.GetTickerPrice(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
