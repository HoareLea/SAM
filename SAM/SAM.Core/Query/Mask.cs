using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<bool> Mask(this IEnumerable<double> values, double value, NumberComparisonType numberComparisonType)
        {
            if (values == null)
                return null;

            List<bool> result = new List<bool>();
            foreach (double value_Temp in values)
                result.Add(Compare(value_Temp, value, numberComparisonType));

            return result;
        }

        public static List<bool> Mask(this IEnumerable<string> values, string value, TextComparisonType textComparisonType)
        {
            if (values == null)
                return null;

            List<bool> result = new List<bool>();
            foreach (string value_Temp in values)
                result.Add(Compare(value_Temp, value, textComparisonType));

            return result;
        }
    }
}