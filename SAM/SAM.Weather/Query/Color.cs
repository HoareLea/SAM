﻿using System.Drawing;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static Color Color(this WeatherDataType weatherDataType)
        {
            switch (weatherDataType)
            {
                case WeatherDataType.AtmosphericPressure:
                    return System.Drawing.Color.FromArgb(255, 0, 128);

                case WeatherDataType.CloudCover:
                    return System.Drawing.Color.FromArgb(128, 0, 255);

                case WeatherDataType.DiffuseSolarRadiation:
                    return System.Drawing.Color.FromArgb(255, 204, 0);

                case WeatherDataType.DryBulbTemperature:
                    return System.Drawing.Color.FromArgb(0, 0, 128);

                case WeatherDataType.GlobalSolarRadiation:
                    return System.Drawing.Color.FromArgb(255, 153, 0);

                case WeatherDataType.RelativeHumidity:
                    return System.Drawing.Color.FromArgb(0, 153, 255);

                case WeatherDataType.WetBulbTemperature:
                    return System.Drawing.Color.FromArgb(0, 0, 255);

                case WeatherDataType.WindDirection:
                    return System.Drawing.Color.FromArgb(64, 0, 255);

                case WeatherDataType.WindSpeed:
                    return System.Drawing.Color.FromArgb(128, 0, 255);
            }

            return System.Drawing.Color.Empty;
        }
    }
}