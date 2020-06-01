using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Segment3D : SAMGeometry, ICurve3D, ISegmentable3D
    {
        private Point3D origin;
        private Vector3D vector;

        public Segment3D(Point3D start, Point3D end)
        {
            origin = new Point3D(start);
            vector = new Vector3D(start, end);
        }

        public Segment3D(Segment3D segment3D)
        {
            origin = new Point3D(segment3D.origin);
            vector = new Vector3D(segment3D.vector);
        }

        public Segment3D(Point3D origin, Vector3D vector)
        {
            this.origin = new Point3D(origin);
            this.vector = new Vector3D(vector);
        }

        public Segment3D(JObject jObject)
            : base(jObject)
        {
        }

        public Point3D this[int index]
        {
            get
            {
                if (index == 0)
                    return origin;
                if (index == 1)
                    return (Point3D)origin.GetMoved(vector);

                return null;
            }
            set
            {
                if (index == 0)
                    origin = value;
                else if (index == 1)
                    vector = new Vector3D(origin, value);
            }
        }

        public Point3D GetStart()
        {
            return new Point3D(origin);
        }

        public Point3D GetEnd()
        {
            return (Point3D)origin.GetMoved(vector);
        }

        public Vector3D Direction
        {
            get
            {
                return vector.Unit;
            }
        }

        public List<Point3D> GetPoints()
        {
            return new List<Point3D>() { origin, this[1] };
        }

        public override ISAMGeometry Clone()
        {
            return new Segment3D(this);
        }

        public List<Segment3D> GetSegments()
        {
            return new List<Segment3D>() { new Segment3D(this) };
        }

        public List<ICurve3D> GetCurves()
        {
            return new List<ICurve3D>() { new Segment3D(this) };
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(new Point3D(origin), this[1], offset);
        }

        public void Reverse()
        {
            origin = GetEnd();
            vector.Negate();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Segment3D((Point3D)origin.GetMoved(vector3D), (Vector3D)vector.Clone());
        }

        public Point3D GetCenter()
        {
            return (Point3D)origin.GetMoved(vector / 2);
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

        public double GetLength()
        {
            return vector.Length;
        }

        public Point3D Closest(Point3D point3D)
        {
            Point3D start = GetStart();
            Point3D end = GetEnd();

            double A = point3D.X - start.X;
            double B = point3D.Y - start.Y;
            double E = point3D.Z - start.Z;
            double C = end.X - start.X;
            double D = end.Y - start.Y;
            double F = end.Z - start.Z;

            double aDot = A * C + B * D + E * F;
            double aLen_sq = C * C + D * D + F * F;
            double aParameter = -1;
            if (aLen_sq != 0)
                aParameter = aDot / aLen_sq;

            if (aParameter < 0)
                return start;
            else if (aParameter > 1)
                return end;
            else
                return new Point3D(start.X + aParameter * C, start.Y + aParameter * D, start.Z + aParameter * F);
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return Closest(point3D).Distance(point3D) < tolerance;
        }

        public Line3D GetLine3D()
        {
            return new Line3D(origin, vector);
        }

        public static explicit operator Segment3D(Line3D line3D)
        {
            if (line3D == null)
                return null;

            return new Segment3D(line3D.Origin, line3D.Direction);
        }

        public static List<Point3D> GetPoints(IEnumerable<Segment3D> segment3Ds, bool close = false)
        {
            if (segment3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>() { segment3Ds.First().GetStart() };
            foreach (Segment3D segment3D in segment3Ds)
                result.Add(segment3D.GetEnd());

            if (close && result.First().Distance(result.Last()) != 0)
                result.Add(result.First());

            return result;
        }

        public static Segment3D Snap(IEnumerable<Point3D> point3Ds, Segment3D segment3D, double maxDistance = double.NaN)
        {
            Point3D point3D_1 = Point3D.Snap(point3Ds, segment3D[0], maxDistance);
            Point3D point3D_2 = Point3D.Snap(point3Ds, segment3D[1], maxDistance);

            return new Segment3D(point3D_1, point3D_2);
        }

        public static bool operator ==(Segment3D segment3D_1, Segment3D segment3D_2)
        {
            if (ReferenceEquals(segment3D_1, null) && ReferenceEquals(segment3D_2, null))
                return true;

            if (ReferenceEquals(segment3D_1, null))
                return false;

            if (ReferenceEquals(segment3D_2, null))
                return false;

            return segment3D_1.origin == segment3D_2.origin && segment3D_1.vector == segment3D_2.vector;
        }

        public static bool operator !=(Segment3D segment3D_1, Segment3D segment3D_2)
        {
            if (ReferenceEquals(segment3D_1, null) && ReferenceEquals(segment3D_2, null))
                return false;

            if (ReferenceEquals(segment3D_1, null))
                return true;

            if (ReferenceEquals(segment3D_2, null))
                return true;

            return segment3D_1.origin != segment3D_2.origin || segment3D_1.vector != segment3D_2.vector;
        }
    }
}