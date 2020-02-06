using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class BoundingBox2D : SAMGeometry, IClosed2D, ISegmentable2D
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
            max = Point2D.Max(point2D_1, point2D_2);
            min = Point2D.Min(point2D_1, point2D_2);
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
                    max = Point2D.Max(max, value);
                    min = Point2D.Min(max, value);
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
                    max = Point2D.Max(min, value);
                    min = Point2D.Min(min, value);
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
            if(closed2D is BoundingBox2D)
                return Inside((BoundingBox2D)closed2D);
            
            if(closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }
        
        public bool Inside(BoundingBox2D boundingBox2D)
        {
            return Inside(boundingBox2D.max) && Inside(boundingBox2D.min);
        }

        public bool Inside(Point2D point2D)
        {
            return point2D.X > min.X && point2D.X < max.X && point2D.Y < max.Y && point2D.Y > min.Y;
        }

        public bool Inside(Point2D point2D, bool acceptOnEdge = true, double tolerance = Tolerance.MicroDistance)
        {
            if (point2D == null)
                return false;

            if (acceptOnEdge)
                return (point2D.X >= min.X - tolerance && point2D.X <= max.X + tolerance && point2D.Y >= min.Y - tolerance && point2D.Y <= max.Y + tolerance);

            return (point2D.X > min.X + tolerance && point2D.X < max.X - tolerance && point2D.Y > min.Y + tolerance && point2D.Y < max.Y - tolerance);
        }

        public override ISAMGeometry Clone()
        {
            return new BoundingBox2D(this);
        }

        public List<Segment2D> GetSegments()
        {
            List<Point2D> points = GetPoints();

            return new List<Segment2D>() {new Segment2D(points[0], points[1]), new Segment2D(points[1], points[2]), new Segment2D(points[2], points[3]), new Segment2D(points[3], points[0]) };
        }

        public List<Point2D> GetPoints()
        {
            double x = Width;
            double y = Height;

            return new List<Point2D>() { new Point2D(min), new Point2D(min.X, min.Y + y), new Point2D(max), new Point2D(max.X, max.Y - y) };
        }

        public double GetArea()
        {
            return Width * Height;
        }

        public override bool FromJObject(JObject jObject)
        {
            max = new Point2D(jObject.Value<JObject>("Max"));
            min = new Point2D(jObject.Value<JObject>("Min"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", GetType().FullName);
            jObject.Add("Max", max.ToJObject());
            jObject.Add("Min", min.ToJObject());

            return jObject;
        }
    }
}
