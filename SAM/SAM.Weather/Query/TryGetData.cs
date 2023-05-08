using System;
using System.Collections.Generic;

namespace SAM.Weather
{
    public static partial class Query
    {   /// <summary>
        /// Tries to get Data from EPW File text line
        /// </summary>
        /// <param name="text">EPW File line text</param>
        /// <param name="dateTime">Out DateTime where year is read from text or yer parameter (if year input is different than -1)</param>
        /// <param name="dictionary">Extracted Data</param>
        /// <param name="includeMinutes">Include minutes in DateTime conversion</param>
        /// <param name="year">Year will be used if value different than -1 otherwise year from text will be used</param>
        /// <returns>True if data extracted correctly</returns>
        public static bool TryGetData(string text, out DateTime dateTime, out Dictionary<string, double> dictionary, bool includeMinutes = true, int year = -1)
        {
            dateTime = default;
            dictionary = null;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            string[] values = text.Trim().Split(',');
            if (values.Length < 4)
                return false;

            int year_Temp = year;
            if (year_Temp == -1)
            {
                if (!int.TryParse(values[0], out year_Temp))
                    return false;
            }

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

            dateTime = new DateTime(year_Temp, month, day, hour, 0, 0);
            if (includeMinutes)
            {
                dateTime = dateTime.AddMinutes(minute);
            }

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

                if (double.TryParse(values[13], out value))
                    dictionary[WeatherDataType.GlobalSolarRadiation.ToString()] = value;

                if (double.TryParse(values[15], out value))
                    dictionary[WeatherDataType.DiffuseSolarRadiation.ToString()] = value;

                if (double.TryParse(values[20], out value))
                    dictionary[WeatherDataType.WindDirection.ToString()] = value;

                if (double.TryParse(values[21], out value))
                    dictionary[WeatherDataType.WindSpeed.ToString()] = value;

                if (double.TryParse(values[22], out value))
                    dictionary[WeatherDataType.CloudCover.ToString()] = value / 10;
            }

            return true;
        }
    }
}