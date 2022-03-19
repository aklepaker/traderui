using IBApi;

namespace traderui.Shared.Events
{
    public class OpenOrderMessage : BaseMessage
    {
        public Contract Contract { get; set; } = new();
        public Order Order { get; set; } = new();
        public OrderState OrderState { get; set; } = new();
    }
}
