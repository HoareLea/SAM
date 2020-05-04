using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Planar
{
    public class Line2D : SAMGeometry, ISAMGeometry2D
    {
        private Point2D origin;
        private Vector2D vector;

        public Line2D(JObject jObject)
            : base(jObject)
        {
        }

        public Line2D(Point2D origin, Vector2D vector)
        {
            this.origin = origin;
            this.vector = vector;
        }

        public Line2D(Line2D line2D)
        {
            origin = new Point2D(line2D.origin);
            vector = new Vector2D(line2D.vector);
        }

        public Vector2D Direction
        {
            get
            {
                return new Vector2D(vector);
            }
        }

        public Point2D Origin
        {
            get
            {
                return new Point2D(origin);
            }
        }

        public override ISAMGeometry Clone()
        {
            return new Line2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point2D(jObject.Value<JObject>("Origin"));
            vector = new Vector2D(jObject.Value<JObject>("Vector"));

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