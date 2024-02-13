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

        /// <summary>
        /// Bounded index between 0 and count - 1
        /// </summary>
        /// <param name="count">Items count</param>
        /// <param name="index">Index</param>
        /// <returns>BoundedIndex</returns>
        public static int BoundedIndex(this int count, int index)
        {
            int max = count - 1;

            if(max < 0)
            {
                return int.MinValue;
            }

            return BoundedIndex(new Range<int>(0, max), index);
        }
    }
}