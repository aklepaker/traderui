using IBApi;

namespace traderui.Shared;

public enum OrderType
{
    MKT = 0,
    LMT,
    STP,
    STPLMT
}

public enum Action
{
    BUY = 0,
    SELL
}

public class WebOrder
{
    public Action Action { get; set; } // TODO: Enum - BUY / SELL
    public OrderType OrderType { get; set; } // TODO: Enum - Type of order LMT, MKT etc.
    public Contract Contract { get; set; } = new Contract();
    public double Price { get; set; }
    public double LmtPrice { get; set; }
    public double Qty { get; set; }
    public bool StopLoss { get; set; } = true;
    public double StopLossAt { get; set; }
    public bool Transmit { get; set; } = false;
}
