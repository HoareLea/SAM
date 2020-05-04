using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Surface : SAMGeometry, IClosed3D
    {
        private IClosed3D boundary;

        public Surface(IClosed3D boundary)
        {
            this.boundary = boundary.Clone() as IClosed3D;
        }

        public Surface(Surface surface)
        {
            boundary = surface.boundary.Clone() as IClosed3D;
        }

        public Surface(JObject jObject)
            : base(jObject)
        {
        }

        public Face3D ToFace(double tolerance = Core.Tolerance.Distance)
        {
            if (boundary is IClosedPlanar3D)
                return new Face3D(boundary as IClosedPlanar3D);

            List<ICurve3D> curve3Ds = null;
            if (boundary is Polycurve3D)
                curve3Ds = ((Polycurve3D)boundary).Explode();
            else if (boundary is ICurvable3D)
                curve3Ds = ((ICurvable3D)boundary).GetCurves();

            IEnumerable<Segment3D> segment3Ds = curve3Ds.FindAll(x => x is Segment3D).Cast<Segment3D>();
            if (segment3Ds.Count() == curve3Ds.Count)
            {
                List<Point3D> point3Ds = Segment3D.GetPoints(segment3Ds, false);
                Plane plane = Create.Plane(point3Ds, tolerance);
                if (plane != null)
                    return new Face3D(new Polygon3D(point3Ds));
            }

            throw new NotImplementedException();
        }

        public override ISAMGeometry Clone()
        {
            return new Surface(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return boundary.GetBoundingBox(offset);
        }

        public IClosed3D GetExternalEdge()
        {
            return boundary.Clone() as IClosed3D;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Surface((IClosed3D)boundary.GetMoved(vector3D));
        }

        public override bool FromJObject(JObject jObject)
        {
            boundary = Geometry.Create.ISAMGeometry<IClosed3D>(jObject);
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Boundary", boundary.ToJObject());
            return jObject;
        }
    }
}