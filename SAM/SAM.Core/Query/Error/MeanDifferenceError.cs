using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double MeanDifferenceError(this List<double> m, List<double> r, out double lower, out double upper)
        {
            lower = double.NaN;
            upper = double.NaN;
            if (m == null || r is null || m.Count != r.Count)
            {
                return double.NaN;
            }

            int count = m.Count;

            List<double> differences = [];
            for (int i = 0; i < count; i++)
            {
                differences.Add(m[i] - r[i]);
            }

            double mean = differences.Average();

            double sum = 0;
            foreach(double difference in differences)
            {
                sum += Math.Pow(difference - mean, 2);
            }

            double result = Math.Sqrt(sum / count);

            lower = mean - 1.96 * result;
            upper = mean + 1.96 * result;

            return result;
        }
    }
}