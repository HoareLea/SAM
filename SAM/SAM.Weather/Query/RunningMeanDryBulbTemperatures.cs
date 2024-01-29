using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static List<double> RunningMeanDryBulbTemperatures(this WeatherYear weatherYear, double factor = 3.8)
        {
            if(weatherYear == null)
            {
                return null;
            }

            return RunningMeanDryBulbTemperatures(weatherYear.WeatherDays, factor);
        }

        public static List<double> RunningMeanDryBulbTemperatures(this IEnumerable<WeatherDay> weatherDays, double factor = 3.8)
        {
            if(weatherDays == null)
            {
                return null;
            }

            List<double> result = new List<double>();

            if(weatherDays.Count() == 0)
            {
                return result;
            }

            List<double> dryBulbTemperatures = new List<double>();
            foreach(WeatherDay weatherDay in weatherDays) 
            {
                if(weatherDay == null)
                {
                    return result;
                }

                double dryBulbTempearture = weatherDay.Average(WeatherDataType.DryBulbTemperature);
                if(double.IsNaN(dryBulbTempearture))
                {
                    return result;
                }

                dryBulbTemperatures.Add(dryBulbTempearture);
            }

            for (int i = 0; i < dryBulbTemperatures.Count; i++)
            {
                result.Add(RunningMeanDryBulbTemperature(dryBulbTemperatures, i, factor));
            }

            return result;
        }
    }
}