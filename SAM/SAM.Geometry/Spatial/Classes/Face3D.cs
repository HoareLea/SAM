using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Spatial
{
    public class Face3D : Face, IClosedPlanar3D, ISAMGeometry3D
    {
        private Plane plane;

        public Face3D(Planar.IClosed2D externalEdge)
            : base(externalEdge)
        {
            plane = new Plane(Point3D.Zero, Vector3D.BaseZ);
        }

        public Face3D(Plane plane, Planar.IClosed2D externalEdge)
            : base(externalEdge)
        {
            this.plane = new Plane(plane);
        }

        public Face3D(Plane plane, Planar.Face2D face2D)
            : base(face2D)
        {
            this.plane = new Plane(plane);
        }

        public Face3D(IClosedPlanar3D closedPlanar3D)
            : base(closedPlanar3D.GetPlane().Convert(closedPlanar3D))
        {
            plane = closedPlanar3D.GetPlane();
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

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(externalEdge).GetBoundingBox(offset);
        }

        IClosed3D IClosed3D.GetExternalEdge()
        {
            return this.GetExternalEdge();
        }

        public IClosedPlanar3D GetExternalEdge()
        {
            return plane.Convert(externalEdge);//.GetExternalEdge();
        }

        public List<IClosedPlanar3D> GetInternalEdges()
        {
            if (internalEdges == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (Planar.IClosed2D closed2D in internalEdges)
                result.Add(plane.Convert(closed2D));//.GetExternalEdge());

            return result;
        }

        public List<IClosedPlanar3D> GetEdges()
        {
            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>() { GetExternalEdge() };
            List<IClosedPlanar3D> closedPlanar3Ds = GetInternalEdges();
            if (closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
                result.AddRange(closedPlanar3Ds);
            return result;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Face3D((Plane)plane.GetMoved(vector3D), (Planar.IClosed2D)externalEdge.Clone());
        }

        public bool RemoveInternalEdge(int index)
        {
            if (index < 0 || internalEdges == null || internalEdges.Count == 0)
                return false;

            if (index >= internalEdges.Count)
                return false;

            internalEdges.RemoveAt(index);
            return true;
        }

        public bool Inside(Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.Coplanar(face3D.plane, tolerance))
                return false;

            IClosed3D closed3D = face3D.plane.Convert(face3D.externalEdge);
            if (closed3D == null)
                return false;

            Planar.IClosed2D closed2D = plane.Convert(closed3D);

            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.Inside(closed2D);

            return externalEdge.Inside(closed2D) && internalEdges.TrueForAll(x => !x.Inside(closed2D));
        }

        public bool Inside(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Planar.Point2D point2D = plane.Convert(point3D);
            
            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.Inside(point2D);

            return externalEdge.Inside(point2D) && internalEdges.TrueForAll(x => !x.Inside(point2D));
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Planar.Point2D point2D = plane.Convert(point3D);

            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.On(point2D);

            return externalEdge.On(point2D) || internalEdges.Any(x => x.On(point2D, tolerance));
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
            if (!base.FromJObject(jObject))
                return false;

            plane = new Plane(jObject.Value<JObject>("Plane"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Plane", plane.ToJObject());

            return jObject;
        }

        public Point3D GetInternaPoint3D()
        {
            return plane.Convert(GetInternalPoint2D());
        }


        public static Face3D Create(IEnumerable<IClosedPlanar3D> edges)
        {
            if (edges == null || edges.Count() == 0)
                return null;

            IClosedPlanar3D externalEdge = null;
            double area_Max = double.MinValue;
            foreach (IClosedPlanar3D closedPlanar3D in edges)
            {
                if (closedPlanar3D == null)
                    continue;
                
                double area = closedPlanar3D.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    externalEdge = closedPlanar3D;
                }

            }

            if (externalEdge == null)
                return null;

            Plane plane = externalEdge.GetPlane();

            List<IClosedPlanar3D> internalEdges = edges.ToList();
            internalEdges.RemoveAll(x => x == null);
            internalEdges.Remove(externalEdge);

            return Create(plane, plane?.Convert(externalEdge), internalEdges.ConvertAll(x => plane.Convert(x)));
        }
        
        public static Face3D Create(Plane plane, Planar.IClosed2D externalEdge, IEnumerable<Planar.IClosed2D> internalEdges)
        {
            if (plane == null || externalEdge == null)
                return null;

            Planar.Face2D face2D = Planar.Face2D.Create(externalEdge, internalEdges);
            if (face2D == null)
                return null;

            return new Face3D(plane, face2D);
        }

        public static Face3D Create(Plane plane, IEnumerable<Planar.IClosed2D> edges, out List<Planar.IClosed2D> edges_Excluded)
        {
            edges_Excluded = null;

            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            Planar.Face2D face2D = Planar.Face2D.Create(edges, out edges_Excluded);
            return new Face3D(plane, face2D);
        }
        
        public static Face3D Create(Plane plane, IEnumerable<Planar.IClosed2D> edges)
        {
            if (plane == null || edges == null || edges.Count() == 0)
                return null;
            
            return new Face3D(plane, Planar.Face2D.Create(edges));
        }
    }
}
