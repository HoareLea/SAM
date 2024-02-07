using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59CorridorExtendedResult : TM59ExtendedResult
    {
        public TM59CorridorExtendedResult(string name, string source, string reference, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, TM59SpaceApplication.Undefined)
        {

        }

        public TM59CorridorExtendedResult(TM59CorridorExtendedResult tM59CorridorExtendedResult)
            : base(tM59CorridorExtendedResult)
        {

        }

        public TM59CorridorExtendedResult(TM59CorridorExtendedResult tM59CorridorExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM59CorridorExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }

        public TM59CorridorExtendedResult(JObject jObject)
            : base(jObject)
        {
        }

        public int GetHoursNumberExceeding28()
        {
            HashSet<int> occupiedHourIndices = OccupiedHourIndices;
            if (occupiedHourIndices == null)
            {
                return -1;
            }

            IndexedDoubles operativeTemperatures = OperativeTemperatures;
            if (operativeTemperatures == null)
            {
                return -1;
            }

            int count = 0;
            foreach (int occupiedHourIndex in occupiedHourIndices)
            {
                double operativeTemperature = operativeTemperatures[occupiedHourIndex];
                if (operativeTemperature > 28)
                {
                    count++;
                }
            }

            return count;
        }

        public override bool Criterion1
        {
            get
            {
                return GetHoursNumberExceeding28() < MaxExceedableHours;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
            {
                return null;
            }

            return result;
        }
    }
}