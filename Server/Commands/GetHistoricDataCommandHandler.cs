using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetHistoricDataCommandHandler : IRequestHandler<GetHistoricDataCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetHistoricDataCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetHistoricDataCommand request, CancellationToken cancellationToken)
        {
            _broker.GetHistoricPrice(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
