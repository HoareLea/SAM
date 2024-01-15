using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
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

                switch (combineType)
                {
                    case CombineType.Sum:
                        result.Add(values_Temp.Sum());
                        break;

                    case CombineType.Average:
                        result.Add(result.Average());
                        break;

                    case CombineType.Min:
                        result.Add(result.Min());
                        break;

                    case CombineType.Max:
                        result.Add(result.Max());
                        break;
                }

                values_Temp.Clear();
            }

            if (includeIncomplete && values_Temp.Count < count)
            {
                switch (combineType)
                {
                    case CombineType.Sum:
                        result.Add(values_Temp.Sum());
                        break;

                    case CombineType.Average:
                        result.Add(result.Average());
                        break;

                    case CombineType.Min:
                        result.Add(result.Min());
                        break;

                    case CombineType.Max:
                        result.Add(result.Max());
                        break;
                }
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

            int count = -1;

            switch(period_Destination)
            {
                case Core.Period.Weekly:
                    switch(period_Source)
                    {
                        case Core.Period.Hourly:
                            count = 168;
                            break;

                        case Core.Period.Daily:
                            count = 7;
                            break;
                    }
                    break;

                case Core.Period.Daily:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            count = 24;
                            break;
                    }
                    break;
            }

            if(count == -1)
            {
                return null;
            }

            return Combine(values, count, combineType, includeIncomplete);
        }
    }
}