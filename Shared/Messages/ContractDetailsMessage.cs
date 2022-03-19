using IBApi;

namespace traderui.Shared.Events
{
    public class ContractDetailsMessage : BaseMessage
    {
        /// <summary>
        /// The contract details requested
        /// </summary>
        public ContractDetails ContractDetails { get; set; } = new ContractDetails();
    }
}
