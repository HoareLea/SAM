using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59CorridorExtendedResult : TM59ExtendedResult
    {
        public TM59CorridorExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, TM59SpaceApplication.Undefined)
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
            IndexedDoubles operativeTemperatures = OperativeTemperatures;
            if (operativeTemperatures == null)
            {
                return -1;
            }

            int count = 0;
            foreach (double operativeTemperature in operativeTemperatures)
            {
                if (operativeTemperature > 28)
                {
                    count++;
                }
            }

            return count;
        }

        public override int MaxExceedableHours
        {
            get
            {
                IndexedDoubles operativeTemperatures = OperativeTemperatures;
                if (operativeTemperatures == null)
                {
                    return -1;
                }

                int count = operativeTemperatures.GetMaxIndex().Value - operativeTemperatures.GetMinIndex().Value + 1;
                return System.Convert.ToInt32(System.Math.Truncate(count * ExceedanceFactor));
            }
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