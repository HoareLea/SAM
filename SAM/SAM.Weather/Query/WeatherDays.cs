using System.Collections.Generic;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static List<WeatherDay> WeatherDays(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if (weatherYears == null || weatherYears.Count == 0)
            {
                return null;
            }


            List<WeatherDay> result = new List<WeatherDay>();
            foreach (WeatherYear weatherYear in weatherYears)
            {
                List<WeatherDay> weatherDays_Temp = weatherYear?.WeatherDays;
                if(weatherDays_Temp == null || weatherDays_Temp.Count == 0)
                {
                    continue;
                }

                result.AddRange(weatherDays_Temp);
            }

            return result;
        }
    }
}