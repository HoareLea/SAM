using System.Linq;

namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Gets the maximum value of the specified weather data type from the given WeatherDay object.
        /// </summary>
        /// <param name="weatherDay">The WeatherDay object to get the maximum value from.</param>
        /// <param name="weatherDataType">The type of weather data to get the maximum value of.</param>
        /// <returns>The maximum value of the specified weather data type from the given WeatherDay object.</returns>
        public static double MaxValue(this WeatherDay weatherDay, WeatherDataType weatherDataType)
        {
            if (weatherDay == null)
            {
                return double.NaN;
            }

            if (!weatherDay.Contains(weatherDataType))
            {
                return double.NaN;
            }

            return weatherDay.GetValues(weatherDataType).Max();
        }
    }
}