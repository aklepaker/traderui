namespace traderui.Shared.Events
{
    public class AccountSummaryMessage : BaseMessage
    {
        public string Account { get; set; }= String.Empty;
        public string Tag { get; set; }= String.Empty;
        public string Value { get; set; } = String.Empty;
        public string Currency { get; set; } = String.Empty;
    }
}
