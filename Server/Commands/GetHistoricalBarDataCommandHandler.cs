using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetHistoricalBarDataCommandHandler : IRequestHandler<GetHistoricalBarDataCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetHistoricalBarDataCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetHistoricalBarDataCommand request, CancellationToken cancellationToken)
        {
            _broker.GetHistoricBarData(request.Symbol, request.RequestId);
            return Task.FromResult(new Unit());
        }
    }
}
