using MediatR;
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
            if (request.ContractId > 0)
            {
                _broker.GetTickerPnL(request.Account, request.ContractId, request.Active);
            }
            else
            {
                _broker.GetPnL(request.Account);
            }

            return Task.FromResult(new Unit());
        }
    }
}
