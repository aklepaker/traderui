using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetPnLCommandHandler : IRequestHandler<GetPnLCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetPnLCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetPnLCommand request, CancellationToken cancellationToken)
        {
            _broker.GetPnL(request.Account);
            return Task.FromResult(new Unit());
        }
    }
}
