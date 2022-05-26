using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static Dictionary<DateTime, double> Values(this WeatherData weatherData, string name)
        {
            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if (weatherYears == null)
            {
                return null;
            }

            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();
            foreach (WeatherYear weatherYear in weatherYears)
            {
                int year = weatherYear.Year;
                List<WeatherDay> weatherDays = weatherYear.WeatherDays;
                if (weatherDays == null)
                {
                    continue;
                }

                DateTime dateTime_Year = new DateTime(year, 1, 1);
                for (int i = 0; i < weatherDays.Count; i++)
                {
                    DateTime dateTime_Day = dateTime_Year.AddDays(i);
                    WeatherDay weatherDay = weatherDays[i];
                    for (int j = 0; j < 23; j++)
                    {
                        if (!TryGetValue(weatherDay, name, j, out double value))
                        {
                            continue;
                        }

                        DateTime dateTime_Hour = dateTime_Day.AddHours(j);

                        result[dateTime_Hour] = value;
                    }
                }
            }

            return result;
        }

        public static Dictionary<DateTime, double> Values(this WeatherData weatherData, WeatherDataType weatherDataType)
        {
            return Values(weatherData, weatherDataType.ToString());
        }

        
    }
}