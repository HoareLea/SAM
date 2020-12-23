namespace SAM.Weather
{
    public static partial class Query
    {
        public static string TypicalPeriodsString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            string[] values = new string[] {
                "TYPICAL/EXTREME PERIODS",
                "6,Summer - Week Nearest Max Temperature For Period,Extreme,1/ 8,1/14,Summer - Week Nearest Average Temperature For Period,Typical,2/12,2/18,Winter - Week Nearest Min Temperature For Period,Extreme,7/29,8/ 4,Winter - Week Nearest Average Temperature For Period,Typical,8/ 5,8/11,Autumn - Week Nearest Average Temperature For Period,Typical,5/27,6/ 2,Spring - Week Nearest Average Temperature For Period,Typical,11/12,11/18"
            };

            return string.Join(",", values);
        }
    }
}