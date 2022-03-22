using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class CancelSubscriptionsCommandHandler : IRequestHandler<CancelSubscriptionsCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public CancelSubscriptionsCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(CancelSubscriptionsCommand request, CancellationToken cancellationToken)
        {
            _broker.CancelSubscriptions();
            return Task.FromResult(new Unit());
        }
    }
}
