namespace SAM.Weather
{
    /// <summary>
    /// The Query class provides static methods to query and manipulate WeatherData objects.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Generates a string representation of typical and extreme periods for the given WeatherData.
        /// </summary>
        /// <param name="weatherData">The WeatherData object for which to generate the string.</param>
        /// <returns>A string representation of typical and extreme periods, or null if weatherData is null.</returns>
        public static string TypicalPeriodsString(this WeatherData weatherData)
        {
            // If the WeatherData object is null, return null.
            if (weatherData == null)
                return null;

            // The values array contains the string values representing typical and extreme periods.
            // Each element in the array follows the format:
            // "index,description,type,startDate,endDate"
            // where index is a number, description is a string describing the period,
            // type is either "Typical" or "Extreme", and startDate and endDate are date strings.
            string[] values = new string[] {
                "TYPICAL/EXTREME PERIODS",
                "6,Summer - Week Nearest Max Temperature For Period,Extreme,1/ 8,1/14,Summer - Week Nearest Average Temperature For Period,Typical,2/12,2/18,Winter - Week Nearest Min Temperature For Period,Extreme,7/29,8/ 4,Winter - Week Nearest Average Temperature For Period,Typical,8/ 5,8/11,Autumn - Week Nearest Average Temperature For Period,Typical,5/27,6/ 2,Spring - Week Nearest Average Temperature For Period,Typical,11/12,11/18"
            };

            // Join the values array elements with commas and return the resulting string.
            return string.Join(",", values);
        }
    }
}
