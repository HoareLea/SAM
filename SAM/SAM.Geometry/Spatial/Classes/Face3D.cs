using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Face3D : Face, IClosedPlanar3D, ISAMGeometry3D
    {
        private Plane plane;

        public Face3D(IClosed2D externalEdge)
            : base(externalEdge)
        {
            plane = new Plane(Point3D.Zero, Vector3D.WorldZ);
        }

        public Face3D(Plane plane, IClosed2D externalEdge)
            : base(externalEdge)
        {
            this.plane = new Plane(plane);
        }

        public Face3D(Plane plane, Face2D face2D)
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
            if(plane == null)
            {
                return null;
            }
            
            return new Plane(plane);
        }

        public override ISAMGeometry Clone()
        {
            return new Face3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(externalEdge2D).GetBoundingBox(offset);
        }

        public IClosedPlanar3D GetExternalEdge3D()
        {
            return plane.Convert(externalEdge2D);//.GetExternalEdge();
        }

        public List<IClosedPlanar3D> GetInternalEdge3Ds()
        {
            if (internalEdge2Ds == null)
                return null;

            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>();
            foreach (IClosed2D closed2D in internalEdge2Ds)
                result.Add(plane.Convert(closed2D));//.GetExternalEdge());

            return result;
        }

        public List<IClosedPlanar3D> GetEdge3Ds()
        {
            List<IClosedPlanar3D> result = new List<IClosedPlanar3D>() { GetExternalEdge3D() };
            List<IClosedPlanar3D> closedPlanar3Ds = GetInternalEdge3Ds();
            if (closedPlanar3Ds != null && closedPlanar3Ds.Count > 0)
                result.AddRange(closedPlanar3Ds);
            return result;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            if(vector3D == null || plane == null || externalEdge2D == null)
            {
                return null;
            }
            
            Face3D face3D = new Face3D((Plane)plane.GetMoved(vector3D), (IClosed2D)externalEdge2D.Clone());
            face3D.internalEdge2Ds = internalEdge2Ds?.ConvertAll(x => (IClosed2D)x.Clone());
            return face3D;

            //return new Face3D((Plane)plane.GetMoved(vector3D), (IClosed2D)externalEdge2D.Clone());
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        public bool RemoveInternalEdge(int index)
        {
            if (index < 0 || internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return false;

            if (index >= internalEdge2Ds.Count)
                return false;

            internalEdge2Ds.RemoveAt(index);
            return true;
        }

        public bool Inside(Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.Coplanar(face3D.plane, tolerance))
                return false;

            IClosed3D closed3D = face3D.plane.Convert(face3D.externalEdge2D);
            if (closed3D == null)
                return false;

            IClosed2D closed2D = plane.Convert(closed3D);

            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.Inside(closed2D, tolerance);

            return externalEdge2D.Inside(closed2D, tolerance) && internalEdge2Ds.TrueForAll(x => !x.Inside(closed2D, tolerance));
        }

        public bool Inside(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Point2D point2D = plane.Convert(point3D);

            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.Inside(point2D, tolerance);

            return externalEdge2D.Inside(point2D, tolerance) && internalEdge2Ds.TrueForAll(x => !x.Inside(point2D, tolerance));
        }

        public bool OnEdge(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Point2D point2D = plane.Convert(point3D);

            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.On(point2D, tolerance);

            return externalEdge2D.On(point2D, tolerance) || internalEdge2Ds.Any(x => x.On(point2D, tolerance));
        }

        public bool InRange(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            return externalEdge2D.InRange(plane.Convert(point3D), tolerance);
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (!plane.On(point3D, tolerance))
                return false;

            Point2D point2D = plane.Convert(point3D);

            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return externalEdge2D.InRange(point2D);

            return externalEdge2D.InRange(point2D) && internalEdge2Ds.TrueForAll(x => !x.Inside(point2D, tolerance));
        }

        public void FlipNormal(bool flipX = true)
        {
            IClosedPlanar3D externalEdge = GetExternalEdge3D();
            if (externalEdge != null)
                externalEdge.Reverse();

            List<IClosedPlanar3D> internalEdges = GetInternalEdge3Ds();
            if (internalEdges != null)
                internalEdges.ForEach(x => x.Reverse());

            plane.FlipZ(flipX);

            if (externalEdge != null)
                externalEdge2D = plane.Convert(externalEdge);

            if (internalEdges != null)
                internalEdge2Ds = internalEdges.ConvertAll(x => plane.Convert(x));
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

        public double Distance(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return double.NaN;

            Point3D point3D_Project = plane.Project(point3D);

            Point2D point2D = plane.Convert(point3D_Project);
            if (point2D == null)
                return double.NaN;

            double a = point3D_Project.Distance(point3D);
            double b = Distance(point2D, tolerance);

            return System.Math.Sqrt((a * a) + (b * b));
        }

        public double DistanceToEdges(Point3D point3D)
        {
            if (point3D == null)
                return double.NaN;

            Point3D point3D_Project = plane.Project(point3D);

            Point2D point2D = plane.Convert(point3D_Project);
            if (point2D == null)
                return double.NaN;

            double a = point3D_Project.Distance(point3D);
            double b = DistanceToEdge2Ds(point2D);

            return System.Math.Sqrt((a * a) + (b * b));
        }

        public void Normalize(double tolerance = Core.Tolerance.Distance)
        {
            Vector3D normal = plane?.Normal;
            if (normal == null)
                return;

            bool clockwise = Query.Clockwise(GetExternalEdge3D(), normal, Core.Tolerance.Angle, tolerance);
            if (!clockwise)
                (externalEdge2D as Polygon2D)?.Reverse();

            List<IClosedPlanar3D> internalEdge3Ds = GetInternalEdge3Ds();
            if (internalEdge3Ds != null)
            {
                for (int i = 0; i < internalEdge3Ds.Count; i++)
                {
                    clockwise = Query.Clockwise(internalEdge3Ds[i], normal, Core.Tolerance.Angle, tolerance);
                    if (clockwise)
                        (internalEdge2Ds[i] as Polygon2D)?.Reverse();
                }
            }
        }

        public Point3D GetInternalPoint3D(double tolerance = Core.Tolerance.Distance)
        {
            return Query.InternalPoint3D(this, tolerance);
        }

        public Point3D GetCentroid()
        {
            return plane?.Convert(externalEdge2D.GetCentroid());
        }

        public static implicit operator Face3D(Polygon3D polygon3D)
        {
            if (polygon3D == null)
                return null;

            return new Face3D(polygon3D);
        }

        public static implicit operator Face3D(Circle3D circle3D)
        {
            if (circle3D == null)
                return null;

            return new Face3D(circle3D);
        }

        public static implicit operator Face3D(Ellipse3D ellipse3D)
        {
            if (ellipse3D == null)
                return null;

            return new Face3D(ellipse3D);
        }

        public static implicit operator Face3D(Rectangle3D rectangle3D)
        {
            if (rectangle3D == null)
                return null;

            return new Face3D(rectangle3D);
        }

        public static implicit operator Face3D(Triangle3D triangle3D)
        {
            if (triangle3D == null)
                return null;

            return new Face3D(triangle3D);
        }


        public static Face3D Create(IEnumerable<IClosedPlanar3D> edges, bool orientInternalEdges = true)
        {
            if (edges == null || edges.Count() == 0)
                return null;

            IClosedPlanar3D externalEdge_3D = null;
            double area_Max = double.MinValue;
            foreach (IClosedPlanar3D closedPlanar3D in edges)
            {
                if (closedPlanar3D == null)
                    continue;

                double area = closedPlanar3D.GetArea();
                if (area > area_Max)
                {
                    area_Max = area;
                    externalEdge_3D = closedPlanar3D;
                }
            }

            if (externalEdge_3D == null)
                return null;

            Plane plane = externalEdge_3D.GetPlane();

            IClosed2D externalEdge_2D = plane?.Convert(externalEdge_3D);

            List<IClosedPlanar3D> internalEdges_3D = edges.ToList();
            internalEdges_3D.RemoveAll(x => x == null);
            internalEdges_3D.Remove(externalEdge_3D);

            List<IClosed2D> internalEdges_2D = internalEdges_3D.ConvertAll(x => plane.Convert(x));

            if (orientInternalEdges)
            {
                for (int i = 0; i < internalEdges_2D.Count; i++)
                {
                    IClosed2D internalEdge_2D = internalEdges_2D[i];

                    if (internalEdge_2D is Polygon2D && externalEdge_2D is Polygon2D)
                    {
                        Polygon2D polygon2D = (Polygon2D)internalEdge_2D;
                        polygon2D.SetOrientation(Geometry.Query.Opposite(((Polygon2D)externalEdge_2D).GetOrientation()));
                        internalEdges_2D[i] = polygon2D;
                    }
                }
            }

            return Create(plane, externalEdge_2D, internalEdges_2D);
        }

        public static Face3D Create(Plane plane, IClosed2D externalEdge, IEnumerable<IClosed2D> internalEdges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            if (plane == null || externalEdge == null)
                return null;

            Face2D face2D = Face2D.Create(externalEdge, internalEdges, edgeOrientationMethod);
            if (face2D == null)
                return null;

            return new Face3D(plane, face2D);
        }

        public static Face3D Create(Plane plane, IEnumerable<IClosed2D> edges, out List<IClosed2D> edges_Excluded, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            edges_Excluded = null;

            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            Face2D face2D = Face2D.Create(edges, out edges_Excluded, edgeOrientationMethod);
            return new Face3D(plane, face2D);
        }

        public static Face3D Create(Plane plane, IEnumerable<IClosed2D> edges, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            if (plane == null || edges == null || edges.Count() == 0)
                return null;

            return new Face3D(plane, Face2D.Create(edges, edgeOrientationMethod));
        }
    }
}