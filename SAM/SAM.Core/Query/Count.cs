namespace SAM.Core
{
    public static partial class Query
    {
        public static int Count(this Range<int> range)
        {
            if(range == null)
            {
                return -1;
            }

            return range.Max - range.Min + 1;

        }
    }
}