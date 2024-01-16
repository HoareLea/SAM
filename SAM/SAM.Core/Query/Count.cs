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

        public static int Count(Period period_Destination, Period period_Source = Core.Period.Hourly)
        {
            switch (period_Destination)
            {
                case Core.Period.Weekly:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            return 168;

                        case Core.Period.Daily:
                            return 7;
                    }
                    break;

                case Core.Period.Daily:
                    switch (period_Source)
                    {
                        case Core.Period.Hourly:
                            return 24;
                    }
                    break;
            }

            return -1;
        }
    }
}