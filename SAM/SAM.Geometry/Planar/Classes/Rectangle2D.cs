using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Rectangle2D : SAMGeometry, IClosed2D, ISegmentable2D
    {
        private Point2D origin;
        private double width;
        private double height;
        private Vector2D heightDirection;

        public Rectangle2D(double width, double height)
        {
            origin = Point2D.Zero;
            this.width = width;
            this.height = height;
            heightDirection = Vector2D.WorldY;
        }

        public Rectangle2D(Point2D origin, double width, double height)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
            heightDirection = Vector2D.WorldY;
        }

        public Rectangle2D(Point2D origin, double width, double height, Vector2D heightDirection)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
            this.heightDirection = heightDirection.Unit;
        }

        public Rectangle2D(Rectangle2D rectangle2D)
        {
            this.origin = new Point2D(rectangle2D.origin);
            this.width = rectangle2D.width;
            this.height = rectangle2D.height;
            this.heightDirection = new Vector2D(rectangle2D.heightDirection);
        }

        public Rectangle2D(JObject jObject)
            : base(jObject)
        {
        }

        public List<Point2D> GetPoints()
        {
            List<Point2D> points = new List<Point2D>();
            points.Add(new Point2D(origin));

            Point2D point2D = null;

            Vector2D heightVector = height * heightDirection;

            point2D = new Point2D(origin);
            point2D.Move(heightVector);
            points.Add(point2D);

            Vector2D widthDirection = WidthDirection;
            Vector2D widthVector = width * widthDirection;

            point2D = new Point2D(point2D);
            point2D.Move(widthVector);
            points.Add(point2D);

            point2D = new Point2D(point2D);
            heightVector.Negate();
            point2D.Move(heightVector);
            points.Add(point2D);

            return points;
        }

        public List<Segment2D> GetSegments()
        {
            List<Point2D> points = GetPoints();

            List<Segment2D> segments = new List<Segment2D>();
            segments.Add(new Segment2D(points[0], points[1]));
            segments.Add(new Segment2D(points[1], points[2]));
            segments.Add(new Segment2D(points[2], points[3]));
            segments.Add(new Segment2D(points[3], points[0]));
            return segments;
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public Orientation GetOrientation()
        {
            List<Point2D> point2Ds = GetPoints();

            return Query.Orientation(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public bool Contains(Point2D point, double offset)
        {
            return Point2D.Contains(GetPoints(), point, offset);
        }

        public Vector2D WidthDirection
        {
            get
            {
                return heightDirection.GetPerpendicular(Orientation.Clockwise);
            }
        }

        public Vector2D HeightDirection
        {
            get
            {
                return heightDirection;
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public Point2D Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = new Point2D(value);
            }
        }

        public double GetArea()
        {
            return height * width;
        }

        public Point2D GetCentroid()
        {
            return Query.Centroid(GetPoints());
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(GetPoints(), offset);
        }

        public void Move(Vector2D vector2D)
        {
            origin.Move(vector2D);
        }

        public Rectangle2D GetMoved(Vector2D Vector2D)
        {
            return new Rectangle2D((Point2D)origin.GetMoved(Vector2D), width, height, heightDirection);
        }

        public double GetPerimeter()
        {
            return width * height;
        }

        public Segment2D[] GetDiagonals()
        {
            List<Point2D> points = GetPoints();

            return new Segment2D[] { new Segment2D(points[0], points[2]), new Segment2D(points[1], points[3]) };
        }

        public void Scale(Point2D point2D, double factor)
        {
            origin.Scale(point2D, factor);
            width = width * factor;
            height = height * factor;
        }

        public IEnumerable<Triangle2D> Triangulate()
        {
            List<Point2D> points = GetPoints();

            return new Triangle2D[] { new Triangle2D(points[0], points[1], points[2]), new Triangle2D(points[2], points[3], points[0]) };
        }

        public override ISAMGeometry Clone()
        {
            return new Rectangle2D(this);
        }

        public bool Inside(Point2D point2D)
        {
            return Query.Inside(GetPoints(), point2D);
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point2D(jObject.Value<JObject>("Origin"));
            width = jObject.Value<double>("Width");
            height = jObject.Value<double>("Height");
            heightDirection = new Vector2D(jObject.Value<JObject>("HeightDirection"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Width", width);
            jObject.Add("Height", height);
            jObject.Add("HeightDirection", heightDirection.ToJObject());

            return jObject;
        }

        public Point2D GetInternalPoint2D()
        {
            Point2D result = new Point2D(origin);

            result.Move((height / 2) * heightDirection);
            result.Move((width / 2) * WidthDirection);

            return result;
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
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

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }
    }
}