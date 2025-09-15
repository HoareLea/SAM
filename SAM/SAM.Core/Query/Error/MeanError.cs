using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double MeanError(this List<double> m, List<double> r)
        {
            if(m == null || r is null || m.Count != r.Count)
            {
                return double.NaN;
            }

            int count = m.Count;

            double sum = 0;
            for (int i = 0; i < count; i++)
            {
                sum += m[i] - r[i];
            }

            return sum / count;
        }
    }
}