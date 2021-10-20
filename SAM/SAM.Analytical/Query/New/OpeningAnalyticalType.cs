namespace SAM.Analytical
{
    public static partial class Query
    {
        public static OpeningAnalyticalType OpeningAnalyticalType(this IOpening opening)
        {
            if(opening == null)
            {
                return Analytical.OpeningAnalyticalType.Undefined;
            }

            if(opening is Door)
            {
                return Analytical.OpeningAnalyticalType.Door;
            }

            if(opening is Window)
            {
                return Analytical.OpeningAnalyticalType.Window;
            }

            return Analytical.OpeningAnalyticalType.Undefined;
        }

        public static OpeningAnalyticalType OpeningAnalyticalType(this OpeningType openingType)
        {
            if (openingType == null)
            {
                return Analytical.OpeningAnalyticalType.Undefined;
            }

            if (openingType is DoorType)
            {
                return Analytical.OpeningAnalyticalType.Door;
            }

            if (openingType is WindowType)
            {
                return Analytical.OpeningAnalyticalType.Window;
            }

            return Analytical.OpeningAnalyticalType.Undefined;
        }
    }
}