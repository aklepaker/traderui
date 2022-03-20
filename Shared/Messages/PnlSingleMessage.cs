namespace traderui.Shared.Events
{
    public class PnlSingleMessage : BaseMessage
    {
        public int Pos { get; set; }
        public double DailyPnl { get; set; }
        public double UnrealizedPnl { get; set; }
        public double RealizedPnl { get; set; }
        public double Value { get; set; }
    }
}
