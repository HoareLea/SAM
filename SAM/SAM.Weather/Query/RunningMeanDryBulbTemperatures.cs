using SAM.Core;
using System.Collections.Generic;
using System.Linq;

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

            int startDayIndex_Temp = 0; //Core.Query.BoundedIndex(count, startDayIndex);
            int endDayIndex_Temp = 364; //Core.Query.BoundedIndex(count, endDayIndex);

            //OPTION 1
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

            for (int i = startDayIndex_Temp; i <= endDayIndex_Temp; i++)
            {
                int boundedIndex = Core.Query.BoundedIndex(count, i - 1);

                runningMeanDryBulbTemperature = (0.2 * weatherDays.ElementAt(boundedIndex).Average(WeatherDataType.DryBulbTemperature)) + (0.8 * runningMeanDryBulbTemperature);
                result.Add(runningMeanDryBulbTemperature);
            }


            //OPTION 2
            //List<double> dryBulbTemperatures = new List<double>();
            //int index = startDayIndex_Temp - 6;
            //for (int i = index; i <= startDayIndex_Temp; i++)
            //{
            //    int boundedIndex = Core.Query.BoundedIndex(count, i);

            //    double dryBulbTempearture = weatherDays.ElementAt(boundedIndex).Average(WeatherDataType.DryBulbTemperature);
            //    if (double.IsNaN(dryBulbTempearture))
            //    {
            //        return result;
            //    }

            //    dryBulbTemperatures.Add(dryBulbTempearture);
            //}

            //double runningMeanDryBulbTemperature = ApproximateRunningMeanDryBulbTemperature(dryBulbTemperatures, 7);
            //result.Add(runningMeanDryBulbTemperature);

            //for (int i = startDayIndex_Temp + 1; i <= endDayIndex_Temp; i++)
            //{
            //    int boundedIndex = Core.Query.BoundedIndex(count, i - 1);

            //    runningMeanDryBulbTemperature = (0.2 * weatherDays.ElementAt(boundedIndex).Average(WeatherDataType.DryBulbTemperature)) + (0.8 * runningMeanDryBulbTemperature);

            //}

            int max = endDayIndex_Temp + 1;

            int startDayBoundedIndex = Core.Query.BoundedIndex(max, startDayIndex);
            int endDayBoundedIndex = Core.Query.BoundedIndex(max, endDayIndex);

            if(startDayBoundedIndex > endDayBoundedIndex)
            {
                List<double> runningMeanDryBulbTemperatures = new List<double>();

                for(int i = startDayBoundedIndex; i < result.Count; i++)
                {
                    runningMeanDryBulbTemperatures.Add(result[i]);
                }

                for(int i = 0; i < max; i++)
                {
                    runningMeanDryBulbTemperatures.Add(result[i]);
                }

                return runningMeanDryBulbTemperatures;
            }
            else
            {
                return result.GetRange(startDayBoundedIndex, max - startDayBoundedIndex);
            }
        }
    }
}