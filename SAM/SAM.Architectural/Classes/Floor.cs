using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public class Floor : HostBuildingElement
    {
        public Floor(Floor floor)
            : base(floor)
        {

        }

        public Floor(JObject jObject)
            : base(jObject)
        {

        }

        public Floor(FloorType floorType, Face3D face3D)
            : base(floorType, face3D)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

    }
}
