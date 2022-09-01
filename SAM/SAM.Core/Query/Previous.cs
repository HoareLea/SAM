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
            if (values == null || index < 0)
            {
                return default(T);
            }

            int count = values.Count();
            if (count == 0)
            {
                return default(T);
            }


            int index_Temp = index;
            while (index_Temp >= count)
            {
                index_Temp -= count;
            }

            index_Temp = index_Temp == 0 ? count - 1 : index_Temp - 1;

            return values.ElementAt(index_Temp);
        }
    }
}