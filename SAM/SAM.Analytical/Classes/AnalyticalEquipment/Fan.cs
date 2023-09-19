using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an fan object in the analytical domain
    /// </summary>
    public class Fan : SimpleEquipment, ISection
    {
        public Fan()
            : base("Fan")
        {

        }

        public Fan(string name)
            : base(name)
        {

        }

        public Fan(JObject jObject)
            : base(jObject)
        {

        }

        public Fan(Fan fan)
            : base(fan)
        {

        }

        public Fan(Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            // TODO: Implement specific deserialization logic for AirHandlingUnit properties

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            // TODO: Implement specific serialization logic for AirHandlingUnit properties

            return jObject;
        }
    }
}
