using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D Project(this Plane plane, Vector3D vector3D)
        {
            if (plane == null || vector3D == null)
                return null;

            Vector3D normal = plane.Normal;
            
            return vector3D - vector3D.DotProduct(normal) * normal;

            //double factor = vector3D.DotProduct(normal) - K;
            //return new Vector3D(vector3D.X - (normal.X * factor), vector3D.Y - (normal.Y * factor), vector3D.Z - (normal.Z * factor));
        }

        public static Point3D Project(this Plane plane, Point3D point3D)
        {
            if (plane == null || point3D == null)
                return null;

            return plane.Closest(point3D);
        }

        public static Point3D Project(this Face3D face3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if(point3D == null || face3D == null)
            {
                return null;
            }


            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Point3D point3D_Project = plane.Project(point3D);
            if(point3D_Project == null)
            {
                return null;
            }

            return face3D.InRange(point3D, tolerance) ? point3D_Project: null;
        }

        public static Point3D Project(this Plane plane, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || point3D == null || vector3D == null)
                return null;

            return plane.Closest(point3D, vector3D, tolerance);
        }

        public static Segment3D Project(this Plane plane, Segment3D segment3D)
        {
            if (plane == null || segment3D == null)
                return null;

            return new Segment3D(plane.Closest(segment3D[0]), plane.Closest(segment3D[1]));
        }

        public static Segment3D Project(this Plane plane, Segment3D segment3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || segment3D == null || vector3D == null)
                return null;

            Point3D point3D_1 = Project(plane, segment3D[0], vector3D, tolerance);
            if (point3D_1 == null)
                return null;

            Point3D point3D_2 = Project(plane, segment3D[1], vector3D, tolerance);
            if (point3D_2 == null)
                return null;

            if (point3D_1.Distance(point3D_2) <= tolerance)
                return null;

            return new Segment3D(point3D_1, point3D_2);
        }

        public static Triangle3D Project(this Plane plane, Triangle3D triangle3D)
        {
            if (plane == null || triangle3D == null)
                return null;

            List<Point3D> point3Ds = triangle3D.GetPoints();
            return new Triangle3D(plane.Closest(point3Ds[0]), plane.Closest(point3Ds[1]), plane.Closest(point3Ds[2]));
        }

        public static Triangle3D Project(this Plane plane, Triangle3D triangle3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || triangle3D == null || vector3D == null)
                return null;

            List<Point3D> point3Ds = triangle3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return null;

            for (int i = 0; i < point3Ds.Count; i++)
            {
                Point3D point3D = Project(plane, point3Ds[i], vector3D, tolerance);
                if (point3D == null)
                    return null;

                point3Ds[i] = point3D;
            }

            if (Planar.Query.Area(point3Ds.ConvertAll(x => Convert(plane, x))) <= tolerance)
                return null;

            return new Triangle3D(point3Ds[0], point3Ds[1], point3Ds[2]);
        }

        public static Polyline3D Project(this Plane plane, Polyline3D polyline3D)
        {
            if (plane == null || polyline3D == null)
                return null;

            return new Polyline3D(polyline3D.Points.ConvertAll(x => plane.Closest(x)));
        }

        public static Polyline3D Project(this Plane plane, Polyline3D polyline3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || polyline3D == null || vector3D == null)
                return null;

            List<Point3D> point3Ds = polyline3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return null;

            for (int i = 0; i < point3Ds.Count; i++)
            {
                Point3D point3D = Project(plane, point3Ds[i], vector3D, tolerance);
                if (point3D == null)
                    return null;

                point3Ds[i] = point3D;
            }

            return new Polyline3D(point3Ds);
        }

        public static Polygon3D Project(this Plane plane, Polygon3D polygon3D)
        {
            if (plane == null || polygon3D == null)
                return null;

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null)
                return null;

            List<Planar.Point2D> point2Ds = point3Ds.ConvertAll(x => Convert(plane, x));

            return new Polygon3D(plane, point2Ds);
        }

        public static Polygon3D Project(this Plane plane, Polygon3D polygon3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || polygon3D == null || vector3D == null)
                return null;

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
                return null;

            for (int i = 0; i < point3Ds.Count; i++)
            {
                Point3D point3D = Project(plane, point3Ds[i], vector3D, tolerance);
                if (point3D == null)
                    return null;

                point3Ds[i] = point3D;
            }

            if (Planar.Query.Area(point3Ds.ConvertAll(x => Convert(plane, x))) <= tolerance)
                return null;

            return new Polygon3D(point3Ds);
        }

        public static ICurve3D Project(this Plane plane, ICurve3D curve3D)
        {
            if (plane == null || curve3D == null)
                return null;

            return Project(plane, curve3D as dynamic);
        }

        public static ICurve3D Project(this Plane plane, ICurve3D curve3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || curve3D == null || vector3D == null)
                return null;

            return Project(plane, curve3D as dynamic, vector3D, tolerance);
        }

        public static Face3D Project(this Plane plane, Face3D face3D)
        {
            if (plane == null || face3D == null)
                return null;

            Planar.IClosed2D externalEdge = Convert(plane, Project(plane, face3D.GetExternalEdge3D()));

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges = internalEdges.ConvertAll(x => Project(plane, x));

            return Face3D.Create(plane, externalEdge, internalEdges?.ConvertAll(x => Convert(plane, x)));
        }

        public static Face3D Project(this Plane plane, Face3D face3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || face3D == null || vector3D == null)
                return null;

            Planar.IClosed2D externalEdge = Convert(plane, Project(plane, face3D.GetExternalEdge3D(), vector3D, tolerance));

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges = internalEdges.ConvertAll(x => Project(plane, x, vector3D, tolerance));

            return Face3D.Create(plane, externalEdge, internalEdges?.ConvertAll(x => Convert(plane, x)));
        }

        public static Line3D Project(this Plane plane, Line3D line3D)
        {
            if (plane == null || line3D == null)
                return null;

            return new Line3D(Project(plane, line3D.Origin), Project(plane, line3D.Direction));
        }

        public static IClosedPlanar3D Project(this Plane plane, IClosedPlanar3D closedPlanar3D)
        {
            if (plane  == null || closedPlanar3D == null)
                return null;

            return Project(plane, closedPlanar3D as dynamic);
        }

        public static IClosedPlanar3D Project(this Plane plane, IClosedPlanar3D closedPlanar3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || closedPlanar3D == null || vector3D == null)
                return null;

            return Project(plane, closedPlanar3D as dynamic, vector3D, tolerance);
        }

        public static Face3D Project(this Plane plane, Shell shell, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if(plane == null || shell == null || vector3D == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            face3Ds = face3Ds.ConvertAll(x => plane.Project(x, vector3D, tolerance));
            face3Ds.RemoveAll(x => x == null || !x.IsValid() || x.GetArea() < tolerance);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            face3Ds = face3Ds.Union(tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            if (face3Ds.Count > 1)
            {
                face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            }

            return face3Ds[0];
        }

        public static Face3D Project(this Plane plane, Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || shell == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            face3Ds = face3Ds.ConvertAll(x => plane.Project(x));
            face3Ds.RemoveAll(x => x == null || !x.IsValid() || x.GetArea() < tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            face3Ds = face3Ds.Union(tolerance);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            if (face3Ds.Count > 1)
            {
                face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            }

            return face3Ds[0];
        }

        public static Point3D Project(this Sphere sphere, Point3D point3D)
        {
            if(sphere == null || point3D == null)
            {
                return null;
            }

            Point3D origin = sphere.Origin;
            if(origin == null || !origin.IsValid())
            {
                return null;
            }

            Vector3D vector3D = new Vector3D(origin, point3D);
            if(!vector3D.IsValid())
            {
                return null;
            }

            vector3D.Normalize();
            vector3D.Scale(sphere.Radious);

            return origin.GetMoved(vector3D) as Point3D;
        }

        public static Point3D Project(this ISegmentable3D segmentable3D, Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            if(segmentable3D == null || point3D == null)
            {
                return null;
            }

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return null;
            }

            double distance = double.MaxValue;
            Point3D result = null;

            foreach (Segment3D segment3D in segment3Ds)
            {
                Point3D point3D_Temp = segment3D.Project(point3D);
                if (point3D_Temp == null)
                {
                    continue;
                }

                double distance_Temp = point3D.Distance(point3D_Temp);
                if(double.IsNaN(distance_Temp))
                {
                    continue;
                }

                if(distance_Temp > distance)
                {
                    continue;
                }

                result = point3D_Temp;
                distance = distance_Temp;
            }

            return result;
        }

    }
}