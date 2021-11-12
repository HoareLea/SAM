using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static OpeningType DefaultOpeningType(this OpeningAnalyticalType openingAnalyticalType)
        {
            OpeningTypeLibrary openingTypeLibrary = DefaultOpeningTypeLibrary();
            if (openingTypeLibrary == null)
            {
                return null;
            }

            return openingTypeLibrary.GetOpeningTypes(openingAnalyticalType)?.FirstOrDefault();
        }
    }
}