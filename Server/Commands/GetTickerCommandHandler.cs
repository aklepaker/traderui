using MediatR;
using Serilog;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetTickerCommandHandler : IRequestHandler<GetTickerCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetTickerCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetTickerCommand request, CancellationToken cancellationToken)
        {
            Log.Information("Requesting Symbol information for {Name}", request.Symbol);
            _broker.GetTicker(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
