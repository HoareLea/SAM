namespace SAM.Architectural
{
    public static partial class Query
    {
        public static OpeningAnalyticalType OpeningAnalyticalType(this IOpening opening)
        {
            if(opening == null)
            {
                return Architectural.OpeningAnalyticalType.Undefined;
            }

            if(opening is Door)
            {
                return Architectural.OpeningAnalyticalType.Door;
            }

            if(opening is Window)
            {
                return Architectural.OpeningAnalyticalType.Window;
            }

            return Architectural.OpeningAnalyticalType.Undefined;
        }

        public static OpeningAnalyticalType OpeningAnalyticalType(this OpeningType openingType)
        {
            if (openingType == null)
            {
                return Architectural.OpeningAnalyticalType.Undefined;
            }

            if (openingType is DoorType)
            {
                return Architectural.OpeningAnalyticalType.Door;
            }

            if (openingType is WindowType)
            {
                return Architectural.OpeningAnalyticalType.Window;
            }

            return Architectural.OpeningAnalyticalType.Undefined;
        }
    }
}