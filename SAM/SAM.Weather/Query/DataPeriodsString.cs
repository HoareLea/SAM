namespace SAM.Weather
{
    /// <summary>
    /// Contains the partial class Query for the WeatherData type.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Generates a comma-separated string with data periods information from the provided WeatherData object.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to extract information from.</param>
        /// <returns>A comma-separated string with data periods information, or null if the input object is null.</returns>
        public static string DataPeriodsString(this WeatherData weatherData)
        {
            // Return null if the input WeatherData object is null
            if (weatherData == null)
                return null;

            // Define the values for the comma-separated string
            string[] values = new string[] {
                "DATA PERIODS",
                "1,1,Data,Sunday,1/ 1,12/31"
            };

            // Join the values array into a single string separated by commas
            return string.Join(",", values);
        }
    }
}
