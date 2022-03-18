using IBApi;

namespace traderui.Client.Models
{
    public class ADR
    {
        public double AverageDailyRange { get; private set; } = 0;
        public List<Bar> HistoricalData { get; private set; } = new List<Bar>();

        public void AddHistoricalData(Bar bar)
        {
            HistoricalData.Add(bar);
        }

        public void Clear()
        {
            HistoricalData = new List<Bar>();
            AverageDailyRange = 0;
        }

        public void CalculateDailyRange()
        {
            double SumDailyRange = 0;
            foreach (var bar in HistoricalData)
            {
                var DailyRange = bar.High / bar.Low;
                SumDailyRange += (100 * (DailyRange - 1));
            }

            AverageDailyRange = SumDailyRange / HistoricalData.Count;
        }
    }
}
