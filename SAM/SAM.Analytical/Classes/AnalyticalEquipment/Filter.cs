using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an filter object in the analytical domain
    /// </summary>
    public class Filter : SimpleEquipment
    {
        public Filter(string name)
            : base(name)
        {

        }

        public Filter(JObject jObject)
            : base(jObject)
        {

        }

        public Filter(Filter filter)
            : base(filter)
        {

        }

        public Filter(Guid guid, string name)
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
