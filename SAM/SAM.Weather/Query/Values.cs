using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// Contains methods for querying WeatherData.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Retrieves the values of the specified weather variable as a dictionary with DateTime keys.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to extract values from.</param>
        /// <param name="name">The name of the weather variable.</param>
        /// <returns>A dictionary containing DateTime keys and corresponding weather variable values as doubles.</returns>
        public static Dictionary<DateTime, double> Values(this WeatherData weatherData, string name)
        {
            // Get the list of WeatherYear objects
            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if (weatherYears == null)
            {
                return null;
            }

            // Initialize the result dictionary
            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();

            // Iterate through each WeatherYear object
            foreach (WeatherYear weatherYear in weatherYears)
            {
                int year = weatherYear.Year;
                List<WeatherDay> weatherDays = weatherYear.WeatherDays;
                if (weatherDays == null)
                {
                    continue;
                }

                // Calculate the DateTime for the current year
                DateTime dateTime_Year = new DateTime(year, 1, 1);

                // Iterate through each WeatherDay object
                for (int i = 0; i < weatherDays.Count; i++)
                {
                    DateTime dateTime_Day = dateTime_Year.AddDays(i);
                    WeatherDay weatherDay = weatherDays[i];

                    // Iterate through each hour of the day (0-22)
                    for (int j = 0; j < 23; j++)
                    {
                        // Get the value for the specified weather variable at the current hour
                        if (!TryGetValue(weatherDay, name, j, out double value))
                        {
                            continue;
                        }

                        // Calculate the DateTime for the current hour
                        DateTime dateTime_Hour = dateTime_Day.AddHours(j);

                        // Add the value to the result dictionary
                        result[dateTime_Hour] = value;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the values of the specified WeatherDataType as a dictionary with DateTime keys.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to extract values from.</param>
        /// <param name="weatherDataType">The WeatherDataType to extract values for.</param>
        /// <returns>A dictionary containing DateTime keys and corresponding weather variable values as doubles.</returns>
        public static Dictionary<DateTime, double> Values(this WeatherData weatherData, WeatherDataType weatherDataType)
        {
            return Values(weatherData, weatherDataType.ToString());
        }

        // Additional code
    }
}
