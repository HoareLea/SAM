using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Polygon2D : SAMGeometry, IClosed2D, ISegmentable2D
    {
        private List<Point2D> points;

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this.points = Point2D.Clone(points);
        }

        public Polygon2D(Polygon2D polygon2D)
        {
            this.points = polygon2D.GetPoints();
        }

        public Polygon2D(JObject jObject)
            : base(jObject)
        {

        }

        public List<Segment2D> GetSegments()
        {
            return Point2D.GetSegments(points, true);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public Orientation GetOrientation()
        {
            return Point2D.Orientation(points, true);
        }

        public override ISAMGeometry Clone()
        {
            return new Polygon2D(this);
        }

        public List<Point2D> Points
        {
            get
            {
                return Point2D.Clone(points);
            }
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => new Point2D(x));
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public bool Inside(Point2D point2D)
        {
            return Point2D.Inside(points, point2D);
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public Point2D Closest(Point2D point2D)
        {
            return Query.Closest(points, point2D);
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public double GetArea()
        {
            return Point2D.GetArea(points);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Core.Create.JArray(points));
            return jObject;
        }

        public Polygon2D Offset(double offset, Orientation orientation)
        {
            return Offset(new double[] { offset }, orientation);
        }

        public Polygon2D Offset(IEnumerable<double> offsets, Orientation orientation)
        {
            if (points == null || points.Count < 3 || offsets == null)
                return null;

            int count = offsets.Count();

            if (count == 0)
                return new Polygon2D(this);

            List<Segment2D> segment2Ds = GetSegments();
            if (segment2Ds == null || segment2Ds.Count() < 3)
                return null;

            segment2Ds.Insert(0, segment2Ds.Last());
            segment2Ds.Add(segment2Ds[1]);

            double offset = offsets.Last();

            List<Segment2D[]> segments2Ds = new List<Segment2D[]>();
            for (int i = 1; i < segment2Ds.Count - 1; i++)
            {
                if (i < count)
                    offset = offsets.ElementAt(i);

                Segment2D segment2D_Previous = segment2Ds[i - 1];
                Segment2D segment2D = segment2Ds[i];
                Segment2D segment2D_Next = segment2Ds[i + 1];

                Segment2D segment2D_Offset = segment2D.Offset(offset, orientation);

                Vector2D Vector2D_Previous = Query.MidVector(segment2D_Previous, segment2D);
                Vector2D Vector2D_Next = Query.MidVector(segment2D, segment2D_Next);

                Segment2D segment2D_Vector_Previous = new Segment2D(segment2D_Previous.End, Vector2D_Previous);
                Segment2D segment2D_Vector_Next = new Segment2D(segment2D_Next.Start, Vector2D_Next);

                Point2D point2D_Intersection_Previous = segment2D_Offset.Intersection(segment2D_Vector_Previous, false);
                if (point2D_Intersection_Previous == null)
                    continue;

                Point2D point2D_Intersection_Next = segment2D_Offset.Intersection(segment2D_Vector_Next, false);
                if (point2D_Intersection_Next == null)
                    continue;

                Segment2D segment2D_Offset_New = new Segment2D(point2D_Intersection_Previous, point2D_Intersection_Next);

                if (!segment2D_Offset_New.Direction.AlmostEqual(segment2D.Direction))
                    continue;

                segments2Ds.Add(new Segment2D[] { segment2D_Offset_New, new Segment2D(segment2D.Start, point2D_Intersection_Previous), new Segment2D(segment2D.Start, point2D_Intersection_Previous) });
            }

            return new Polygon2D(segments2Ds.ConvertAll(x => x[0].Start));
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
           return new BoundingBox2D(points, offset);
        }

        public Point2D GetInternalPoint2D()
        {
            return Point2D.GetInternalPoint2D(points);
        }

        public bool On(Point2D point2D, double tolerance = 1E-09)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }
    }
}
