using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face3D : SAMGeometry, IClosedPlanar3D, ISAMGeometry3D
    {
        private Plane plane;
        private Planar.IClosed2D externalEdge;
        private List<Planar.IClosed2D> internalEdges;

        public Face3D(Planar.Polygon2D polygon2D)
        {
            plane = new Plane(Point3D.Zero, Vector3D.BaseZ);
            externalEdge = polygon2D;
        }

        public Face3D(Plane plane, Planar.IClosed2D externalEdge)
        {
            this.plane = new Plane(plane);
            this.externalEdge = (Planar.IClosed2D)externalEdge.Clone();
        }

        public Face3D(IClosedPlanar3D closedPlanar3D)
        {
            plane = closedPlanar3D.GetPlane();
            externalEdge = plane.Convert(closedPlanar3D);

        }

        public Face3D(Face3D face3D)
        {
            this.plane = new Plane(face3D.plane);
            this.externalEdge = (Planar.IClosed2D)face3D.externalEdge.Clone();
        }

        public Face3D(JObject jObject)
            : base(jObject)
        {

        }

        public Plane GetPlane()
        {
            return new Plane(plane);
        }

        public override ISAMGeometry Clone()
        {
            return new Face3D(this);
        }

        public IClosedPlanar3D ToClosedPlanar3D()
        {

            return plane.Convert(externalEdge);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(externalEdge).GetBoundingBox(offset);
        }

        public Planar.IClosed2D ExternalEdge
        {
            get
            {
                return externalEdge.Clone() as Planar.IClosed2D;
            }
        }

        public List<Planar.IClosed2D> InternalEdges
        {
            get
            {
                if (internalEdges == null)
                    return null;

                return internalEdges.ConvertAll(x => x.Clone() as Planar.IClosed2D);
            }
        }

        public List<Planar.IClosed2D> Edges
        {
            get
            {
                List<Planar.IClosed2D> result = new List<Planar.IClosed2D>() { externalEdge };
                if (internalEdges != null && internalEdges.Count > 0)
                    result.AddRange(internalEdges);
                return result;
            }
        }

        public IClosed3D GetExternalEdges()
        {
            return plane.Convert(externalEdge).GetExternalEdges();
        }

        public List<IClosed3D> GetInternalEdges()
        {
            if (internalEdges == null)
                return null;

            List<IClosed3D> result = new List<IClosed3D>();
            foreach (Planar.IClosed2D closed2D in internalEdges)
                result.Add(plane.Convert(closed2D).GetExternalEdges());

            return result;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Face3D((Plane)plane.GetMoved(vector3D), (Planar.IClosed2D)externalEdge.Clone());
        }

        public double GetArea()
        {
            double area = externalEdge.GetArea();
            if (internalEdges != null && internalEdges.Count > 0)
                foreach (Planar.IClosed2D closed2D in internalEdges)
                    area -= closed2D.GetArea();

            return area;
        }

        public bool Inside(Face3D face3D, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.Coplanar(face3D.plane, tolerance))
                return false;

            IClosed3D closed3D = face3D.plane.Convert(face3D.externalEdge);

            return externalEdge.Inside(plane.Convert(closed3D));
        }

        public bool Inside(Point3D point3D, double tolerance = Tolerance.MicroDistance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Planar.Point2D point2D = plane.Convert(point3D);
            return externalEdge.Inside(point2D);
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
            externalEdge = Planar.Create.IClosed2D(jObject.Value<JObject>("ExternalEdge"));

            if (jObject.ContainsKey("InternalEdges"))
                internalEdges = Core.Create.IJSAMObjects<Planar.IClosed2D>(jObject.Value<JArray>("InternalEdges"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());
            jObject.Add("ExternalEdge", externalEdge.ToJObject());
            if (internalEdges != null)
                jObject.Add("InternalEdges", Core.Create.JArray(internalEdges));

            return jObject;
        }

        
        public static Face3D GetFace(Plane plane, IEnumerable<Planar.IClosed2D> edges, out List<Planar.IClosed2D> edges_Excluded)
        {
            edges_Excluded = null;

            if (edges == null || edges.Count() == 0 || plane == null)
                return null;

            Planar.IClosed2D closed2D_Max = null;
            double area_Max = double.MinValue;
            foreach (Planar.IClosed2D closed2D in edges)
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

            Face3D result = new Face3D(plane, closed2D_Max);
            edges_Excluded = new List<Planar.IClosed2D>();
            foreach (Planar.IClosed2D closed2D in edges)
            {
                if (result == closed2D_Max)
                    continue;

                if (!closed2D_Max.Inside(closed2D))
                {
                    edges_Excluded.Add(closed2D);
                    continue;
                }

                if (result.internalEdges == null)
                    result.internalEdges = new List<Planar.IClosed2D>();

                result.internalEdges.Add((Planar.IClosed2D)closed2D.Clone());
            }

            return result;
        }
        
        public static Face3D GetFace(Plane plane, IEnumerable<Planar.IClosed2D> edges)
        {
            List<Planar.IClosed2D> edges_Excluded = null;
            return GetFace(plane, edges, out edges_Excluded);
        }

        public static List<Face3D> GetFaces(Plane plane, IEnumerable<Planar.IClosed2D> edges)
        {
            if (plane == null || edges == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            if (edges.Count() == 0)
                return result;

            List<Planar.IClosed2D> edges_Current = new List<Planar.IClosed2D>(edges);
            while (edges_Current.Count > 0)
            {
                List<Planar.IClosed2D> edges_Excluded = null;
                Face3D face = GetFace(plane, edges_Current, out edges_Excluded);
                if (face == null)
                    break;

                result.Add(face);

                edges_Current = edges_Excluded;
            }

            return result;
        }
    }
}
