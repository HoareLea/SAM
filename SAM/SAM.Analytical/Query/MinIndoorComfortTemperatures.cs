﻿using SAM.Core;
using SAM.Weather;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<double> MinIndoorComfortTemperatures(this WeatherYear weatherYear, TM52BuildingCategory tM52BuildingCategory, int startDayIndex, int endDayIndex, double acceptableTemperatureDifference = double.NaN)
        {
            return MinIndoorComfortTemperatures(weatherYear?.WeatherDays, tM52BuildingCategory, startDayIndex, endDayIndex, acceptableTemperatureDifference);
        }

        public static List<double> MinIndoorComfortTemperatures(this WeatherYear weatherYear, TM52BuildingCategory tM52BuildingCategory, double acceptableTemperatureDifference = double.NaN)
        {
            List<WeatherDay> weatherDays = weatherYear?.WeatherDays;
            if(weatherDays == null || weatherDays.Count == 0)
            {
                return null;
            }

            return MinIndoorComfortTemperatures(weatherDays, tM52BuildingCategory, 0, weatherDays.Count - 1, acceptableTemperatureDifference);
        }

        public static List<double> MinIndoorComfortTemperatures(this IEnumerable<WeatherDay> weatherDays, TM52BuildingCategory tM52BuildingCategory, int startDayIndex, int endDayIndex, double acceptableTemperatureDifference = double.NaN)
        {
            if(weatherDays == null || weatherDays.Count() == 0)
            {
                return null;
            }

            if (tM52BuildingCategory == TM52BuildingCategory.Undefined)
            {
                return null;
            }

            if (double.IsNaN(acceptableTemperatureDifference))
            {
                Range<double> temperatureRange = tM52BuildingCategory.TemperatureRange();
                if (temperatureRange == null)
                {
                    return null;
                }

                acceptableTemperatureDifference = temperatureRange.Min;
            }

            List<double> runningMeanDryBulbTemperatures = weatherDays.RunningMeanDryBulbTemperatures(startDayIndex, endDayIndex);
            if (runningMeanDryBulbTemperatures == null || runningMeanDryBulbTemperatures.Count == 0)
            {
                return null;
            }

            double factor = 18.8 + acceptableTemperatureDifference;

            return runningMeanDryBulbTemperatures?.ConvertAll(x => (0.33 * x) + factor);

        }
    }
}