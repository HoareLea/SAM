using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an empty section in the analytical domain
    /// </summary>
    public class EmptySection : SimpleEquipment
    {
        public EmptySection(string name)
            : base(name)
        {

        }

        public EmptySection(JObject jObject)
            : base(jObject)
        {

        }

        public EmptySection(Filter filter)
            : base(filter)
        {

        }

        public EmptySection(Guid guid, string name)
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
