using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using SAM.Architectural;

namespace SAM.Analytical
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

        public RoofType(System.Guid guid, string name)
        : base(guid, name)
        {

        }

        public RoofType(RoofType roofType, string name)
            : base(roofType, name)
        {

        }

        public RoofType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name, materialLayers)
        {

        }

        public RoofType(System.Guid guid, string name, IEnumerable<MaterialLayer> materialLayers)
            : base(guid, name, materialLayers)
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
