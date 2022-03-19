using IBApi;

namespace traderui.Shared.Events
{
    public class TickPriceMessage : BaseMessage
    {
        public int TickerId { get; set; }
        public int Field { get; set; }
        public double Price { get; set; }
        public TickAttrib TickerAttrib { get; set; } = new();
    }
}
