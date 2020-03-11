using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Polyline2D : SAMGeometry, IBoundable2D, ISegmentable2D
    {
        private List<Point2D> points;

        public Polyline2D(IEnumerable<Point2D> point2Ds, bool close = false)
        {
            this.points = Point2D.Clone(point2Ds);
            if (close && !IsClosed())
                points.Add(points.First());
        }

        public Polyline2D(IEnumerable<Segment2D> segment2Ds)
        {
            points = new List<Point2D>() { segment2Ds.ElementAt(0).GetStart() };
            foreach (Segment2D segment2D in segment2Ds)
                points.Add(segment2D.GetEnd());
        }

        public Polyline2D(Polyline2D polyline2D)
        {
            this.points = Point2D.Clone(polyline2D.points);
        }

        public Polyline2D(JObject jObject)
            : base(jObject)
        {

        }

        public List<Point2D> Points
        {
            get
            {
                return new List<Point2D>(points);
            }
        }

        public List<Segment2D> GetSegments()
        {
            return Point2D.GetSegments(points, false);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
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

            points.Add(new Point2D(points.First()));
        }

        public void Open()
        {
            if (!IsClosed())
                return;

            points.RemoveAt(points.Count - 1);
        }

        public Polygon2D ToPolygon2D()
        {
            List<Point2D> point2Ds = new List<Point2D>(points);
            if (IsClosed())
                point2Ds.Remove(point2Ds.Last());

            return new Polygon2D(point2Ds);
        }

        public override ISAMGeometry Clone()
        {
            return new Polyline2D(this);
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(points);
        }

        public Point2D GetStart()
        {
            return new Point2D(points.First());
        }

        public Point2D GetEnd()
        {
            return new Point2D(points.Last());
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public ISAMGeometry2D GetMoved(Vector2D vector2D)
        {
            return new Polyline2D(points.ConvertAll(x => (Point2D)x.GetMoved(vector2D)));
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => (Point2D)x.Clone());
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Create.Point2Ds(jObject.Value<JArray>("Points"));
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
            List<Segment2D> segment2Ds = GetSegments();

            if (segment2Ds == null)
                return double.NaN;

            double length = 0;
            segment2Ds.ForEach(x => length += x.GetLength());
            return length;
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }
    }
}
