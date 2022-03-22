using MediatR;

namespace traderui.Server.Controllers
{
    public class GetHistoricDataCommand: IRequest<Unit>
    {
        public string Symbol { get; set; }
    }
}
