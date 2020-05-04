using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Spatial
{
    public class Line3D : SAMGeometry, ISAMGeometry3D
    {
        private Point3D origin;
        private Vector3D vector;

        public Line3D(Line3D line3D)
        {
            origin = new Point3D(line3D.origin);
            vector = new Vector3D(line3D.vector);
        }

        public Line3D(Point3D origin, Vector3D vector)
        {
            this.origin = new Point3D(origin);
            this.vector = vector.Unit;
        }

        public Line3D(JObject jObject)
            : base(jObject)
        {
        }

        public Vector3D Direction
        {
            get
            {
                return new Vector3D(vector);
            }
        }

        public Point3D Origin
        {
            get
            {
                return new Point3D(origin);
            }
        }

        public override ISAMGeometry Clone()
        {
            return new Line3D(this);
        }

        public void Reverse()
        {
            vector.Negate();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Segment3D((Point3D)origin.GetMoved(vector3D), (Vector3D)vector.Clone());
        }

        public Point3D Project(Point3D point3D)
        {
            double t = (point3D - origin).DotProduct(vector);
            return origin.GetMoved(vector * t) as Point3D;
        }

        public Segment3D Bound(Point3D start, Point3D end)
        {
            return new Segment3D(Project(start), Project(end));
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point3D(jObject.Value<JObject>("Origin"));
            vector = new Vector3D(jObject.Value<JObject>("Vector"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }
    }
}