using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public class Wall : HostPartition<WallType>
    {
        public Wall(Wall wall)
            : base(wall)
        {

        }

        public Wall(JObject jObject)
            : base(jObject)
        {

        }

        public Wall(WallType wallType, Face3D face3D)
            : base(wallType, face3D)
        {

        }

        public Wall(System.Guid guid, WallType wallType, Face3D face3D)
            : base(guid, wallType, face3D)
        {

        }

        public Wall(System.Guid guid, Wall wall, Face3D face3D, double tolerance = Core.Tolerance.Distance)
            : base(guid, wall, face3D, tolerance)
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
