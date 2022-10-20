using System.Collections.Generic;
using System.Linq;

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

        public static T Next<T>(this IEnumerable<T> values, int index)
        {
            if (values == null)
            {
                return default(T);
            }
            
            int index_Temp = Next(values.Count(), index);
            if(index_Temp == -1)
            {
                return default(T);
            }

            return values.ElementAt(index_Temp);
        }

        public static int Next(this int count, int index)
        {
            if (index <= 0)
            {
                return -1;
            }

            if (count == 0)
            {
                return -1;
            }

            int result = index + 1;
            while (result >= count)
            {
                result -= count;
            }

            return result;
        }
    }
}