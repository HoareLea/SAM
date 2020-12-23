namespace SAM.Weather
{
    public static partial class Convert
    {
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