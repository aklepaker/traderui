using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.Hubs;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetHistoricalBarDataCommandHandler : IRequestHandler<GetHistoricalBarDataCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;
        private readonly BrokerHubService _brokerHubService;

        public GetHistoricalBarDataCommandHandler(IInteractiveBrokers broker, BrokerHubService brokerHubService)
        {
            _broker = broker;
            _brokerHubService = brokerHubService;
        }

        public Task<Unit> Handle(GetHistoricalBarDataCommand request, CancellationToken cancellationToken)
        {
            _broker.GetHistoricBarData(request.Symbol, request.RequestId);
            return Task.FromResult(new Unit());
        }
    }
}
