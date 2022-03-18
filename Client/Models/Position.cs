using IBApi;

namespace traderui.Client.Models
{
    public class Position
    {
        public int PositionId { get; set; }
        public Contract Contract { get; set; } = new();
        public double Size { get; set; }
        public double AvgCost { get; set; }

        public double Daily { get; set; } = 0;
        public double Unrealized { get; set; } = 0;
        public double Realized { get; set; } = 0;
        public double Value { get; set; } = 0;
    }
}
