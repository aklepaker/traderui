using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace traderui.Server.Hubs
{
    public class BrokerHubService
    {
        private readonly IHubContext<BrokerHub> _hubContext;
        public List<string> ListOfSymbolGroups { get; set; } = new List<string>();

        public BrokerHubService(IHubContext<BrokerHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void AddToGroup(string connectionId, string groupName)
        {
            Log.Information("Adding {ConnectionId} to {groupName} notifications", connectionId, groupName);
            var name = groupName.ToUpper();

            ListOfSymbolGroups.ForEach(g => _hubContext.Groups.RemoveFromGroupAsync(connectionId, g));
            if (!ListOfSymbolGroups.Any(g => g.Equals(name)))
            {
                ListOfSymbolGroups.Add(name);
            }

            _hubContext.Groups.AddToGroupAsync(connectionId, name);
        }

        public void RemoveFromGroup(string connectionId)
        {
            Log.Information("Removing {ConnectionId} from all notifications", connectionId);
            ListOfSymbolGroups.ForEach(g => _hubContext.Groups.RemoveFromGroupAsync(connectionId, g));
        }
    }
}
