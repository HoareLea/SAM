using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double MeanAbsolutePercentageError(this List<double> m, List<double> r, double tolerance = Core.Tolerance.Distance)
        {
            if (m == null || r is null || m.Count != r.Count)
            {
                return double.NaN;
            }

            int count = m.Count;

            double sum = 0;
            int count_Temp = 0;
            for (int i = 0; i < count; i++)
            {
                double value = r[i];

                if (Math.Abs(value) < tolerance)
                {
                    continue;
                }

                sum += Math.Abs((m[i] - value) / value);
                count_Temp++;
            }

            return count_Temp > 0 ? (100.0 * sum / count_Temp) : double.NaN;
        }
    }
}