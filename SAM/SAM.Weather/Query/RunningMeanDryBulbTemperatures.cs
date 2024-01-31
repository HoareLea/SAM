using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Weather
{
    public static partial class Query
    {
        public static List<double> RunningMeanDryBulbTemperatures(this WeatherYear weatherYear)
        {
            return RunningMeanDryBulbTemperatures(weatherYear?.WeatherDays);
        }

        public static List<double> RunningMeanDryBulbTemperatures(this WeatherYear weatherYear, int startDayIndex, int endDayIndex)
        {
            if(weatherYear == null)
            {
                return null;
            }

            return RunningMeanDryBulbTemperatures(weatherYear.WeatherDays, startDayIndex, endDayIndex);
        }

        public static List<double> RunningMeanDryBulbTemperatures(this IEnumerable<WeatherDay> weatherDays)
        {
            if (weatherDays == null)
            {
                return null;
            }

            if(weatherDays.Count() == 0)
            {
                return new List<double>();
            }

            return RunningMeanDryBulbTemperatures(weatherDays, 0, weatherDays.Count() - 1);

            //List<double> result = new List<double>();

            //if(weatherDays.Count() == 0)
            //{
            //    return result;
            //}

            //List<double> dryBulbTemperatures = new List<double>();
            //foreach(WeatherDay weatherDay in weatherDays) 
            //{
            //    if(weatherDay == null)
            //    {
            //        return result;
            //    }

            //    double dryBulbTempearture = weatherDay.Average(WeatherDataType.DryBulbTemperature);
            //    if(double.IsNaN(dryBulbTempearture))
            //    {
            //        return result;
            //    }

            //    dryBulbTemperatures.Add(dryBulbTempearture);
            //}

            //for (int i = 0; i < dryBulbTemperatures.Count; i++)
            //{
            //    result.Add(ApproximateRunningMeanDryBulbTemperature(dryBulbTemperatures, i));
            //}

            //return result;
        }

        public static List<double> RunningMeanDryBulbTemperatures(this IEnumerable<WeatherDay> weatherDays, int startDayIndex, int endDayIndex)
        {
            if (weatherDays == null || startDayIndex > endDayIndex)
            {
                return null;
            }

            List<double> result = new List<double>();

            int count = weatherDays.Count();
            if (count == 0)
            {
                return result;
            }

            int startDayIndex_Temp = Core.Query.BoundedIndex(count, startDayIndex);
            int endDayIndex_Temp = Core.Query.BoundedIndex(count, endDayIndex);

            List<double> dryBulbTemperatures = new List<double>();
            int index = startDayIndex_Temp - 7;
            for (int i = index; i < startDayIndex_Temp; i++)
            {
                int boundedIndex = Core.Query.BoundedIndex(count, i);

                double dryBulbTempearture = weatherDays.ElementAt(boundedIndex).Average(WeatherDataType.DryBulbTemperature);
                if (double.IsNaN(dryBulbTempearture))
                {
                    return result;
                }

                dryBulbTemperatures.Add(dryBulbTempearture);
            }

            double runningMeanDryBulbTemperature = ApproximateRunningMeanDryBulbTemperature(dryBulbTemperatures, 7);

            for(int i = startDayIndex_Temp; i <= endDayIndex_Temp; i++)
            {
                runningMeanDryBulbTemperature = (0.2 * weatherDays.ElementAt(i).Average(WeatherDataType.DryBulbTemperature)) + (0.8 * runningMeanDryBulbTemperature);
                result.Add(runningMeanDryBulbTemperature);
            }

            return result;
        }
    }
}