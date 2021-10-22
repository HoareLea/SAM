using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Floor : HostPartition<FloorType>
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

        public Floor(System.Guid guid,FloorType floorType, Face3D face3D)
            : base(guid, floorType, face3D)
        {

        }

        public Floor(System.Guid guid, Floor floor, Face3D face3D, double tolerance = Core.Tolerance.Distance)
            : base(guid, floor, face3D, tolerance)
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
