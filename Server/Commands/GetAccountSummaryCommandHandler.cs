using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetAccountSummaryCommandHandler : IRequestHandler<GetAccountSummaryCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetAccountSummaryCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetAccountSummaryCommand request, CancellationToken cancellationToken)
        {
            _broker.GetAccountSummary(request.StopRequest);
            return Task.FromResult(new Unit());
        }
    }
}
