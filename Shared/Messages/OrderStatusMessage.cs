namespace traderui.Shared.Events
{
    public class OrderStatusMessage : BaseMessage
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = String.Empty;
        public double Filled { get; set; }
        public double Remaining { get; set; }
        public double AvgFillPrice { get; set; }
        public int PermId { get; set; }
        public int ParentId { get; set; }
        public double LastFillPrice { get; set; }
        public int ClientId { get; set; }
        public string WhyHeld { get; set; }= String.Empty;
        public double MarketCapPrice { get; set; }
    }
}
