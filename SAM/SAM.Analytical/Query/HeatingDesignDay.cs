using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static DesignDay HeatingDesignDay(this WeatherData weatherData)
        {
            List<WeatherYear> weatherYears = weatherData?.WeatherYears;
            if (weatherYears == null || weatherYears.Count == 0)
            {
                return null;
            }

            WeatherDay weatherDay = null;
            int dayIndex = -1;

            List<Tuple<int, WeatherDay>> tuples = new List<Tuple<int, WeatherDay>>();
            foreach (WeatherYear weatherYear in weatherYears)
            {
                weatherDay = Weather.Query.HeatingDesignWeatherDay(weatherYear.WeatherDays, out dayIndex);
                if (weatherDay == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<int, WeatherDay>(dayIndex, weatherDay));
            }

            int yearIndex = -1;

            weatherDay = Weather.Query.HeatingDesignWeatherDay(tuples.ConvertAll(x => x.Item2), out yearIndex);
            dayIndex = tuples[yearIndex].Item1;

            string name = null;
            if (!weatherData.TryGetValue(WeatherDataParameter.City, out name) || string.IsNullOrWhiteSpace(name))
            {
                name = weatherData.Name;
            }

            string sufix = "ANN HTG 100% CONDS DB";
            name = string.IsNullOrWhiteSpace(name) ? sufix : string.Format("{0} {1}", name, sufix);

            int year = weatherYears[yearIndex].Year;
            DateTime dateTime = new DateTime(year, 1, 1);
            dateTime = dateTime.AddDays(dayIndex);

            DesignDay result = Create.DesignDay(name, (short)dateTime.Year, (byte)dateTime.Month, (byte)dateTime.Day, weatherDay);
            return result;
        }
    }
}