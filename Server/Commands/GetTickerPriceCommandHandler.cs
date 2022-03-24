using MediatR;
using Serilog;
using traderui.Server.Controllers;
using traderui.Server.IBKR;

namespace traderui.Server.Commands
{
    public class GetTickerPriceCommandHandler : IRequestHandler<GetTickerPriceCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;

        public GetTickerPriceCommandHandler(IInteractiveBrokers broker)
        {
            _broker = broker;
        }

        public Task<Unit> Handle(GetTickerPriceCommand request, CancellationToken cancellationToken)
        {
            _broker.GetTickerPrice(request.Symbol);
            return Task.FromResult(new Unit());
        }
    }
}
