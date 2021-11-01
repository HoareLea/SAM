using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using SAM.Architectural;

namespace SAM.Analytical
{
    public class WallType : HostPartitionType
    {
        public WallType(WallType wallType)
            : base(wallType)
        {

        }

        public WallType(WallType wallType, string name)
            : base(wallType, name)
        {

        }

        public WallType(JObject jObject)
            : base(jObject)
        {

        }

        public WallType(string name)
            : base(name)
        {

        }

        public WallType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public WallType(string name, IEnumerable<MaterialLayer> materialLayers)
            : base(name, materialLayers)
        {

        }

        public WallType(System.Guid guid, string name, IEnumerable<MaterialLayer> materialLayers)
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
