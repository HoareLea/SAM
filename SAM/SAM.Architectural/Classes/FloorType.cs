using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public class FloorType : HostPartitionType
    {
        public FloorType(FloorType floorType)
            : base(floorType)
        {

        }

        public FloorType(JObject jObject)
            : base(jObject)
        {

        }

        public FloorType(string name)
            : base(name)
        {

        }

        public FloorType(string name, IEnumerable<MaterialLayer> materialLayers)
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
