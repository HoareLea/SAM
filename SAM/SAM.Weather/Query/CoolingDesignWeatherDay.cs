using System.Collections.Generic;
using System.Linq;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static WeatherDay CoolingDesignWeatherDay(this WeatherData weatherData)
        {
            return CoolingDesignWeatherDay(weatherData?.WeatherDays());
        }

        public static WeatherDay CoolingDesignWeatherDay(this WeatherYear weatherYear)
        {
            return CoolingDesignWeatherDay(weatherYear?.WeatherDays);
        }

        public static WeatherDay CoolingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays, out int dayIndex)
        {
            dayIndex = -1;

            if (weatherDays == null || weatherDays.Count() == 0)
            {
                return null;
            }

            double dryBulbTemperature_Max = double.MinValue;
            double solarRadiation_Max = double.MinValue;
            WeatherDay weatherDay_DryBulbTemperature = null;
            WeatherDay weatherDay_SolarRadiation = null;

            for(int i=0; i < weatherDays.Count(); i++)
            {
                WeatherDay weatherDay = weatherDays.ElementAt(i);
                if(weatherDay == null)
                {
                    continue;
                }
                
                for (int j = 0; j < 24; j++)
                {
                    if (weatherDay.TryGetValue(WeatherDataType.DryBulbTemperature, j, out double dryBulbTemperature) && dryBulbTemperature > dryBulbTemperature_Max)
                    {
                        weatherDay_DryBulbTemperature = weatherDay;
                        dryBulbTemperature_Max = dryBulbTemperature;
                        dayIndex = i;
                    }

                    double solarRadiation = weatherDay.CalculatedGlobalRadiation(j);
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

        public static WeatherDay CoolingDesignWeatherDay(this IEnumerable<WeatherDay> weatherDays)
        {
            return CoolingDesignWeatherDay(weatherDays, out int dayIndex);
        }
    }
}