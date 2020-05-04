using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Polyline3D : SAMGeometry, IBoundable3D, ISegmentable3D, ICurve3D
    {
        private List<Point3D> points;

        public Polyline3D(IEnumerable<Point3D> point3Ds, bool close = false)
        {
            this.points = Point3D.Clone(point3Ds);
            if (close && !IsClosed())
                points.Add(points.First());
        }

        public Polyline3D(IEnumerable<Segment3D> segment3Ds, bool close = false)
        {
            points = new List<Point3D>() { segment3Ds.ElementAt(0).GetStart() };
            foreach (Segment3D segment3D in segment3Ds)
                points.Add(segment3D.GetEnd());
        }

        public Polyline3D(Polyline3D polyline3D)
        {
            this.points = Point3D.Clone(polyline3D.points);
        }

        public Polyline3D(JObject jObject)
            : base(jObject)
        {
        }

        public List<Point3D> Points
        {
            get
            {
                return new List<Point3D>(points);
            }
        }

        public List<Segment3D> GetSegments()
        {
            return Point3D.GetSegments(points, false);
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x);
        }

        public bool IsClosed()
        {
            return points.First().Equals(points.Last());
        }

        public bool IsClosed(double tolerance)
        {
            return points.First().Distance(points.Last()) < tolerance;
        }

        public void Close()
        {
            if (IsClosed())
                return;

            points.Add(new Point3D(points.First()));
        }

        public void Open()
        {
            if (!IsClosed())
                return;

            points.RemoveAt(points.Count - 1);
        }

        public Polygon3D ToPolygon3D()
        {
            return new Polygon3D(points);
        }

        public override ISAMGeometry Clone()
        {
            return new Polyline3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(points);
        }

        public Point3D GetStart()
        {
            return new Point3D(points.First());
        }

        public Point3D GetEnd()
        {
            return new Point3D(points.Last());
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Polyline3D(points.ConvertAll(x => (Point3D)x.GetMoved(vector3D)));
        }

        public List<Point3D> GetPoints()
        {
            return points.ConvertAll(x => (Point3D)x.Clone());
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Create.Point3Ds(jObject.Value<JArray>("Points"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Geometry.Create.JArray(points));
            return jObject;
        }

        public double GetLength()
        {
            List<Segment3D> segments3D = GetSegments();

            if (segments3D == null)
                return double.NaN;

            double length = 0;
            segments3D.ForEach(x => length += x.GetLength());
            return length;
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(this, point3D, tolerance);
        }
    }
}