namespace traderui.Shared.Events
{
    public class PnlMessage : BaseMessage
    {
        public double DailyPnl { get; set; }
        public double UnrealizedPnl { get; set; }
        public double RealizedPnl { get; set; }
    }
}
