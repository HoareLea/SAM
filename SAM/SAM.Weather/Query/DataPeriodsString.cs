namespace SAM.Weather
{
    public static partial class Query
    {
        public static string DataPeriodsString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string[] values = new string[] {
                "DATA PERIODS",
                "1,1,Data,Sunday,1/ 1,12/31"
            };

            return string.Join(",", values);
        }
    }
}