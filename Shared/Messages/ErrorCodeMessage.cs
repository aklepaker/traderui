namespace traderui.Shared.Events
{
    public class ErrorCodeMessage : BaseMessage
    {
        public int Id { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = String.Empty;
    }
}
