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
            if (weatherDay == null)
            {
                return null;
            }

            List<double> dryBulbTemperatures = weatherDay?.GetValues(WeatherDataType.DryBulbTemperature);
            if (dryBulbTemperatures == null)
            {
                return null;
            }

            List<Range<double>> result = new List<Range<double>>();
            foreach (double dryBulbTemeperature in dryBulbTemperatures)
            {
                result.Add(DryBulbTemperatureRange(dryBulbTemeperature));
            }
            return result;
        }

        public static List<Range<double>> DryBulbTemperatureRanges(this WeatherYear weatherYear)
        {
            List<WeatherDay> weatherDays = weatherYear?.WeatherDays;
            if (weatherDays == null)
            {
                return null;
            }

            List<Range<double>> result = new List<Range<double>>();
            foreach (WeatherDay weatherDay in weatherDays)
            {
                List<Range<double>> ranges = DryBulbTemperatureRanges(weatherDay);
                if(ranges != null)
                {
                    result.AddRange(ranges);
                }
            }

            return result;
        }

        public static List<Range<double>> DryBulbTemperatureRanges(this WeatherData weatherData)
        {
            List<WeatherDay> weatherDays = weatherData?.WeatherDays();
            if (weatherDays == null)
            {
                return null;
            }

            List<Range<double>> result = new List<Range<double>>();
            foreach (WeatherDay weatherDay in weatherDays)
            {
                List<Range<double>> ranges = DryBulbTemperatureRanges(weatherDay);
                if (ranges != null)
                {
                    result.AddRange(ranges);
                }
            }

            return result;
        }
    }
}