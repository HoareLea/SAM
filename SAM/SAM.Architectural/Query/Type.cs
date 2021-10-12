namespace SAM.Architectural
{
    public static partial class Query
    {
        public static HostPartitionType Type(this IHostPartition hostPartition)
        {
            return (hostPartition as dynamic)?.Type;
        }

        public static OpeningType Type(this IOpening opening)
        {
            return (opening as dynamic)?.Type;
        }
    }
}