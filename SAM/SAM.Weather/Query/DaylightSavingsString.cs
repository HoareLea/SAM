namespace SAM.Weather
{
    /// <summary>
    /// Contains the partial class Query for the WeatherData type.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Generates a comma-separated string with daylight savings and holidays information from the provided WeatherData object.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to extract information from.</param>
        /// <returns>A comma-separated string with daylight savings and holidays information, or null if the input object is null.</returns>
        public static string DaylightSavingsString(this WeatherData weatherData)
        {
            // Return null if the input WeatherData object is null
            if (weatherData == null)
                return null;

            // Define the values for the comma-separated string
            string[] values = new string[] {
                "HOLIDAYS/DAYLIGHT SAVINGS",
                ",No,0,0,0"
            };

            // Join the values array into a single string separated by commas
            return string.Join(",", values);
        }
    }
}
