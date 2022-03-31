using IBApi;

namespace traderui.Shared.Requests
{
    public class PlaceOrderRequest
    {
        public MarketAction Action { get; set; } // TODO: Enum - BUY / SELL
        public OrderType OrderType { get; set; } // TODO: Enum - Type of order LMT, MKT etc.
        // public Contract Contract { get; set; } = new Contract();
        public double Price { get; set; }
        public double LmtPrice { get; set; }
        public double Qty { get; set; }
        public bool StopLoss { get; set; } = true;
        public double StopLossAt { get; set; }
        public bool Transmit { get; set; } = false;
        public ContractDetails ContractDetails { get; set; } = new();
        public bool TakeProfitAndUpdateSellorder { get; set; } = false;
        public double TakeProfitAt { get; set; }
    }
}
