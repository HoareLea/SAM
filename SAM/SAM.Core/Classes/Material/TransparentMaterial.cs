using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class TransparentMaterial : Material
    {
        public TransparentMaterial(string name)
            : base(name)
        {

        }

        public TransparentMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public TransparentMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public TransparentMaterial(TransparentMaterial transparentMaterial)
            : base(transparentMaterial)
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