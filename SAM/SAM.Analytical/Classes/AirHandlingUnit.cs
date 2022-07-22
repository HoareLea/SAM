using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class AirHandlingUnit : SAMObject, IAnalyticalEquipment
    {
        public AirHandlingUnit(string name)
            : base(name)
        {

        }

        public AirHandlingUnit(JObject jObject)
            : base(jObject)
        {

        }

        public AirHandlingUnit(AirHandlingUnit airHandlingUnit)
            : base(airHandlingUnit)
        {

        }

        public AirHandlingUnit(Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;


            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            return jObject;
        }
    }
}