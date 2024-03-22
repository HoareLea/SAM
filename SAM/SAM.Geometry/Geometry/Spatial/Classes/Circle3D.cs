using Newtonsoft.Json.Linq;
using System;

namespace SAM.Geometry.Spatial
{
    public class Circle3D : SAMGeometry, IClosedPlanar3D, IBoundable3D
    {
        private Plane plane;
        private double radius;

        public Circle3D(Plane plane, double radius)
        {
            this.plane = new Plane(plane);
            this.radius = radius;
        }

        public Circle3D(Circle3D circle3D)
        {
            plane = new Plane(circle3D.plane);
            radius = circle3D.radius;
        }

        public Circle3D(JObject jObject)
            : base(jObject)
        {
        }

        public Point3D Center
        {
            get
            {
                return new Point3D(plane.Origin);
            }
            set
            {
                plane.Origin = value;
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

        public double GetArea()
        {
            return System.Math.PI * radius * radius;
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

        public Point3D GetCentroid()
        {
            return new Point3D(plane.Origin);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Circle3D((Plane)plane.GetMoved(vector3D), radius);
        }

        public void Move(Vector3D vector3D)
        {
            plane = (Plane)plane.GetMoved(vector3D);
        }

        public double GetPerimeter()
        {
            return 2 * System.Math.PI * radius;
        }

        public Plane GetPlane()
        {
            if(plane == null)
            {
                return null;
            }
            
            return new Plane(plane);
        }

        /// <summary>
        /// Gets Point3D on circle for given angle
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Point on Circle</returns>
        public Point3D GetPoint3D(double angle)
        {
            if(plane == null || double.IsNaN(radius))
            {
                return null;
            }

            Planar.Circle2D circle2D = new Planar.Circle2D(plane.Convert(plane.Origin), radius);
            Planar.Point2D point2D = circle2D.GetPoint2D(angle);
            if(point2D == null)
            {
                return null;
            }

            return plane.Convert(point2D);
        }

        public override ISAMGeometry Clone()
        {
            return new Circle3D((Plane)plane.Clone(), radius);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new NotImplementedException();
        }

        public IClosed3D GetExternalEdge()
        {
            return new Circle3D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            plane = new Plane(jObject.Value<JObject>("Plane"));
            radius = jObject.Value<double>("Radius");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("Radius", radius);

            return jObject;
        }

        public void Reverse()
        {
            plane.FlipZ(true);
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        public override bool Equals(object obj)
        {
            Circle3D circle3D = obj as Circle3D;
            if(circle3D == null)
            {
                return false;
            }

            return circle3D.plane == plane && circle3D.radius == radius;
        }

        public override int GetHashCode()
        {
            return new Tuple<Plane, double>(plane, radius).GetHashCode();
        }

        public static bool operator ==(Circle3D circle3D_1, Circle3D circle3D_2)
        {
            if (ReferenceEquals(circle3D_1, null) && ReferenceEquals(circle3D_2, null))
                return true;

            if (ReferenceEquals(circle3D_1, null))
                return false;

            if (ReferenceEquals(circle3D_2, null))
                return false;

            return circle3D_1.plane == circle3D_2.plane && circle3D_1.radius == circle3D_2.radius;
        }

        public static bool operator !=(Circle3D circle3D_1, Circle3D circle3D_2)
        {
            if (ReferenceEquals(circle3D_1, null) && ReferenceEquals(circle3D_2, null))
                return false;

            if (ReferenceEquals(circle3D_1, null))
                return true;

            if (ReferenceEquals(circle3D_2, null))
                return true;

            return circle3D_1.plane != circle3D_2.plane || circle3D_1.radius != circle3D_2.radius;
        }
    }
}