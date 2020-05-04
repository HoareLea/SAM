using Newtonsoft.Json.Linq;
using SAM.Geometry.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Polyline2D : SAMGeometry, IBoundable2D, ISegmentable2D, IEnumerable<Segment2D>, IReversible
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
            return Create.Segment2Ds(points, false);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public bool IsClosed()
        {
            return points.First().Equals(points.Last());
        }

        public bool IsClosed(double tolerance = Core.Tolerance.Distance)
        {
            return points.First().Distance(points.Last()) < tolerance;
        }

        public Point2D Closest(Point2D point2D, bool includeEdges)
        {
            if (includeEdges)
                return Query.Closest(this, point2D);

            return Query.Closest(points, point2D);
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

        public Segment2D First()
        {
            if (points == null || points.Count < 2)
                return null;

            return new Segment2D(points[0], points[1]);
        }

        public Segment2D Last()
        {
            if (points == null)
                return null;

            int count = points.Count;

            if (count < 2)
                return null;

            return new Segment2D(points[count - 2], points[count - 1]);
        }

        public ISAMGeometry2D GetMoved(Vector2D vector2D)
        {
            return new Polyline2D(points.ConvertAll(x => (Point2D)x.GetMoved(vector2D)));
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => (Point2D)x.Clone());
        }

        public int CountPoints()
        {
            return points.Count;
        }

        public int CountSegments()
        {
            return points.Count - 1;
        }

        public Point2D this[int index]
        {
            get
            {
                return points[index];
            }
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

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public bool Add(Segment2D segment2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null || segment2D.GetLength() < tolerance)
                return false;

            if (points == null)
                points = new List<Point2D>();

            Point2D point2D_Start = segment2D.Start;
            Point2D point2D_End = segment2D.End;

            if (points.Count == 0)
            {
                points.Add(point2D_Start);
                points.Add(point2D_End);
                return true;
            }

            Point2D point2D;

            point2D = points[0];

            if (point2D.Distance(point2D_Start) < tolerance)
            {
                points.Insert(0, point2D_Start);
                points.Insert(0, point2D_End);
                return true;
            }

            if (point2D.Distance(point2D_End) < tolerance)
            {
                points.Insert(0, point2D_End);
                points.Insert(0, point2D_Start);
                return true;
            }

            point2D = points.Last();

            if (point2D.Distance(point2D_End) < tolerance)
            {
                points.Add(point2D_End);
                points.Add(point2D_Start);
                return true;
            }

            if (point2D.Distance(point2D_Start) < tolerance)
            {
                points.Add(point2D_Start);
                points.Add(point2D_End);
                return true;
            }

            return false;
        }

        public Point2D ClosestEnd(Point2D point2D)
        {
            Point2D end = GetEnd();
            Point2D start = GetStart();

            if (end.Distance(point2D) < start.Distance(point2D))
                return end;
            else
                return start;
        }

        public int GetEndIndex(Point2D point2D)
        {
            Point2D end = GetEnd();
            Point2D start = GetStart();

            if (end.Distance(point2D) < start.Distance(point2D))
                return points.Count - 1;
            else
                return 0;
        }

        public int GetSegmentIndex(Point2D point2D)
        {
            Point2D end = GetEnd();
            Point2D start = GetStart();

            if (end.Distance(point2D) < start.Distance(point2D))
                return points.Count - 2;
            else
                return 0;
        }

        public Segment2D GetSegment(int index)
        {
            return GetSegments()[index];
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Parameter(this, point2D, inverted, tolerance);
        }

        //Inserts new point on one of the edges (closest to given point2D)
        public Point2D InsertClosest(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Modify.InsertClosest(points, point2D, false, tolerance);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Query.Trim(this, parameter, inverted);
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public IEnumerator<Segment2D> GetEnumerator()
        {
            return GetSegments().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}