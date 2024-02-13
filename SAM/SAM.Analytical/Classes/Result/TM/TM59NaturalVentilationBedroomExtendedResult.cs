using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59NaturalVentilationBedroomExtendedResult : TM59NaturalVentilationExtendedResult
    {
        public TM59NaturalVentilationBedroomExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, TM59SpaceApplication.Sleeping)
        {

        }

        public TM59NaturalVentilationBedroomExtendedResult(TM59NaturalVentilationBedroomExtendedResult tM59NaturalVentilationBedroomExtendedResult)
            : base(tM59NaturalVentilationBedroomExtendedResult)
        {

        }

        public TM59NaturalVentilationBedroomExtendedResult(TM59NaturalVentilationBedroomExtendedResult tM59NaturalVentilationBedroomExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM59NaturalVentilationBedroomExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }

        public TM59NaturalVentilationBedroomExtendedResult(JObject jObject)
            : base(jObject)
        {
        }

        /// <summary>
        /// Number of hours during the night-time (22:00 - 7:00 -> 9 hours) that the bedroom spaces are occupied
        /// </summary>
        /// <returns>Annual night occupied hours for bedroom</returns>
        public int GetAnnualNightOccupiedHours()
        {
            return GetOccupiedNightHourIndices().Count;
        }

        /// <summary>
        /// the maximum number of hours you are allowed to exceed an operative temperature of 26°C in the bedroom spaces
        /// </summary>
        /// <returns>Maximal exceedable night hours</returns>
        public int GetAnnualMaxExceedableNightHours()
        {
            return System.Convert.ToInt32(System.Math.Truncate(GetAnnualNightOccupiedHours() * 0.01));
        }

        public HashSet<int> GetOccupiedNightHourIndices()
        {
            HashSet<int> occupiedHourIndices = OccupiedHourIndices;
            if (occupiedHourIndices == null)
            {
                return null;
            }

            HashSet<int> result = new HashSet<int>();
            foreach (int occupiedHourIndex in occupiedHourIndices)
            {
                int hourOfDay = Core.Query.HourOfDay(occupiedHourIndex);
                if (hourOfDay >= 22 || hourOfDay < 7)
                {
                    result.Add(occupiedHourIndex);
                }
            }

            return result;
        }

        public int GetNightHoursNumberExceeding26()
        {
            HashSet<int> occupiedNightHourIndices = GetOccupiedNightHourIndices();
            if (occupiedNightHourIndices == null)
            {
                return -1;
            }

            IndexedDoubles operativeTemperatures = OperativeTemperatures;
            if (operativeTemperatures == null)
            {
                return -1;
            }

            int count = 0;
            foreach (int occupiedNightHourIndex in occupiedNightHourIndices)
            {
                double operativeTemperature = operativeTemperatures[occupiedNightHourIndex];
                if (operativeTemperature > 26)
                {
                    count++;
                }
            }

            return count;
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