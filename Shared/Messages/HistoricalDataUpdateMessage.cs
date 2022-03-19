using IBApi;

namespace traderui.Shared.Events
{
    public class HistoricalDataUpdateMessage : BaseMessage
    {
        public Bar Bar { get; set; } = new(String.Empty, 0, 0, 0, 0, 0, 0, 0);
    }
}
