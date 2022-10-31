using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Triangle3D> Triangulate(this Face3D face3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            Planar.Face2D face2D = plane.Convert(face3D);
            if (face2D == null)
                return null;

            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(face2D, tolerance);
            if (triangle2Ds == null)
                return null;

            return triangle2Ds.ConvertAll(x => plane.Convert(x));

        }

        public static List<Triangle3D> Triangulate(this Polygon3D polygon3D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = polygon3D?.GetPlane();
            if (plane == null)
                return null;

            Planar.Polygon2D polygon2D = plane.Convert(polygon3D);
            if (polygon2D == null)
                return null;

            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(polygon2D, tolerance);
            if (triangle2Ds == null)
                return null;

            return triangle2Ds.ConvertAll(x => plane.Convert(x));
        }

        public static List<Triangle3D> Triangulate(this Shell shell, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if (face3Ds == null)
                return null;

            List<List<Triangle3D>> triangle3Ds = Enumerable.Repeat<List<Triangle3D>>(null, face3Ds.Count).ToList();
            System.Threading.Tasks.Parallel.For(0, face3Ds.Count, (int i) =>
            {
                triangle3Ds[i] = face3Ds[i]?.Triangulate(tolerance);
            });

            List<Triangle3D> result = new List<Triangle3D>();
            foreach (List<Triangle3D> triangle3Ds_Temp in triangle3Ds)
            {
                if (triangle3Ds_Temp != null && triangle3Ds_Temp.Count > 0)
                {
                    result.AddRange(triangle3Ds_Temp);
                }
            }

            return result;
        }

        public static List<Triangle3D> Triangulate(this Extrusion extrusion, double tolerance = Core.Tolerance.MicroDistance)
        {
            Shell shell = Create.Shell(extrusion, tolerance);
            if (shell == null)
            {
                return null;
            }

            return Triangulate(shell, tolerance);
        }

        public static List<Triangle3D> Triangulate(this Face3D face3D, IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return null;
            }

            Planar.Face2D face2D = plane.Convert(face3D);
            if (face2D == null)
            {
                return null;
            }

            List<Tuple<Planar.Point2D, Vector3D>> tuples = null;
            if (point3Ds != null)
            {
                tuples = new List<Tuple<Planar.Point2D, Vector3D>>();
                foreach (Point3D point3D in point3Ds)
                {
                    if (point3D == null)
                    {
                        continue;
                    }

                    Point3D point3D_Project = plane.Project(point3D);
                    if (point3D_Project == null)
                    {
                        continue;
                    }

                    if (!boundingBox3D.InRange(point3D_Project, tolerance))
                    {
                        continue;
                    }

                    Planar.Point2D point2D = plane.Convert(point3D_Project);
                    if (point2D == null)
                    {
                        continue;
                    }

                    if (tuples.Find(x => x.Item1.AlmostEquals(point2D, tolerance)) != null)
                    {
                        continue;
                    }

                    Vector3D vector3D = new Vector3D(point3D_Project, point3D);

                    tuples.Add(new Tuple<Planar.Point2D, Vector3D>(point2D, vector3D));
                }
            }

            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(face2D, tuples?.ConvertAll(x => x.Item1), tolerance);
            if (triangle2Ds == null)
            {
                return null;
            }

            if (tuples == null || tuples.Count == 0)
            {
                return triangle2Ds.ConvertAll(x => plane.Convert(x));
            }

            List<Triangle3D> result = new List<Triangle3D>();
            foreach (Planar.Triangle2D triangle2D in triangle2Ds)
            {
                List<Planar.Point2D> point2Ds = triangle2D.GetPoints();
                if (point2Ds == null || point2Ds.Count < 3)
                {
                    continue;
                }

                List<Point3D> point3Ds_Triangle3D = new List<Point3D>();
                foreach (Planar.Point2D point2D in point2Ds)
                {
                    Point3D point3D = plane.Convert(point2D);

                    Tuple<Planar.Point2D, Vector3D> tuple = tuples.Find(x => x.Item1.AlmostEquals(point2D, tolerance));
                    if (tuple != null)
                    {
                        point3D.Move(tuple.Item2);
                    }

                    point3Ds_Triangle3D.Add(point3D);
                }

                result.Add(new Triangle3D(point3Ds_Triangle3D[0], point3Ds_Triangle3D[1], point3Ds_Triangle3D[2]));
            }

            return result;
        }

        public static List<Triangle3D> Triangulate(this Sphere sphere, int density)
        {
            if (sphere == null || density < 1)
            {
                return null;
            }

            double factor = System.Math.PI / density;

            List<List<Point3D>> point3DList = new List<List<Point3D>>();
            for (int i = 0; i <= density; i++)
            {
                double value_1 = i * factor;

                List<Point3D> point3Ds = new List<Point3D>();
                for (int j = -density; j <= density; j++)
                {
                    double value_2 = j * factor;

                    Point3D point3D = sphere.Convert(new Planar.Point2D(value_1, value_2));

                    point3Ds.Add(point3D);
                }

                point3DList.Add(point3Ds);
            }

            List<Triangle3D> result = new List<Triangle3D>();
            for (int i = 0; i < point3DList.Count - 1; i++)
            {
                for (int j = 1; j < point3DList[i].Count; j++)
                {
                    Triangle3D triangle3D = null;

                    triangle3D = new Triangle3D(point3DList[i][j - 1], point3DList[i + 1][j - 1], point3DList[i + 1][j]);
                    result.Add(triangle3D);

                    if(point3DList[i][j - 1] != point3DList[i][j])
                    {
                        triangle3D = new Triangle3D(point3DList[i][j - 1], point3DList[i + 1][j], point3DList[i][j]);
                        result.Add(triangle3D);
                    }
                }
            }

            return result;
        }

        public static List<Triangle3D> Triangulate(this Circle3D circle3D, int density)
        {
            if(circle3D == null || density < 1)
            {
                return null;
            }

            Plane plane = circle3D.GetPlane();
            if (plane == null)
            {
                return null;
            }

            Planar.Circle2D circle2D = new Planar.Circle2D(plane.Convert(circle3D.Center), circle3D.Radious);
            List<Planar.Triangle2D> triangle2Ds = Planar.Query.Triangulate(circle2D, density);
            if(triangle2Ds == null || triangle2Ds.Count == 0)
            {
                return null;
            }

            return triangle2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}