using MediatR;

namespace traderui.Server.Controllers
{
    public class GetTickerPriceCommand : IRequest<Unit>
    {
        public string Symbol { get; set; }
    }
}
