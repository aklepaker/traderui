using MediatR;

namespace traderui.Server.Commands
{
    public class GetTickerCommand : IRequest<Unit>
    {
        public string Symbol { get; set; }
        public string ConnectionId { get; set; }
    }
}
