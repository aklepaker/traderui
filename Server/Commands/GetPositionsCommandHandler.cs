using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetPositionsCommandHandler : IRequestHandler<GetPositionsCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetPositionsCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetPositionsCommand request, CancellationToken cancellationToken)
        {
            _broker.GetPositions();
            return Task.FromResult(new Unit());
        }
    }
}
