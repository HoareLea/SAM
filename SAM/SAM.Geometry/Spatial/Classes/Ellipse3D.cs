using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Spatial
{
    public class Ellipse3D: SAMGeometry, IClosedPlanar3D
    {
        private Ellipse2D ellipse2D;
        private Plane plane;

        public Ellipse3D(JObject jObject)
            : base(jObject)
        {

        }

        public Ellipse3D(Ellipse3D ellipse3D)
        {
            ellipse2D = new Ellipse2D(ellipse3D.ellipse2D);
            plane = new Plane(ellipse3D.plane);
        }

        public Ellipse3D(Plane plane, Ellipse2D ellipse2D)
        {
            this.ellipse2D = new Ellipse2D(ellipse2D);
            this.plane = new Plane(plane);
        }

        public override bool FromJObject(JObject jObject)
        {
            ellipse2D = new Ellipse2D(jObject.Value<JObject>("Ellipse2D"));
            plane = new Plane(jObject.Value<JObject>("Plane"));

            return true;
        }

        public double GetArea()
        {
            return ellipse2D.GetArea();
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new System.NotImplementedException();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Ellipse3D((Plane)plane.GetMoved(vector3D), ellipse2D);
        }

        public Plane GetPlane()
        {
            if (plane == null)
                return null;

            return new Plane(plane);
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        public void Reverse()
        {
            throw new System.NotImplementedException();
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Ellipse2D", ellipse2D.ToJObject());
            jObject.Add("Plane", plane.ToJObject());

            return jObject;
        }

        public override ISAMGeometry Clone()
        {
            return new Ellipse3D(plane, (Ellipse2D)ellipse2D?.Clone());
        }

        public Ellipse2D Ellipse2D
        {
            get
            {
                if (ellipse2D == null)
                {
                    return null;
                }

                return new Ellipse2D(ellipse2D);
            }
        }
    }
}
