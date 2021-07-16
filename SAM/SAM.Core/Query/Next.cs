using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Next(this IEnumerable<double> values, double value)
        {
            if (values == null)
                return double.NaN;

            double result = double.NaN;
            foreach (double value_Temp in values)
            {
                if (value_Temp <= value)
                {
                    continue;
                }
                
                if (double.IsNaN(result) || result > value_Temp)
                {
                    result = value_Temp;
                }
            }

            return result;
        }
    }
}