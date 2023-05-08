namespace SAM.Weather
{
    /// <summary>
    /// Provides additional methods for converting weather data between different formats.
    /// </summary>
    public static partial class Convert
    {
        /// <summary>
        /// Converts a WeatherData object to its EPW file representation as a string.
        /// </summary>
        /// <param name="weatherData">The WeatherData object to be converted.</param>
        /// <returns>A string representing the EPW file format of the input WeatherData, or null if the conversion fails.</returns>
        public static string ToEPW(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string locationString = weatherData.LocationString();
            string designConditionsString = weatherData.DesignConditionsString();
            string typicalPeriodsString = weatherData.TypicalPeriodsString();
            string groundTemperaturesString = weatherData.GroundTemperaturesString();
            string daylightSavingsString = weatherData.DaylightSavingsString();
            string commentsString = weatherData.CommentsString();
            string dataPeriods = weatherData.DataPeriodsString();
            string dataString = weatherData.DataString();

            string[] values = new string[]
            {
                locationString == null ? string.Empty : locationString,
                designConditionsString == null ? string.Empty : designConditionsString,
                typicalPeriodsString == null ? string.Empty : typicalPeriodsString,
                groundTemperaturesString == null ? string.Empty : groundTemperaturesString,
                daylightSavingsString == null ? string.Empty : daylightSavingsString,
                commentsString == null ? string.Empty : commentsString,
                dataPeriods == null ? string.Empty : dataPeriods,
                dataString == null ? string.Empty : dataString
            };


            return string.Join("\n", values);
        }
    }
}