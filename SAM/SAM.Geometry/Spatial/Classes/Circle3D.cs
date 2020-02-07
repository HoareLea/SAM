﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Circle3D: SAMGeometry, IClosedPlanar3D, IBoundable3D
    {
        private Plane plane;
        private double radious;

        public Circle3D(Plane plane, double radious)
        {
            this.plane = new Plane(plane);
            this.radious = radious;
        }

        public Circle3D(Circle3D circle3D)
        {
            this.plane = new Plane(circle3D.plane);
            this.radious = circle3D.radious;
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

        public double GetArea()
        {
            return Math.PI * radious * radious;
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

        public Point3D GetCentroid()
        {
            return new Point3D(plane.Origin);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Circle3D((Plane)plane.GetMoved(vector3D), radious);
        }

        public void Move(Vector3D vector3D)
        {
            plane = (Plane)plane.GetMoved(vector3D);
        }

        public double GetPerimeter()
        {
            return 2 * Math.PI * radious;
        }

        public Plane GetPlane()
        {
            return new Plane(plane);
        }

        public override ISAMGeometry Clone()
        {
            return new Circle3D((Plane)plane.Clone(), radious);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new NotImplementedException();
        }

        public IClosed3D GetBoundary()
        {
            return new Circle3D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            plane = new Plane(jObject.Value<JObject>("Plane"));
            radious = jObject.Value<double>("Radious");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("Radious", radious);

            return jObject;
        }
    }
}
