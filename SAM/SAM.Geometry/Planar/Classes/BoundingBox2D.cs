using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class BoundingBox2D : SAMGeometry, IClosed2D, ISegmentable2D, IBoundable2D
    {
        private Point2D min;
        private Point2D max;

        public BoundingBox2D(IEnumerable<Point2D> point2Ds)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D.X > aX_Max)
                    aX_Max = point2D.X;
                if (point2D.X < aX_Min)
                    aX_Min = point2D.X;
                if (point2D.Y > aY_Max)
                    aY_Max = point2D.Y;
                if (point2D.Y < aY_Min)
                    aY_Min = point2D.Y;
            }

            min = new Point2D(aX_Min, aY_Min);
            max = new Point2D(aX_Max, aY_Max);
        }

        public BoundingBox2D(Point2D point2D_1, Point2D point2D_2)
        {
            max = Query.Max(point2D_1, point2D_2);
            min = Query.Min(point2D_1, point2D_2);
        }

        public BoundingBox2D(Point2D point2D_1, Point2D point2D_2, double offset)
        {
            max = Query.Max(point2D_1, point2D_2);
            min = Query.Min(point2D_1, point2D_2);

            min = new Point2D(min.X - offset, min.Y - offset);
            max = new Point2D(max.X + offset, max.Y + offset);
        }

        public BoundingBox2D(Point2D point2D, double offset)
        {
            min = new Point2D(point2D.X - offset, point2D.Y - offset);
            max = new Point2D(point2D.X + offset, point2D.Y + offset);
        }

        public BoundingBox2D(IEnumerable<Point2D> point2Ds, double offset)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D.X > aX_Max)
                    aX_Max = point2D.X;
                if (point2D.X < aX_Min)
                    aX_Min = point2D.X;
                if (point2D.Y > aY_Max)
                    aY_Max = point2D.Y;
                if (point2D.Y < aY_Min)
                    aY_Min = point2D.Y;
            }

            min = new Point2D(aX_Min - offset, aY_Min - offset);
            max = new Point2D(aX_Max + offset, aY_Max + offset);
        }

        public BoundingBox2D(BoundingBox2D boundingBox2D)
        {
            min = new Point2D(boundingBox2D.min);
            max = new Point2D(boundingBox2D.max);
        }

        public BoundingBox2D(IEnumerable<BoundingBox2D> boundingBox2Ds)
        {
            if (boundingBox2Ds != null)
            {
                HashSet<Point2D> point2Ds_Min = new HashSet<Point2D>();
                HashSet<Point2D> point2Ds_Max = new HashSet<Point2D>();
                foreach (BoundingBox2D boundingBox2D in boundingBox2Ds)
                {
                    point2Ds_Min.Add(boundingBox2D.min);
                    point2Ds_Max.Add(boundingBox2D.max);
                }

                min = Query.Min(point2Ds_Min);
                max = Query.Max(point2Ds_Max);
            }
        }

        public BoundingBox2D(JObject jObject)
            : base(jObject)
        {
        }

        public Point2D Min
        {
            get
            {
                return new Point2D(min);
            }
            set
            {
                if (max == null)
                {
                    max = new Point2D(value);
                    min = new Point2D(value);
                }
                else
                {
                    max = Query.Max(max, value);
                    min = Query.Min(max, value);
                }
            }
        }

        public Point2D Max
        {
            get
            {
                return new Point2D(max);
            }
            set
            {
                if (min == null)
                {
                    max = new Point2D(value);
                    min = new Point2D(value);
                }
                else
                {
                    max = Query.Max(min, value);
                    min = Query.Min(min, value);
                }
            }
        }

        public double Width
        {
            get
            {
                return max.X - min.X;
            }
        }

        public double Height
        {
            get
            {
                return max.Y - min.Y;
            }
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is BoundingBox2D)
                return Inside((BoundingBox2D)closed2D);

            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public bool Inside(BoundingBox2D boundingBox2D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            return Inside(boundingBox2D.max, acceptOnEdge, tolerance) && Inside(boundingBox2D.min, acceptOnEdge, tolerance);
        }

        public bool Inside(Point2D point2D)
        {
            if (point2D == null)
                return false;

            return point2D.X > min.X && point2D.X < max.X && point2D.Y < max.Y && point2D.Y > min.Y;
        }

        public bool Inside(Point2D point2D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return false;

            if (acceptOnEdge)
                return (point2D.X >= min.X - tolerance && point2D.X <= max.X + tolerance && point2D.Y >= min.Y - tolerance && point2D.Y <= max.Y + tolerance);

            return (point2D.X > min.X + tolerance && point2D.X < max.X - tolerance && point2D.Y > min.Y + tolerance && point2D.Y < max.Y - tolerance);
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public Segment2D GetSegment(Point2D point2D, Vector2D direction)
        {
            if (point2D == null || direction == null)
                return null;

            List<Segment2D> segment2Ds = GetSegments();
            if (segment2Ds == null)
                return null;

            Segment2D segment2D = new Segment2D(point2D, direction);

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Segment2D segment2D_Temp in segment2Ds)
            {
                Point2D point2D_Closest_1 = null;
                Point2D point2D_Closest_2 = null;
                Point2D point2D_Intersection = segment2D_Temp.Intersection(segment2D, out point2D_Closest_1, out point2D_Closest_2);
                if (point2D_Intersection == null)
                    continue;

                point2Ds.Add(point2D_Intersection);
            }

            if (point2Ds == null)
                return null;

            Point2D point2D_Closest = Query.Closest(point2Ds, point2D);
            if (point2D_Closest == null)
                return null;

            return new Segment2D(point2D, point2D_Closest);
        }

        public override ISAMGeometry Clone()
        {
            return new BoundingBox2D(this);
        }

        public List<Segment2D> GetSegments()
        {
            List<Point2D> points = GetPoints();

            return new List<Segment2D>() { new Segment2D(points[0], points[1]), new Segment2D(points[1], points[2]), new Segment2D(points[2], points[3]), new Segment2D(points[3], points[0]) };
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public List<Point2D> GetPoints()
        {
            double y = Height;

            return new List<Point2D>() { new Point2D(min), new Point2D(min.X, min.Y + y), new Point2D(max), new Point2D(max.X, max.Y - y) };
        }

        public Point2D GetPoint(Corner corner)
        {
            if (corner == Corner.Undefined)
                return null;

            switch (corner)
            {
                case Corner.BottomLeft:
                    return new Point2D(min);

                case Corner.BottomRight:
                    return new Point2D(max.X, max.Y - Height);

                case Corner.TopLeft:
                    return new Point2D(min.X, min.Y + Height);

                case Corner.TopRight:
                    return new Point2D(max);
            }

            return null;
        }

        public Corner GetCorner(Point2D point2D)
        {
            if (point2D == null)
                return Corner.Undefined;

            Corner result = Corner.Undefined;
            double distance_Min = double.MaxValue;

            foreach (Corner corner in Enum.GetValues(typeof(Corner)))
            {
                Point2D point2D_Temp = GetPoint(corner);
                if (point2D_Temp == null)
                    continue;

                double distance = point2D.Distance(point2D_Temp);
                if (distance < distance_Min)
                {
                    result = corner;
                    distance_Min = distance;
                }
            }

            return result;
        }

        public double GetArea()
        {
            return Width * Height;
        }

        public Point2D GetCentroid()
        {
            return Point2D.Mid(min, max);
        }

        public Point2D GetInternalPoint2D()
        {
            return Point2D.Mid(min, max);
        }

        public override bool FromJObject(JObject jObject)
        {
            max = new Point2D(jObject.Value<JObject>("Max"));
            min = new Point2D(jObject.Value<JObject>("Min"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Max", max.ToJObject());
            jObject.Add("Min", min.ToJObject());

            return jObject;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(min, max, offset);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Parameter(this, point2D, inverted, tolerance);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Query.Trim(this, parameter, inverted);
        }

        public bool Include(BoundingBox2D boundingBox2D)
        {
            if (boundingBox2D == null)
                return false;

            max = Query.Max(max, boundingBox2D.Max);
            min = Query.Min(min, boundingBox2D.Min);
            return true;
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }

        public Segment2D[] GetDiagonals()
        {
            List<Point2D> point2Ds = GetPoints();

            return new Segment2D[] { new Segment2D(point2Ds[0], point2Ds[2]), new Segment2D(point2Ds[1], point2Ds[3]) };
        }
    }
}