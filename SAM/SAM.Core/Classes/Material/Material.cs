using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public abstract class Material : SAMObject, IMaterial
    {
        public Material(string name)
            : base(name)
        {

        }

        public Material(Guid guid, string name)
            : base(guid, name)
        {

        }

        public Material(JObject jObject)
            : base(jObject)
        {
        }

        public Material(Material material)
            : base(material)
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
                return jObject;

            return jObject;
        }
    }
}