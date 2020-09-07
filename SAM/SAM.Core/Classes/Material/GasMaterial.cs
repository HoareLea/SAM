using Newtonsoft.Json.Linq;

using System;

namespace SAM.Core
{
    public class GasMaterial : Material
    {
        public GasMaterial(string name)
            : base(name)
        {

        }

        public GasMaterial(Guid guid, string name)
            : base(guid, name)
        {

        }

        public GasMaterial(JObject jObject)
            : base(jObject)
        {
        }

        public GasMaterial(GasMaterial gasMaterial)
            : base(gasMaterial)
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