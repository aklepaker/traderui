using System.Globalization;

namespace traderui.Client.Shared
{
    public static class Helper
    {
        public static string CurrencyFormat(double value)
        {
            return "$" + value.ToString("#.00", CultureInfo.InvariantCulture);
        }

        public static string TwoDecimal(double value)
        {
            return value.ToString("#.00", CultureInfo.InvariantCulture);
        }

        public static string Percent(double value)
        {
            return value.ToString("#.00", CultureInfo.InvariantCulture);
        }
    }
}
