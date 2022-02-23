using System;
using System.Collections.Generic;
using SAM.Weather;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static DesignDay DesignDay(this WeatherData weatherData)
        {
            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if(weatherYears == null || weatherYears.Count == 0)
            {
                return null;
            }

            WeatherDay weatherDay = null;
            int dayIndex = -1;

            List<Tuple<int, WeatherDay>> tuples = new List<Tuple<int, WeatherDay>>();
            foreach(WeatherYear weatherYear in weatherYears)
            {
                weatherDay = Weather.Query.DesignWeatherDay(weatherYear.WeatherDays, out dayIndex);
                if(weatherDay == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<int, WeatherDay>(dayIndex, weatherDay));
            }

            int yearIndex = -1;

            weatherDay = Weather.Query.DesignWeatherDay(tuples.ConvertAll(x => x.Item2), out yearIndex);
            dayIndex = tuples[yearIndex].Item1;

            string name = null;
            if(!weatherData.TryGetValue(WeatherDataParameter.City, out name) || string.IsNullOrWhiteSpace(name))
            {
                name = weatherData.Name;
            }

            int year = weatherYears[yearIndex].Year;
            DateTime dateTime = new DateTime(year, 1, 1);
            dateTime.AddDays(dayIndex);

            return DesignDay(name, (short)dateTime.Year,  (byte)dateTime.Month, (byte)dateTime.Day, weatherDay);
        }

        public static DesignDay DesignDay(string name, short year, byte month, byte day, WeatherDay weatherDay)
        {
            if(year < 0)
            {
                return null;
            }

            if(month < 1 || month > 12)
            {
                return null;
            }

            DesignDay result = new DesignDay(name, year, month, day);
            
            if(weatherDay != null)
            {
                foreach (string key in weatherDay.Keys)
                {
                    result[key] = weatherDay[key];
                }
            }

            return result;
        }
    }
}