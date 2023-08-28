using Newtonsoft.Json.Linq;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// Represents an mixing section in the analytical domain
    /// </summary>
    public class MixingSection : SimpleEquipment
    {
        public MixingSection()
            : base("Mixing Section")
        {

        }

        public MixingSection(string name)
            : base(name)
        {

        }

        public MixingSection(JObject jObject)
            : base(jObject)
        {

        }

        public MixingSection(MixingSection mixingSection)
            : base(mixingSection)
        {

        }

        public MixingSection(Guid guid, string name)
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
