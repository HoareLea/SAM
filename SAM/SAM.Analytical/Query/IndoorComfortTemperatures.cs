using SAM.Core;
using SAM.Weather;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<double> IndoorComfortTemperatures(this WeatherYear weatherYear, TM52BuildingCategory tM52BuildingCategory, double acceptableTemperatureDifference = double.NaN)
        {
            if(weatherYear == null || tM52BuildingCategory == TM52BuildingCategory.Undefined)
            {
                return null;
            }

            if (double.IsNaN(acceptableTemperatureDifference))
            {
                Range<double> temperatureRange = tM52BuildingCategory.TemperatureRange();
                if (temperatureRange == null)
                {
                    return null;
                }

                acceptableTemperatureDifference = temperatureRange.Max;
            }

            List<double> runningMeanDryBulbTemperatures = weatherYear.RunningMeanDryBulbTemperatures();
            if(runningMeanDryBulbTemperatures == null || runningMeanDryBulbTemperatures.Count == 0)
            {
                return null;
            }

            double factor = 18.8 + acceptableTemperatureDifference;

            return runningMeanDryBulbTemperatures?.ConvertAll(x => (0.33 * x) + factor);
        }
    }
}