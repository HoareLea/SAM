using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM59NaturalVentilationResult : TM59Result
    {
        private int hoursExceedingComfortRange;

        public TM59NaturalVentilationResult(
            string name, 
            string source, 
            string reference, 
            int occupiedHours, 
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
        }

        public TM59NaturalVentilationResult(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceedingComfortRange,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceedingComfortRange = hoursExceedingComfortRange;
        }

        public int HoursExceedingComfortRange
        {
            get
            {
                return hoursExceedingComfortRange;
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

            return result;
        }
    }
}