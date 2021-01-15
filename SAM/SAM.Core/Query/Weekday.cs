namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Weekday(this Week week)
        {
            if (week == Week.Undefined)
                return false;
            
            return !Weekend(week);
        }
    }
}