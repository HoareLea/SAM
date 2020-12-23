using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static string DataString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;
            
            IEnumerable<int> years = weatherData?.Years;
            if (years == null)
                return null;

            List<string> values = new List<string>();
            foreach (int year in years)
                values.Add(DataString(year, weatherData[year]));

            return string.Join("\n", values);
        }

        public static string DataString(int year, WeatherYear weatherYear)
        {
            if (weatherYear == null)
                return null;

            string[] values = Enumerable.Repeat(string.Empty, 365).ToArray();
            for (int i = 0; i < 365; i++)
            {
                WeatherDay weatherDay = weatherYear[i];
                if (weatherDay == null)
                    continue;

                DateTime dateTime = new DateTime(year, 1, 1);
                dateTime = dateTime.AddDays(i);

                values[i] = DataString(year, dateTime.Month, dateTime.Day, weatherDay);
            }

            return string.Join("\n", values);
        }

        public static string DataString(int year, int month, int day, WeatherDay weatherDay)
        {
            if (weatherDay == null)
                return null;

            List<string> values = new List<string>();
            for (int i = 0; i < 24; i++)
            {
                string value = DataString(year, month, day, i, weatherDay);
                values.Add(value == null ? string.Empty : value);
            }

            return string.Join("\n", values);
        }

        public static string DataString(int year, int month, int day, int hour, WeatherDay weatherDay)
        {
            if (weatherDay == null)
                return null;

            List<string> values = new List<string>();

            double dryBulbTemperature = weatherDay[WeatherDataType.DryBulbTemperature, hour];
            double wetBulbTemperature = weatherDay[WeatherDataType.WetBulbTemperature, hour];
            double relativeHumidity = weatherDay[WeatherDataType.RelativeHumidity, hour];
            double atmosphericPressure = weatherDay[WeatherDataType.AtmosphericPressure, hour];

            double directSolarRadiation = weatherDay[WeatherDataType.DirectSolarRadiation, hour];
            double diffuseSolarRadiation = weatherDay[WeatherDataType.DiffuseSolarRadiation, hour];

            if (double.IsNaN(directSolarRadiation) && !double.IsNaN(diffuseSolarRadiation))
            {
                double globalSolarRadiation = weatherDay[WeatherDataType.GlobalSolarRadiation, hour];
                if (!double.IsNaN(globalSolarRadiation))
                    directSolarRadiation = globalSolarRadiation - diffuseSolarRadiation;
            }

            if (double.IsNaN(diffuseSolarRadiation) && !double.IsNaN(directSolarRadiation))
            {
                double globalSolarRadiation = weatherDay[WeatherDataType.GlobalSolarRadiation, hour];
                if (!double.IsNaN(globalSolarRadiation))
                    diffuseSolarRadiation = globalSolarRadiation - directSolarRadiation;
            }

            double windDirection = weatherDay[WeatherDataType.WindDirection, hour];
            double windSpeed = weatherDay[WeatherDataType.WindSpeed, hour];
            double cloudCover = weatherDay[WeatherDataType.CloudCover, hour];

            values.Add(year.ToString()); //Year
            values.Add(month.ToString()); //Month
            values.Add(day.ToString()); //Day
            values.Add((hour + 1).ToString()); // Hour
            values.Add(0.ToString()); //Minute
            values.Add("C9C9C9C9*0?9?9?9?9?9?9?9A7A7A7A7A7A7*0E8*0*0"); //Flags
            values.Add(double.IsNaN(dryBulbTemperature) ? 0.ToString() : dryBulbTemperature.ToString()); //Dry Bulb Temperature
            values.Add(double.IsNaN(wetBulbTemperature) ? 0.ToString() : wetBulbTemperature.ToString()); //Wet Bulb Temperature
            values.Add(double.IsNaN(relativeHumidity) ? 0.ToString() : relativeHumidity.ToString()); //Relative Humidity
            values.Add(double.IsNaN(atmosphericPressure) ? 0.ToString() : atmosphericPressure.ToString()); //Atmospheric Pressure
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 4))); // Solar
            values.Add(double.IsNaN(directSolarRadiation) ? 0.ToString() : directSolarRadiation.ToString()); //Direct Solar Radiation
            values.Add(double.IsNaN(diffuseSolarRadiation) ? 0.ToString() : diffuseSolarRadiation.ToString()); //Diffuse Solar Radiation
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 4))); // Illumination
            values.Add(double.IsNaN(windDirection) ? 0.ToString() : windDirection.ToString()); //Wind Direction
            values.Add(double.IsNaN(windSpeed) ? 0.ToString() : windSpeed.ToString()); //Wind Speed
            values.Add(double.IsNaN(cloudCover) ? 0.ToString() : cloudCover.ToString()); //Cloud Cover
            values.Add(string.Join(",", Enumerable.Repeat(0.ToString(), 9))); // Sky, Precipiatation, Snow etc.

            return string.Join(",", values);
        }
    }
}