using IBApi;

namespace traderui.Shared.Events
{
    public class PositionMessage : BaseMessage
    {
        public string Account { get; set; } = String.Empty;
        public Contract Contract { get; set; } = new();
        public double Pos { get; set; }
        public double AvgCost { get; set; }
    }
}
