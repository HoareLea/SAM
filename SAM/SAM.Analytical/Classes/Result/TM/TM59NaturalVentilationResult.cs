using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM59NaturalVentilationResult : TM59Result
    {
        private int hoursExceedingComfortRange;
        private int summerOccupiedHours;
        private int maxExceedableSummerHours;

        public TM59NaturalVentilationResult(
            string name, 
            string source, 
            string reference, 
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours, 
            int maxExceedableHours,
            int summerOccupiedHours,
            int maxExceedableSummerHours,
            int hoursExceedingComfortRange,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
            this.summerOccupiedHours = summerOccupiedHours;
            this.maxExceedableSummerHours = maxExceedableSummerHours;
        }

        public TM59NaturalVentilationResult(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            TM52BuildingCategory tM52BuildingCategory,
            int occupiedHours,
            int maxExceedableHours,
            int summerOccupiedHours,
            int maxExceedableSummerHours,
            int hoursExceedingComfortRange,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, tM52BuildingCategory, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
            this.summerOccupiedHours = summerOccupiedHours;
            this.maxExceedableSummerHours = maxExceedableSummerHours;
        }

        public int HoursExceedingComfortRange
        {
            get
            {
                return hoursExceedingComfortRange;
            }
        }

        public int SummerOccupiedHours
        {
            get
            {
                return summerOccupiedHours;
            }
        }

        public int MaxExceedableSummerHours
        {
            get
            {
                return maxExceedableSummerHours;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("HoursExceedingComfortRange"))
            {
                hoursExceedingComfortRange = jObject.Value<int>("HoursExceedingComfortRange");
            }

            if (jObject.ContainsKey("SummerOccupiedHours"))
            {
                summerOccupiedHours = jObject.Value<int>("SummerOccupiedHours");
            }

            if (jObject.ContainsKey("MaxExceedableSummerHours"))
            {
                maxExceedableSummerHours = jObject.Value<int>("MaxExceedableSummerHours");
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

            if (hoursExceedingComfortRange != int.MinValue)
            {
                result.Add("HoursExceedingComfortRange", hoursExceedingComfortRange);
            }

            if (summerOccupiedHours != int.MinValue)
            {
                result.Add("SummerOccupiedHours", summerOccupiedHours);
            }

            if (maxExceedableSummerHours != int.MinValue)
            {
                result.Add("MaxExceedableSummerHours", maxExceedableSummerHours);
            }

            return result;
        }
    }
}