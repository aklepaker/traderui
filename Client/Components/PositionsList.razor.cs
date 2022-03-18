using Microsoft.AspNetCore.Components;
using traderui.Client.Models;

namespace traderui.Client.Components
{
    public partial class PositionsList
    {
        [Parameter] public List<Position> Positions { get; set; } = new();
        [Parameter] public double DailyPnL { get; set; }
        [Parameter] public double UnrealizedPnL { get; set; }
        [Parameter] public double RealizedPnL { get; set; }
    }
}
