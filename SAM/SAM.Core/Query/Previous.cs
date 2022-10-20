using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Previous(this IEnumerable<double> values, double value)
        {
            if (values == null)
                return double.NaN;

            double result = double.NaN;
            foreach (double value_Temp in values)
            {
                if (value_Temp >= value)
                {
                    continue;
                }
                
                if (double.IsNaN(result) || result < value_Temp)
                {
                    result = value_Temp;
                }
            }

            return result;
        }

        public static T Previous<T>(this IEnumerable<T> values, int index)
        {
            if (values == null)
            {
                return default(T);
            }

            int index_Temp = Previous(values.Count(), index);
            if(index_Temp == -1)
            {
                return default(T);
            }

            return values.ElementAt(index_Temp);
        }

        public static int Previous(this int count, int index)
        {
            if (index < 0)
            {
                return -1;
            }

            if (count == 0)
            {
                return -1;
            }

            int result = index;
            while (result >= count)
            {
                result -= count;
            }

            result = result == 0 ? count - 1 : result - 1;

            return result;
        }
    }
}