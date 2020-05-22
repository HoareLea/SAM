using Newtonsoft.Json.Linq;

using SAM.Core;

namespace SAM.Architectural
{
    public class Level : SAMObject
    {
        private double elevation;

        public Level(Level level)
            : base(level)
        {
            elevation = level.elevation;
        }

        public Level(double elevation)
            : base()
        {
            this.elevation = elevation;
        }

        public Level(string name, double elevation)
            : base(name)
        {
            this.elevation = elevation;
        }

        public Level(JObject jObject)
            : base(jObject)
        {
        }

        public double Elevation
        {
            get
            {
                return elevation;
            }
        }

        public Geometry.Spatial.Plane GetPlane()
        {
            return new Geometry.Spatial.Plane(new Geometry.Spatial.Point3D(0, 0, elevation), Geometry.Spatial.Vector3D.WorldZ);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            elevation = jObject.Value<double>("Elevation");
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            jObject.Add("Elevation", elevation);

            return jObject;
        }
    }
}