using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static WeatherDay DesignWeatherDay(this WeatherData weatherData)
        {
            return DesignWeatherDay(weatherData?.WeatherDays());
        }

        public static WeatherDay DesignWeatherDay(this WeatherYear weatherYear)
        {
            return DesignWeatherDay(weatherYear?.WeatherDays);
        }

        public static WeatherDay DesignWeatherDay(this IEnumerable<WeatherDay> weatherDays)
        {
            if (weatherDays == null || weatherDays.Count() == 0)
            {
                return null;
            }

            double dryBulbTemperature_Max = double.MinValue;
            double solarRadiation_Max = double.MinValue;
            WeatherDay weatherDay_DryBulbTemperature = null;
            WeatherDay weatherDay_SolarRadiation = null;
            foreach (WeatherDay weatherDay in weatherDays)
            {
                for (int i = 0; i < 24; i++)
                {
                    if (weatherDay.TryGetValue(WeatherDataType.DryBulbTemperature, i, out double dryBulbTemperature) && dryBulbTemperature > dryBulbTemperature_Max)
                    {
                        weatherDay_DryBulbTemperature = weatherDay;
                        dryBulbTemperature_Max = dryBulbTemperature;
                    }

                    double solarRadiation = weatherDay.CalculatedGlobalRadiation(i);
                    if (!double.IsNaN(solarRadiation) && solarRadiation > solarRadiation_Max)
                    {
                        weatherDay_SolarRadiation = weatherDay;
                        solarRadiation_Max = solarRadiation;
                    }
                }
            }

            if (weatherDay_DryBulbTemperature == null)
            {
                return null;
            }

            WeatherDay result = new WeatherDay(weatherDay_DryBulbTemperature);
            if (weatherDay_SolarRadiation != null)
            {
                for (int i = 0; i < 24; i++)
                {
                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.DiffuseSolarRadiation))
                    {
                        result[WeatherDataType.DiffuseSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.DiffuseSolarRadiation][i];
                    }

                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.DirectSolarRadiation))
                    {
                        result[WeatherDataType.DirectSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.DirectSolarRadiation][i];
                    }

                    if (weatherDay_SolarRadiation.Contains(WeatherDataType.GlobalSolarRadiation))
                    {
                        result[WeatherDataType.GlobalSolarRadiation][i] = weatherDay_SolarRadiation[WeatherDataType.GlobalSolarRadiation][i];
                    }
                }
            }

            return result;
        }
    }
}