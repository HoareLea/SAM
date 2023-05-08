using System.Collections.Generic;
using System.Linq;


namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Gets the cooling design weather day from the given weather data.
        /// </summary>
        /// <param name="weatherData">The weather data.</param>
        /// <returns>The cooling design weather day.</returns>
        public static WeatherDay CoolingDesignWeatherDay(this WeatherData weatherData)
        {
            return CoolingDesignWeatherDay(weatherData?.WeatherDays());
        }

        /// <summary>
        /// Gets the cooling design weather day from the given weather year.
        /// </summary>
        /// <param name="weatherYear">The weather year.</param>
        /// <returns>The cooling design weather day.</returns>
        public static WeatherDay CoolingDesignWeatherDay(this WeatherYear weatherYear)
        {
            return CoolingDesignWeatherDay(weatherYear?.WeatherDays);
        }

        /// <summary>
        /// Gets the WeatherDay with the highest DryBulbTemperature and highest SolarRadiation from the given IEnumerable of WeatherDays.
        /// </summary>
        /// <param name="weatherDays">The IEnumerable of WeatherDays to search.</param>
        /// <param name="dayIndex">The index of the WeatherDay in the IEnumerable.</param>
        /// <returns>The WeatherDay with the highest DryBulbTemperature and highest SolarRadiation.</returns>
        public static WeatherDay CoolingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays, out int dayIndex)
        {
            //Initialize dayIndex to -1
            dayIndex = -1;

            //Check if weatherDays is null or empty
            if (weatherDays == null || weatherDays.Count() == 0)
            {
                return null;
            }

            //Initialize max values for dryBulbTemperature and solarRadiation
            double dryBulbTemperature_Max = double.MinValue;
            double solarRadiation_Max = double.MinValue;
            WeatherDay weatherDay_DryBulbTemperature = null;
            WeatherDay weatherDay_SolarRadiation = null;

            //Loop through weatherDays
            for (int i = 0; i < weatherDays.Count(); i++)
            {
                WeatherDay weatherDay = weatherDays.ElementAt(i);

                //Check if weatherDay is null
                if (weatherDay == null)
                {
                    continue;
                }

                //Loop through 24 hours
                for (int j = 0; j < 24; j++)
                {
                    //Check if dryBulbTemperature is greater than max value
                    if (weatherDay.TryGetValue(WeatherDataType.DryBulbTemperature, j, out double dryBulbTemperature) && dryBulbTemperature > dryBulbTemperature_Max)
                    {
                        //Set weatherDay_DryBulbTemperature to weatherDay
                        weatherDay_DryBulbTemperature = weatherDay;
                        //Set dryBulbTemperature_Max to dryBulbTemperature
                        dryBulbTemperature_Max = dryBulbTemperature;
                        //Set dayIndex to i
                        dayIndex = i;
                    }

                    //Calculate solarRadiation
                    double solarRadiation = weatherDay.CalculatedGlobalRadiation(j);
                    //Check if solarRadiation is greater than max value
                    if (!double.IsNaN(solarRadiation) && solarRadiation > solarRadiation_Max)
                    {
                        //Set weatherDay_SolarRadiation to weatherDay
                        weatherDay_SolarRadiation = weatherDay;
                        //Set solarRadiation_Max to solarRadiation
                        solarRadiation_Max = solarRadiation;
                    }
                }
            }

            //Check if weatherDay_DryBulbTemperature is null
            if (weatherDay_DryBulbTemperature == null)
            {
                return null;
            }

            //Create new WeatherDay object
            WeatherDay result = new WeatherDay(weatherDay_DryBulbTemperature);
            //Check if weatherDay_SolarRadiation is not null
            if (weatherDay_SolarRadiation != null)
            {
                //Loop through 24 hours
                for (int i = 0; i < 24; i++)
                {
                    //Check if weatherDay_SolarRadiation contains DiffuseSolarRadiation
                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.DiffuseSolarRadiation))
                    {
                        //Set result DiffuseSolarRadiation to weatherDay_SolarRadiation DiffuseSolarRadiation
                        result[WeatherDataType.DiffuseSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.DiffuseSolarRadiation][i];
                    }

                    //Check if weatherDay_SolarRadiation contains DirectSolarRadiation
                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.DirectSolarRadiation))
                    {
                        //Set result DirectSolarRadiation to weatherDay_SolarRadiation DirectSolarRadiation
                        result[WeatherDataType.DirectSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.DirectSolarRadiation][i];
                    }

                    //Check if weatherDay_SolarRadiation contains GlobalSolarRadiation
                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.GlobalSolarRadiation))
                    {
                        //Set result GlobalSolarRadiation to weatherDay_SolarRadiation GlobalSolarRadiation
                        result[WeatherDataType.GlobalSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.GlobalSolarRadiation][i];
                    }
                }
            }

            //Return result
            return result;

        }

        /// <summary>
        /// Gets the cooling design weather day from the given weather days.
        /// </summary>
        /// <param name="weatherDays">The weather days.</param>
        /// <returns>The cooling design weather day.</returns>
        public static WeatherDay CoolingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays)
        {
            return CoolingDesignWeatherDay(weatherDays, out int dayIndex);
        }

    }

}