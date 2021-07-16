using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Dictionary<Range<int>, T> RangeDictionary<T>(this IEnumerable<T> values)
        {
            if (values == null)
                return null;

            List<T> values_Temp = new List<T>(values);

            Dictionary<Range<int>, T> result = new Dictionary<Range<int>, T>();

            Range<int> range = null;
            T value = default;
            while (values_Temp.Count > 0)
            {
                T value_Temp = values_Temp[0];
                values_Temp.RemoveAt(0);

                if (range == null)
                {
                    range = new Range<int>(0);
                    value = value_Temp;
                    continue;
                }

                if (value.Equals(value_Temp))
                {
                    range = new Range<int>(range.Min, range.Max + 1);
                    continue;
                }
                else
                {
                    result[range] = value;
                    range = new Range<int>(range.Max + 1);
                    value = value_Temp;
                    continue;
                }
            }

            if (range != null)
                result[range] = value;

            return result;

        }
    }
}