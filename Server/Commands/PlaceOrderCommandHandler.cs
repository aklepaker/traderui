using AutoMapper;
using MediatR;
using traderui.Server.IBKR;
using traderui.Shared;

namespace traderui.Server.Commands
{
    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Unit>
    {
        private readonly IInteractiveBrokers _broker;
        private readonly IMapper _mapper;

        public PlaceOrderCommandHandler(IInteractiveBrokers broker, IMapper mapper)
        {
            _broker = broker;
            _mapper = mapper;
        }

        public Task<Unit> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            var order = _mapper.Map<WebOrder>(request);
            _broker.PlaceOrder(order);
            return Task.FromResult(Unit.Value);
        }
    }
}
