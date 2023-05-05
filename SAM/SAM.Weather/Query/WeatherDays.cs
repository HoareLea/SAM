using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// Contains methods for querying weather data.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Retrieves a list of WeatherDay objects from the provided WeatherData object.
        /// </summary>
        /// <param name="weatherData">The WeatherData object containing weather information.</param>
        /// <returns>
        /// A list of WeatherDay objects if the provided WeatherData object is valid and has data;
        /// otherwise, returns null.
        /// </returns>
        public static List<WeatherDay> WeatherDays(this WeatherData weatherData)
        {
            // If the input weatherData is null, return null
            if (weatherData == null)
                return null;

            // Get the list of WeatherYear objects from the input weatherData
            List<WeatherYear> weatherYears = weatherData?.WeatherYears;

            // If the weatherYears list is null or empty, return null
            if (weatherYears == null || weatherYears.Count == 0)
            {
                return null;
            }

            // Initialize the result list to store WeatherDay objects
            List<WeatherDay> result = new List<WeatherDay>();

            // Loop through each WeatherYear object in the weatherYears list
            foreach (WeatherYear weatherYear in weatherYears)
            {
                // Get the list of WeatherDay objects from the current weatherYear
                List<WeatherDay> weatherDays_Temp = weatherYear?.WeatherDays;

                // If the weatherDays_Temp list is null or empty, continue to the next iteration
                if (weatherDays_Temp == null || weatherDays_Temp.Count == 0)
                {
                    continue;
                }

                // Add all WeatherDay objects from the current weatherYear to the result list
                result.AddRange(weatherDays_Temp);
            }

            // Return the result list containing all WeatherDay objects
            return result;
        }
    }
}
