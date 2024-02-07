using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM59MechanicalVentilationResult : TM59Result
    {
        private int hoursExceeding26;

        public TM59MechanicalVentilationResult(
            string name, 
            string source, 
            string reference, 
            int occupiedHours, 
            int maxExceedableHours,
            int hoursExceeding26,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceeding26 = hoursExceeding26;
        }

        public TM59MechanicalVentilationResult(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding26,
            bool pass,
            params TM59SpaceApplication[] tM59SpaceApplications)
            : base(guid, name, source, reference, occupiedHours, maxExceedableHours, pass, tM59SpaceApplications)
        {
            this.hoursExceeding26 = hoursExceeding26;
        }

        public int HoursExceeding26
        {
            get
            {
                return hoursExceeding26;
            }
        }


        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("HoursExceeding26"))
            {
                hoursExceeding26 = jObject.Value<int>("HoursExceeding26");
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

            if (hoursExceeding26 != int.MinValue)
            {
                result.Add("HoursExceeding26", hoursExceeding26);
            }

            return result;
        }
    }
}