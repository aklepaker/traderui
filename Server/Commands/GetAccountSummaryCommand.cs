using MediatR;

namespace traderui.Server.Controllers
{
    public class GetAccountSummaryCommand: IRequest<Unit>
    {
        public bool StopRequest { get; set; }
    }
}
