using MediatR;

namespace traderui.Server.Controllers
{
    public class GetPnLCommand: IRequest<Unit>
    {
        public string Account { get; set; }
        public int ContractId { get; set; }
        public bool Active { get; set; }
    }
}
