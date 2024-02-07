namespace SAM.Core
{
    public static partial class Query
    {
        public static int HourOfDay(int hourOfYear)
        {
            return hourOfYear % 24;
        }
    }
}