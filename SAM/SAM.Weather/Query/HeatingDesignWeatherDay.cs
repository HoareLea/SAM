using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries against a database.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Gets the heating design weather day from the given weather data.
        /// </summary>
        /// <param name="weatherData">The weather data.</param>
        /// <returns>The heating design weather day.</returns>
        public static WeatherDay HeatingDesignWeatherDay(this WeatherData weatherData)
        {
            return HeatingDesignWeatherDay(weatherData?.WeatherDays());
        }

        /// <summary>
        /// Gets the heating design weather day from a given weather year.
        /// </summary>
        /// <param name="weatherYear">The weather year.</param>
        /// <returns>The heating design weather day.</returns>
        public static WeatherDay HeatingDesignWeatherDay(this WeatherYear weatherYear)
        {
            return HeatingDesignWeatherDay(weatherYear?.WeatherDays);
        }

        /// <summary>
        /// Gets the heating design weather day from the given weather days.
        /// </summary>
        /// <param name="weatherDays">The weather days.</param>
        /// <param name="dayIndex">Index of the day.</param>
        /// <returns>The heating design weather day.</returns>
        public static WeatherDay HeatingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays, out int dayIndex)
        {
            dayIndex = -1;

            if (weatherDays == null || weatherDays.Count() == 0)
            {
                return null;
            }

            double dryBulbTemperature_Min = double.MaxValue;
            double windSpeed = double.MaxValue;
            double relativeHumidity = double.MaxValue;
            WeatherDay weatherDay_DryBulbTemperature = null;

            for (int i = 0; i < weatherDays.Count(); i++)
            {
                WeatherDay weatherDay = weatherDays.ElementAt(i);
                if (weatherDay == null)
                {
                    continue;
                }

                for (int j = 0; j < 24; j++)
                {
                    if (weatherDay.TryGetValue(WeatherDataType.DryBulbTemperature, j, out double dryBulbTemperature) && dryBulbTemperature < dryBulbTemperature_Min)
                    {
                        weatherDay_DryBulbTemperature = weatherDay;
                        dryBulbTemperature_Min = dryBulbTemperature;
                        dayIndex = i;

                        weatherDay.TryGetValue(WeatherDataType.WindSpeed, j, out windSpeed);
                        weatherDay.TryGetValue(WeatherDataType.RelativeHumidity, j, out relativeHumidity);
                    }
                }
            }

            if (weatherDay_DryBulbTemperature == null)
            {
                return null;
            }

            WeatherDay result = new WeatherDay(weatherDay_DryBulbTemperature);

            for (int i = 0; i < 24; i++)
            {
                result[WeatherDataType.DryBulbTemperature, i] = dryBulbTemperature_Min;
                result[WeatherDataType.WindSpeed, i] = windSpeed;
                result[WeatherDataType.GlobalSolarRadiation, i] = 0;
                result[WeatherDataType.DiffuseSolarRadiation, i] = 0;
                result[WeatherDataType.CloudCover, i] = 0;
                result[WeatherDataType.RelativeHumidity, i] = relativeHumidity;
                result[WeatherDataType.WindDirection, i] = 0;
            }

            return result;
        }

        /// <summary>
        /// Gets the heating design weather day from the given weather days.
        /// </summary>
        /// <param name="weatherDays">The weather days.</param>
        /// <returns>The heating design weather day.</returns>
        public static WeatherDay HeatingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays)
        {
            return HeatingDesignWeatherDay(weatherDays, out int dayIndex);
        }
    }
}