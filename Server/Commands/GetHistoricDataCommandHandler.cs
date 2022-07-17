using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.Hubs;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetHistoricDataCommandHandler : IRequestHandler<GetHistoricDataCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;
        private readonly BrokerHubService _brokerHubService;

        public GetHistoricDataCommandHandler(IInteractiveBrokers broker, BrokerHubService brokerHubService)
        {
            _broker = broker;
            _brokerHubService = brokerHubService;
        }

        public Task<Unit> Handle(GetHistoricDataCommand request, CancellationToken cancellationToken)
        {
            _brokerHubService.AddToGroup(request.ConnectionId, request.Symbol);
            _broker.GetHistoricPrice(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
