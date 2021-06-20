using Newtonsoft.Json.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Architectural
{
    public class Room : Core.SAMObject, ISAMGeometry3DObject
    {
        private Point3D location;

        public Room(Room room)
            : base(room)
        {
            location = room?.location;
        }

        public Room(JObject jObject)
            : base(jObject)
        {

        }

        public Room(string name, Point3D location)
            : base(name)
        {
            this.location = location;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            location = new Point3D(jObject.Value<JObject>("Location"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (location != null)
                jObject.Add("Location", location.ToJObject());

            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }

        public void Transform(Transform3D transform3D)
        {
            location = location?.Transform(transform3D);
        }

        public void Move(Vector3D vector3D)
        {
            location = location?.GetMoved(vector3D) as Point3D;
        }

    }
}
