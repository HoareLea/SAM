using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face : SAMGeometry, IClosedPlanar3D, ISAMGeometry3D
    {
        private Plane plane;
        private Planar.IClosed2D externalBoundary;
        private List<Planar.IClosed2D> internalBoundaries;

        public Face(Planar.Polygon2D polygon2D)
        {
            plane = new Plane(Point3D.Zero, Vector3D.BaseZ);
            externalBoundary = polygon2D;
        }

        public Face(Plane plane, Planar.IClosed2D externalboundary)
        {
            this.plane = new Plane(plane);
            this.externalBoundary = (Planar.IClosed2D)externalboundary.Clone();
        }

        public Face(IClosedPlanar3D closedPlanar3D)
        {
            plane = closedPlanar3D.GetPlane();
            externalBoundary = plane.Convert(closedPlanar3D);

        }

        public Face(Face face)
        {
            this.plane = new Plane(face.plane);
            this.externalBoundary = (Planar.IClosed2D)face.externalBoundary.Clone();
        }

        public Face(JObject jObject)
            : base(jObject)
        {

        }

        public Plane GetPlane()
        {
            return new Plane(plane);
        }

        public override ISAMGeometry Clone()
        {
            return new Face(this);
        }

        public IClosedPlanar3D ToClosedPlanar3D()
        {

            return plane.Convert(externalBoundary);
        }

        public Surface ToSurface()
        {
            return new Surface(plane.Convert(externalBoundary));
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(externalBoundary).GetBoundingBox(offset);
        }

        public Planar.IClosed2D ExternalBoundary
        {
            get
            {
                return externalBoundary.Clone() as Planar.IClosed2D;
            }
        }

        public List<Planar.IClosed2D> InternalBoundaries
        {
            get
            {
                if (internalBoundaries == null)
                    return null;

                return internalBoundaries.ConvertAll(x => x.Clone() as Planar.IClosed2D);
            }
        }

        public List<Planar.IClosed2D> Boundaries
        {
            get
            {
                List<Planar.IClosed2D> result = new List<Planar.IClosed2D>() { externalBoundary };
                if (internalBoundaries != null && internalBoundaries.Count > 0)
                    result.AddRange(internalBoundaries);
                return result;
            }
        }

        public IClosed3D GetExternalBoundary()
        {
            return plane.Convert(externalBoundary).GetExternalBoundary();
        }

        public List<IClosed3D> GetInternalBoundaries()
        {
            if (internalBoundaries == null)
                return null;

            List<IClosed3D> result = new List<IClosed3D>();
            foreach (Planar.IClosed2D closed2D in internalBoundaries)
                result.Add(plane.Convert(closed2D).GetExternalBoundary());

            return result;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Face((Plane)plane.GetMoved(vector3D), (Planar.IClosed2D)externalBoundary.Clone());
        }

        public double GetArea()
        {
            return externalBoundary.GetArea();
        }

        public bool Inside(Face face, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.Coplanar(face.plane, tolerance))
                return false;

            IClosed3D closed3D = face.plane.Convert(face.externalBoundary);

            return externalBoundary.Inside(plane.Convert(closed3D));
        }

        public bool Inside(Point3D point3D, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Planar.Point2D point2D = plane.Convert(point3D);
            return externalBoundary.Inside(point2D);
        }

        public IClosedPlanar3D Project(IClosed3D closed3D)
        {
            if(closed3D is ISegmentable3D)
            {
                List<Point3D> point3Ds = ((ISegmentable3D)closed3D).GetPoints().ConvertAll(x => plane.Project(x));
                return new Polygon3D(point3Ds);
            }

            return null;
        }

        public override bool FromJObject(JObject jObject)
        {
            plane = new Plane(jObject.Value<JObject>("Plane"));
            externalBoundary = Planar.Create.IClosed2D(jObject.Value<JObject>("ExternalBoundary"));

            if (jObject.ContainsKey("InternalBoundaries"))
                internalBoundaries = Core.Create.IJSAMObjects<Planar.IClosed2D>(jObject.Value<JArray>("InternalBoundaries"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("ExternalBoundary", externalBoundary.ToJObject());
            if (internalBoundaries != null)
                jObject.Add("InternalBoundaries", Core.Create.JArray(internalBoundaries));

            return jObject;
        }

        
        public static Face GetFace(Plane plane, IEnumerable<Planar.IClosed2D> boundaries, out List<Planar.IClosed2D> boundaries_Excluded)
        {
            boundaries_Excluded = null;

            if (boundaries == null || boundaries.Count() == 0 || plane == null)
                return null;

            Planar.IClosed2D closed2D_Max = null;
            double area_Max = double.MinValue;
            foreach (Planar.IClosed2D closed2D in boundaries)
            {
                double area = closed2D.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    closed2D_Max = closed2D;
                }

            }

            if (closed2D_Max == null)
                return null;

            Face result = new Face(plane, closed2D_Max);
            boundaries_Excluded = new List<Planar.IClosed2D>();
            foreach (Planar.IClosed2D closed2D in boundaries)
            {
                if (result == closed2D_Max)
                    continue;

                if (!closed2D_Max.Inside(closed2D))
                {
                    boundaries_Excluded.Add(closed2D);
                    continue;
                }

                if (result.internalBoundaries == null)
                    result.internalBoundaries = new List<Planar.IClosed2D>();

                result.internalBoundaries.Add((Planar.IClosed2D)closed2D.Clone());
            }

            return result;
        }
        
        public static Face GetFace(Plane plane, IEnumerable<Planar.IClosed2D> boundaries)
        {
            List<Planar.IClosed2D> boundaries_Excluded = null;
            return GetFace(plane, boundaries, out boundaries_Excluded);
        }

        public static List<Face> GetFaces(Plane plane, IEnumerable<Planar.IClosed2D> boundaries)
        {
            if (plane == null || boundaries == null)
                return null;

            List<Face> result = new List<Face>();
            if (boundaries.Count() == 0)
                return result;

            List<Planar.IClosed2D> boundaries_Current = new List<Planar.IClosed2D>(boundaries);
            while (boundaries_Current.Count > 0)
            {
                List<Planar.IClosed2D> boundaries_Excluded = null;
                Face face = GetFace(plane, boundaries_Current, out boundaries_Excluded);
                if (face == null)
                    break;

                result.Add(face);

                boundaries_Current = boundaries_Excluded;
            }

            return result;
        }
    }
}
