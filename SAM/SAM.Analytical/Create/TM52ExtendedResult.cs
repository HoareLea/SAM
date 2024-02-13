using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static TM52ExtendedResult TM52ExtendedResult(TM52ExtendedResult tM52ExtendedResult, int startIndex, int endIndex)
        {
            if(tM52ExtendedResult == null || startIndex > endIndex)
            {
                return null;
            }

            HashSet<int> hourIndexes = new HashSet<int>();
            for (int i = startIndex; i <= endIndex; i++)
            {
                hourIndexes.Add(i);
            }

            return TM52SpaceExtendedResult(tM52ExtendedResult, hourIndexes);
        }

        public static TM52ExtendedResult TM52SpaceExtendedResult(TM52ExtendedResult tM52SpaceExtendedResult, IEnumerable<int> hourIndexes)
        {
            if (tM52SpaceExtendedResult == null || hourIndexes == null)
            {
                return null;
            }

            HashSet<int> occupiedHourIndices_Old = tM52SpaceExtendedResult.OccupiedHourIndices;
            IndexedDoubles minAcceptableTemperatures_Old = tM52SpaceExtendedResult.MinAcceptableTemperatures;
            IndexedDoubles maxAcceptableTemperatures_Old = tM52SpaceExtendedResult.MaxAcceptableTemperatures;
            IndexedDoubles operativeTemperatures_Old = tM52SpaceExtendedResult.OperativeTemperatures;

            HashSet<int> occupiedHourIndices = new HashSet<int>();
            IndexedDoubles minAcceptableTemperatures = new IndexedDoubles();
            IndexedDoubles maxAcceptableTemperatures = new IndexedDoubles();
            IndexedDoubles operativeTemperatures = new IndexedDoubles();
            foreach(int hourIndex in hourIndexes)
            {
                if (occupiedHourIndices_Old.Contains(hourIndex))
                {
                    occupiedHourIndices.Add(hourIndex);
                }

                if (minAcceptableTemperatures_Old.ContainsIndex(hourIndex))
                {
                    minAcceptableTemperatures.Add(hourIndex, minAcceptableTemperatures_Old[hourIndex]);
                }

                if (maxAcceptableTemperatures_Old.ContainsIndex(hourIndex))
                {
                    maxAcceptableTemperatures.Add(hourIndex, maxAcceptableTemperatures_Old[hourIndex]);
                }

                if (operativeTemperatures_Old.ContainsIndex(hourIndex))
                {
                    operativeTemperatures.Add(hourIndex, operativeTemperatures_Old[hourIndex]);
                }
            }

            TM52ExtendedResult result = new TM52ExtendedResult(tM52SpaceExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures);

            return result;
        }
    }
}