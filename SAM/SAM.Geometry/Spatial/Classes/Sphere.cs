using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Sphere : SAMGeometry, IBoundable3D
    {
        private Point3D origin;
        private double radious;

        public Sphere(Point3D origin, double radious)
        {
            this.origin = origin;
            this.radious = radious;
        }

        public Sphere(Sphere sphere)
        {
            origin = new Point3D(sphere.origin);
            radious = sphere.radious;
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

        public double Radious
        {
            get
            {
                return radious;
            }
        }

        public bool Inside(Point3D point3D)
        {
            return origin.Distance(point3D) < radious;
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
            return new Sphere((Point3D)origin.GetMoved(vector3D), radious);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point3D(jObject.Value<JObject>("Origin"));
            radious = jObject.Value<double>("Radious");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Radious", radious);

            return jObject;
        }
    }
}