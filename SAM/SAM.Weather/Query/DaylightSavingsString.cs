namespace SAM.Weather
{
    public static partial class Query
    {
        public static string DaylightSavingsString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string[] values = new string[] {
                "HOLIDAYS/DAYLIGHT SAVINGS",
                ",No,0,0,0"
            };

            return string.Join(",", values);
        }
    }
}