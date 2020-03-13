using System.Globalization;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Tolerance(int decimals)
        {
            if (decimals < 0)
                return double.NaN;

            if (decimals == 0)
                return 0;

            string value = "0" + CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            for (int i = 0; i < decimals; i++)
                value += "0";

            value += "1";

            return double.Parse(value);
        }
    }
}
