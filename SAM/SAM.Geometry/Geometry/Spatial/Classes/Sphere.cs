using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Sphere : SAMGeometry, IBoundable3D
    {
        private Point3D origin;
        private double radius;

        public Sphere(Point3D origin, double radius)
        {
            this.origin = origin;
            this.radius = radius;
        }

        public Sphere(Sphere sphere)
        {
            origin = new Point3D(sphere.origin);
            radius = sphere.radius;
        }

        public Sphere(JObject jObject)
            : base(jObject)
        {
        }

        public override ISAMGeometry Clone()
        {
            return new Sphere(this);
        }

        public Point3D Origin
        {
            get
            {
                return origin;
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
        }

        public bool Inside(Point3D point3D)
        {
            return origin.Distance(point3D) < radius;
        }

        public bool Inside(Segment3D segment3D)
        {
            return Inside(segment3D.GetPoints());
        }

        public bool Inside(Polygon3D polygon3D)
        {
            return Inside(polygon3D.GetPoints());
        }

        public bool Inside(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return false;

            foreach (Point3D point3D in point3Ds)
                if (!Inside(point3D))
                    return false;

            return true;
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new NotImplementedException();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Sphere((Point3D)origin.GetMoved(vector3D), radius);
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point3D(jObject.Value<JObject>("Origin"));
            radius = jObject.Value<double>("Radius");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Radius", radius);

            return jObject;
        }
    }
}