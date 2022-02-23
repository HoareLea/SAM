using SAM.Weather;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static DesignDay CoolingDesignDay(this WeatherData weatherData)
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
                weatherDay = Weather.Query.CoolingDesignWeatherDay(weatherYear.WeatherDays, out dayIndex);
                if (weatherDay == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<int, WeatherDay>(dayIndex, weatherDay));
            }

            int yearIndex = -1;

            weatherDay = Weather.Query.CoolingDesignWeatherDay(tuples.ConvertAll(x => x.Item2), out yearIndex);
            dayIndex = tuples[yearIndex].Item1;

            string name = null;
            if (!weatherData.TryGetValue(WeatherDataParameter.City, out name) || string.IsNullOrWhiteSpace(name))
            {
                name = weatherData.Name;
            }

            string sufix = "ANN CLG 0% CONDS DB=>GRad";
            name = string.IsNullOrWhiteSpace(name) ? sufix : string.Format("{0} {1}", name, sufix);

            int year = weatherYears[yearIndex].Year;
            DateTime dateTime = new DateTime(year, 1, 1);
            dateTime = dateTime.AddDays(dayIndex);

            DesignDay result = Create.DesignDay(name, "Cooling Design Day automated :) \n *calculated from weather data as day with max temp and then radation replaced from max global radation day and cloud cover removed", (short)dateTime.Year, (byte)dateTime.Month, (byte)dateTime.Day, weatherDay);
            if (result != null)
            {
                if (result.Contains(WeatherDataType.CloudCover))
                {
                    for (int i = 0; i < 24; i++)
                    {
                        result[WeatherDataType.CloudCover, i] = 0;
                    }
                }

            }

            return result;
        }
    }
}