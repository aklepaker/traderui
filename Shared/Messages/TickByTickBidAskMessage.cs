using IBApi;

namespace traderui.Shared.Events
{
    public class TickByTickBidAskMessage : BaseMessage
    {
        public long Time { get; set; }
        public double BidPrice { get; set; }
        public double AskPrice { get; set; }
        public int BidSize { get; set; }
        public int AskSize { get; set; }
        public TickAttribBidAsk TickAttribBidAsk { get; set; } = new();
    }
}
