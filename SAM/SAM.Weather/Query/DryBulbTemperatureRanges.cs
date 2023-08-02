using SAM.Core;
using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries.
    /// </summary>
    public static partial class Query
    {
        public static List<Range<double>> DryBulbTemperatureRanges(this WeatherDay weatherDay)
        {
            List<double> dryBulbTemperatures = weatherDay?.GetValues(WeatherDataType.DryBulbTemperature);
            if(dryBulbTemperatures == null)
            {
                return null;
            }

            List<Range<double>> result = new List<Range<double>>();
            foreach(double dryBulbTemeperature in dryBulbTemperatures)
            {
                result.Add(DryBulbTemperatureRange(dryBulbTemeperature));
            }
            return result;
        }
    }
}