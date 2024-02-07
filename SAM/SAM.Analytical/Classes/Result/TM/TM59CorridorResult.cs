using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    public class TM59CorridorResult : TM59Result
    {
        private int hoursExceeding28;

        public TM59CorridorResult(
            string name, 
            string source, 
            string reference, 
            int occupiedHours, 
            int maxExceedableHours,
            int hoursExceeding28,
            bool pass)
            : base(name, source, reference, occupiedHours, maxExceedableHours, pass, TM59SpaceApplication.Undefined)
        {
            this.hoursExceeding28 = hoursExceeding28;
        }

        public TM59CorridorResult(
            Guid guid, 
            string name, 
            string source, 
            string reference,
            int occupiedHours,
            int maxExceedableHours,
            int hoursExceeding28,
            bool pass)
            : base(guid, name, source, reference, occupiedHours, maxExceedableHours, pass, TM59SpaceApplication.Undefined)
        {
            this.hoursExceeding28 = hoursExceeding28;
        }

        public int HoursExceeding28
        {
            get
            {
                return hoursExceeding28;
            }
        }


        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("HoursExceeding28"))
            {
                hoursExceeding28 = jObject.Value<int>("HoursExceeding28");
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

            if (hoursExceeding28 != int.MinValue)
            {
                result.Add("HoursExceeding28", hoursExceeding28);
            }

            return result;
        }
    }
}