using System;
using System.Configuration;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int BoundedIndex(this Range<int> range, int index)
        {
            if(range == null)
            {
                return int.MinValue;
            }

            if(range.In(index))
            {
                return index;
            }

            int count = range.Count();

            if(index < range.Min)
            {
                int difference = range.Min - index - 1;
                int reminder = difference % count;
                return range.Max - reminder;
            }

            if(index > range.Max)
            {
                int difference = index - range.Max - 1;
                int reminder = difference % count;
                return range.Min + reminder;
            }

            return int.MinValue;
        }
    }
}