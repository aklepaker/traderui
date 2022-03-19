namespace traderui.Shared.Events
{
    public class HistoricalDataEndMessage : BaseMessage
    {
        public string Start { get; set; } = String.Empty;
        public string End { get; set; } = String.Empty;
    }
}
