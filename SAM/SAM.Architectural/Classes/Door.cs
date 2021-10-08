using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public class Door : BuildingElement<DoorType>, IOpening
    {
        public Door(Door door)
            : base(door)
        {

        }

        public Door(JObject jObject)
            : base(jObject)
        {

        }

        public Door(DoorType doorType, Face3D face3D)
            : base(doorType, face3D)
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
