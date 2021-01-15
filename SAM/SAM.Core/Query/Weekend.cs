namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Weekend(this Week week)
        {
            return week == Week.Saturday || week == Week.Sunday;
        }
    }
}