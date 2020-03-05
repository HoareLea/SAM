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
        
        public Polygon2D Offset(IEnumerable<double> offsets, Orientation orientation, double tolerance = Tolerance.MicroDistance)
        {
            throw new NotImplementedException();

            //if (points == null || points.Count < 3 || offsets == null)
            //    return null;

            //int count = offsets.Count();

            //if (count == 0)
            //    return new Polygon2D(this);

            //List<Segment2D> segment2Ds = GetSegments();
            //if (segment2Ds == null)
            //    return null;

            //double offset_Max = offsets.Max() + 1;

            //double offset = offsets.Last();

            //BoundingBox2D boundingBox2D = GetBoundingBox(offset_Max);

            //for(int i=0; i < segment2Ds.Count; i++)
            //{
            //    if (i < count)
            //        offset = offsets.ElementAt(i);

            //    Segment2D segment2D = segment2Ds[i].Offset(offset, orientation);

            //    Vector2D direction = segment2Ds[i].Direction;

            //    Segment2D segment2D_1 = boundingBox2D.GetSegment(segment2D.Start, direction);
                
            //    direction.Negate();
            //    Segment2D segment2D_2 = boundingBox2D.GetSegment(segment2D.End, direction);

            //    segment2Ds[i] = new Segment2D(segment2D_1.Start, segment2D_2.End);
            //}

            //segment2Ds = Modify.Split(segment2Ds);

            //CurveGraph2D curveGraph2D = null;

            //curveGraph2D = new CurveGraph2D(segment2Ds);
            //List<PolycurveLoop2D> polycurveLoop2Ds = curveGraph2D.GetPolycurveLoop2Ds();
            //if (polycurveLoop2Ds == null)
            //    return null;

            //List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            //foreach(PolycurveLoop2D polycurveLoop2D in polycurveLoop2Ds)
            //{
            //    Polygon2D polygon2D = polycurveLoop2D.ToPolygon2D();
            //    if (polygon2D.Distance(this) > tolerance)
            //        continue;

            //    polygon2Ds.Add(polygon2D);
            //}

            //if (polygon2Ds == null || polygon2Ds.Count == 0)
            //    return null;

            //curveGraph2D = new CurveGraph2D(polygon2Ds);
            //List<PolycurveLoop2D> polycurveLoop2Ds = curveGraph2D.GetPolycurveLoop2Ds_External();
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
           return new BoundingBox2D(points, offset);
        }

        public Point2D GetInternalPoint2D()
        {
            return Point2D.GetInternalPoint2D(points);
        }
    }
}
