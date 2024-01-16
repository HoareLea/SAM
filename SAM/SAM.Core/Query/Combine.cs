using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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

        public static List<double> Combine(this IEnumerable<double> values, Period period_Destination, CombineType combineType, Period period_Source = Core.Period.Hourly, bool includeIncomplete = true)
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

            int count = Count(period_Destination, period_Source);
            if(count == -1)
            {
                return null;
            }

            return Combine(values, count, combineType, includeIncomplete);
        }
    
        public static IndexedDoubles Combine(this IndexedDoubles indexedDoubles, Period period_Destination, CombineType combineType, Period period_Source = Core.Period.Hourly, bool includeIncomplete = true)
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

            int count = Count(period_Destination, period_Source);
            if(count == -1)
            {
                return null;
            }

            int? min = indexedDoubles.GetMinIndex();
            if(min == null || !min.HasValue)
            {
                return null;
            }

            int? max = indexedDoubles.GetMaxIndex();
            if (max == null || !max.HasValue)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();

            int index = min.Value;
            while(min < max)
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
    }
}