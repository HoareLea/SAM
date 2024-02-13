using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static double ApproximateRunningMeanDryBulbTemperature(IEnumerable<double> dailyMeanDryBulbOutdoorTemperatures, int dayIndex)
        {
            if (dayIndex == -1 || dailyMeanDryBulbOutdoorTemperatures == null || dailyMeanDryBulbOutdoorTemperatures.Count() == 0)
            {
                return double.NaN;
            }

            int count_Temp = dailyMeanDryBulbOutdoorTemperatures.Count();

            double[] factors = new double[] { 1, 0.8, 0.6, 0.5, 0.4, 0.3, 0.2};

            List<double> values = new List<double>();
            for (int i = 1; i <= 7; i++)
            {
                int index = dayIndex - i;
                int boundedIndex = Core.Query.BoundedIndex(count_Temp, index);

                double dryBulbTemperature = dailyMeanDryBulbOutdoorTemperatures.ElementAt(boundedIndex);

                values.Add(dryBulbTemperature * factors[i - 1]);
            }

            return values.Sum() / 3.8;
        }
    }
}