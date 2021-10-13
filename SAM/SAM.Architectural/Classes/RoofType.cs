using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public class RoofType : HostPartitionType
    {
        public RoofType(RoofType roofType)
            : base(roofType)
        {

        }

        public RoofType(JObject jObject)
            : base(jObject)
        {

        }

        public RoofType(string name)
            : base(name)
        {

        }

        public RoofType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name, materialLayers)
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
