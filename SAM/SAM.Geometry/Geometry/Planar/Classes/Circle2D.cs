using Newtonsoft.Json.Linq;
using System;

namespace SAM.Geometry.Planar
{
    public class Circle2D : SAMGeometry, IClosed2D, IBoundable2D
    {
        private Point2D center;
        private double radius;

        public Circle2D(Point2D center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public Circle2D(Circle2D circle2D)
        {
            center = new Point2D(circle2D.center);
            radius = circle2D.radius;
        }

        public Circle2D(JObject jObject)
            : base(jObject)
        {
        }

        public Point2D Center
        {
            get
            {
                return new Point2D(center);
            }
            set
            {
                center = value;
            }
        }

        public double Diameter
        {
            get
            {
                return radius * 2;
            }
            set
            {
                radius = value / 2;
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
            }
        }

        public override ISAMGeometry Clone()
        {
            return new Circle2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            center = new Point2D(jObject.Value<JObject>("Center"));
            radius = jObject.Value<double>("Radius");
            return true;
        }

        public double GetArea()
        {
            return System.Math.PI * radius * radius;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(new Point2D(center[0] - radius, center[1] - radius), new Point2D(center[0] + radius, center[1] + radius), offset);
        }

        public Point2D GetCentroid()
        {
            return new Point2D(center);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(center, radius).GetHashCode();
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            return new Point2D(center);
        }

        public Circle2D GetMoved(Vector2D vector2D)
        {
            return new Circle2D((Point2D)center.GetMoved(vector2D), radius);
        }

        public double GetPerimeter()
        {
            return 2 * System.Math.PI * radius;
        }

        /// <summary>
        /// Gets point on Circle for given angle
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Point2D on circle</returns>
        public Point2D GetPoint2D(double angle)
        {
            if (double.IsNaN(angle) || double.IsNaN(radius))
            {
                return null;
            }

            Vector2D vector2D = Query.Vector2D(angle);
            if (vector2D == null)
            {
                return null;
            }

            return center.GetMoved(vector2D * radius);
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return center.Distance(point2D) < radius + tolerance;
        }

        public bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x, tolerance));

            throw new NotImplementedException();
        }

        public void Move(Vector2D vector2D)
        {
            center.Move(vector2D);
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return System.Math.Abs(center.Distance(point2D) - radius) <= tolerance;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Center", center.ToJObject());
            jObject.Add("Radius", radius);

            return jObject;
        }

        public ISAMGeometry2D GetTransformed(ITransform2D transform2D)
        {
            return Query.Transform(this, transform2D);
        }

        public bool Transform(ITransform2D transform2D)
        {
            if(transform2D == null)
            {
                return false;
            }

            Circle2D circle2D = Query.Transform(this, transform2D);
            if(circle2D == null)
            {
                return false;
            }

            center = circle2D.Center;
            radius = circle2D.Radius;
            return true;
        }
    }
}