using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double RootMeanSquaredError(this List<double> m, List<double> r)
        {
            if (m == null || r is null || m.Count != r.Count)
            {
                return double.NaN;
            }

            int count = m.Count;

            double sum = 0;
            for (int i = 0; i < count; i++)
            {
                double value = r[i];

                sum += Math.Pow(m[i] - r[i], 2);
            }

            return Math.Sqrt(sum / count);
        }
    }
}