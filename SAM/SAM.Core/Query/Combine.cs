using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double Combine(this IEnumerable<double> values, CombineType combineType)
        {
            if (values == null || combineType == CombineType.Undefined)
            {
                return double.NaN;
            }

            List<double> values_Temp = new List<double>(values);

            switch (combineType)
            {
                case CombineType.Sum:
                    return values_Temp.Sum();

                case CombineType.Average:
                    return values_Temp.Average();

                case CombineType.Min:
                    return values_Temp.Min();

                case CombineType.Max:
                    return values_Temp.Max();
            }

            return double.NaN;
        }
        
        public static List<double> Combine(this IEnumerable<double> values, int count, CombineType combineType, bool includeIncomplete = true)
        {
            if (values == null || combineType == CombineType.Undefined)
            {
                return null;
            }

            List<double> result = new List<double>();

            List<double> values_Temp = new List<double>();
            foreach (double value in values)
            {
                values_Temp.Add(value);
                if (values_Temp.Count < count)
                {
                    continue;
                }

                result.Add(Combine(values_Temp, combineType));
                values_Temp.Clear();
            }

            if (includeIncomplete && values_Temp.Count < count)
            {
                result.Add(Combine(values_Temp, combineType));
            }

            return result;
        }

        public static List<double> Combine(this IEnumerable<double> values, Period period_Destination, CombineType combineType, Period period_Source = Core.Period.Hourly, bool includeIncomplete = true, int year = 2007)
        {
            if(values == null || period_Destination == Core.Period.Undefined || combineType == CombineType.Undefined)
            {
                return null;
            }


            if(period_Source == Core.Period.Undefined)
            {
                period_Source = Period(values.Count());
            }

            if(period_Source == period_Destination)
            {
                return new List<double>(values);
            }

            if(period_Destination != Core.Period.Monthly)
            {
                int count = Count(period_Destination, period_Source);
                if (count == -1)
                {
                    return null;
                }

                return Combine(values, count, combineType, includeIncomplete);
            }

            if(period_Source == Core.Period.Weekly)
            {
                return null;
            }

            List<double> result = new List<double>();

            switch(period_Source)
            {
                case Core.Period.Daily:
                    for (int i = 1; i <= 12; i++)
                    {
                        int count = DateTime.DaysInMonth(year, i);

                        List<double> values_Month = new List<double>();
                        for (int j = 1; j <= count; j++)
                        {
                            int index = new DateTime(year, i, j).DayOfYear;
                            values_Month.Add(values.ElementAt(index));
                        }

                        double value = Combine(values_Month, combineType);
                        result.Add(value);
                    }

                    return result;

                case Core.Period.Hourly:
                    for (int i = 1; i <= 12; i++)
                    {
                        int count = DateTime.DaysInMonth(year, i);

                        List<double> values_Month = new List<double>();
                        for (int j = 1; j <= count; j++)
                        {
                            for (int k = 0; k < 24; k++)
                            {
                                int index = new DateTime(year, i, j, k, 0, 0).HourOfYear();
                                values_Month.Add(values.ElementAt(index));
                            }
                        }

                        double value = Combine(values_Month, combineType);
                        result.Add(value);
                    }

                    return result;
            }

            return null;
        }
    
        public static IndexedDoubles Combine(this IndexedDoubles indexedDoubles, Period period_Destination, CombineType combineType, Period period_Source = Core.Period.Hourly, bool includeIncomplete = true, int year = 2007)
        {
            if(indexedDoubles == null)
            {
                return null;
            }

            if (period_Source == Core.Period.Undefined)
            {
                period_Source = Period(indexedDoubles);
            }

            if(period_Source == Core.Period.Undefined)
            {
                return null;
            }

            if(period_Destination == period_Source)
            {
                return new IndexedDoubles(indexedDoubles);
            }

            IndexedDoubles result = new IndexedDoubles();

            if (period_Destination != Core.Period.Monthly)
            {
                int count = Count(period_Destination, period_Source);
                if (count == -1)
                {
                    return null;
                }

                int? min = indexedDoubles.GetMinIndex();
                if (min == null || !min.HasValue)
                {
                    return null;
                }

                int? max = indexedDoubles.GetMaxIndex();
                if (max == null || !max.HasValue)
                {
                    return null;
                }

                int index = min.Value;
                while (min < max)
                {
                    IndexedDoubles indexedDoubles_Temp = Create.IndexedDoubles(indexedDoubles, min.Value, min.Value + count);
                    min += count;

                    IEnumerable<double> values = indexedDoubles_Temp?.Values;
                    if (values == null)
                    {
                        continue;
                    }

                    if (!includeIncomplete && values.Count() != count)
                    {
                        continue;
                    }

                    result.Add(index, Combine(values, combineType));
                    index++;
                }

                return result;
            }

            if (period_Source == Core.Period.Weekly)
            {
                return null;
            }

            switch (period_Source)
            {
                case Core.Period.Daily:
                    int index = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        int count = DateTime.DaysInMonth(year, i);
                        IndexedDoubles indexedDoubles_Temp = Create.IndexedDoubles(indexedDoubles, index, index + count);

                        index += count;

                        IEnumerable<double> values = indexedDoubles_Temp?.Values;
                        if (values == null)
                        {
                            continue;
                        }

                        if (!includeIncomplete && values.Count() != count)
                        {
                            continue;
                        }

                        result.Add(index, Combine(values, combineType));
                    }

                    return result;

                case Core.Period.Hourly:
                    int start = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        int count = DateTime.DaysInMonth(year, i) * 24;

                        IndexedDoubles indexedDoubles_Temp = Create.IndexedDoubles(indexedDoubles, start, start + count);
                        start += count;

                        IEnumerable<double> values = indexedDoubles_Temp?.Values;
                        if (values == null)
                        {
                            continue;
                        }

                        if (!includeIncomplete && values.Count() != count)
                        {
                            continue;
                        }

                        result.Add(start, Combine(values, combineType));
                    }

                    return result;
            }

            return null;

        }
    }
}