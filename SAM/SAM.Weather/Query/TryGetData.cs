using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static bool TryGetData(string text, out DateTime dateTime, out Dictionary<string, double> dictionary)
        {
            dateTime = default;
            dictionary = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] values = text.Trim().Split(',');
            if (values.Length < 4)
                return false;

            int year;
            if (!int.TryParse(values[0], out year))
                return false;

            int month;
            if (!int.TryParse(values[1], out month))
                return false;

            int day;
            if (!int.TryParse(values[2], out day))
                return false;

            int hour;
            if (!int.TryParse(values[3], out hour))
                return false;

            hour = hour - 1;
            if (hour < 0)
                hour = 0;

            int minute = 0;
            if (values.Length > 4)
            {
                if (!int.TryParse(values[4], out minute))
                    return false;
            }

            dateTime = new DateTime(year, month, day);
            dateTime = dateTime.AddHours(hour);
            dateTime = dateTime.AddMinutes(minute);

            if (values.Length > 23)
            {
                dictionary = new Dictionary<string, double>();
                double value;

                if (double.TryParse(values[6], out value))
                    dictionary[WeatherDataType.DryBulbTemperature.ToString()] = value;

                if (double.TryParse(values[7], out value))
                    dictionary[WeatherDataType.WetBulbTemperature.ToString()] = value;

                if (double.TryParse(values[8], out value))
                    dictionary[WeatherDataType.RelativeHumidity.ToString()] = value;

                if (double.TryParse(values[9], out value))
                    dictionary[WeatherDataType.AtmosphericPressure.ToString()] = value;

                if (double.TryParse(values[14], out value))
                    dictionary[WeatherDataType.DirectSolarRadiation.ToString()] = value;

                if (double.TryParse(values[20], out value))
                    dictionary[WeatherDataType.WindDirection.ToString()] = value;

                if (double.TryParse(values[21], out value))
                    dictionary[WeatherDataType.WindSpeed.ToString()] = value;

                if (double.TryParse(values[22], out value))
                    dictionary[WeatherDataType.CloudCover.ToString()] = value;
            }

            return true;
        }
    }
}