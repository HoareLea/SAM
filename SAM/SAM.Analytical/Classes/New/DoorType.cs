using Newtonsoft.Json.Linq;
using SAM.Architectural;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class DoorType : OpeningType
    {
        public DoorType(DoorType doorType)
            : base(doorType)
        {

        }

        public DoorType(DoorType doorType, string name)
            : base(doorType, name)
        {

        }

        public DoorType(JObject jObject)
            : base(jObject)
        {

        }

        public DoorType(string name)
            : base(name)
        {

        }

        public DoorType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public DoorType(string name, IEnumerable<MaterialLayer> paneMaterialLayers, IEnumerable<MaterialLayer> frameMaterialLayers = null)
            : base(name, paneMaterialLayers, frameMaterialLayers)
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
