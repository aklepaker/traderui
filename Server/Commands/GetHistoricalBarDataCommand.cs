using MediatR;

namespace traderui.Server.Controllers
{
    public class GetHistoricalBarDataCommand: IRequest<Unit>
    {
        public string Symbol { get; set; }
        public int RequestId { get; set; }
        public string ConnectionId { get; set; }
    }
}
