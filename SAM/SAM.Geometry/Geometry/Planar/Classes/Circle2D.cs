using Newtonsoft.Json.Linq;
using System;

namespace SAM.Geometry.Planar
{
    public class Circle2D : SAMGeometry, IClosed2D, IBoundable2D
    {
        private Point2D center;
        private double radious;

        public Circle2D(Point2D center, double radious)
        {
            this.center = center;
            this.radious = radious;
        }

        public Circle2D(Circle2D circle2D)
        {
            center = new Point2D(circle2D.center);
            radious = circle2D.radious;
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
                return radious * 2;
            }
            set
            {
                radious = value / 2;
            }
        }

        public double Radious
        {
            get
            {
                return radious;
            }
            set
            {
                radious = value;
            }
        }

        public override ISAMGeometry Clone()
        {
            return new Circle2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            center = new Point2D(jObject.Value<JObject>("Center"));
            radious = jObject.Value<double>("Radious");
            return true;
        }

        public double GetArea()
        {
            return System.Math.PI * radious * radious;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(new Point2D(center[0] - radious, center[1] - radious), new Point2D(center[0] + radious, center[1] + radious), offset);
        }

        public Point2D GetCentroid()
        {
            return new Point2D(center);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(center, radious).GetHashCode();
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            return new Point2D(center);
        }

        public Circle2D GetMoved(Vector2D vector2D)
        {
            return new Circle2D((Point2D)center.GetMoved(vector2D), radious);
        }

        public double GetPerimeter()
        {
            return 2 * System.Math.PI * radious;
        }

        /// <summary>
        /// Gets point on Circle for given angle
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Point2D on circle</returns>
        public Point2D GetPoint2D(double angle)
        {
            if (double.IsNaN(angle) || double.IsNaN(radious))
            {
                return null;
            }

            Vector2D vector2D = Query.Vector2D(angle);
            if (vector2D == null)
            {
                return null;
            }

            return center.GetMoved(vector2D * radious);
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return center.Distance(point2D) < radious + tolerance;
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
            return System.Math.Abs(center.Distance(point2D) - radious) <= tolerance;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Center", center.ToJObject());
            jObject.Add("Radious", radious);

            return jObject;
        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            return Query.Transform(this, transform2D);
        }

        public bool Transform(Transform2D transform2D)
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
            radious = circle2D.Radious;
            return true;
        }
    }
}