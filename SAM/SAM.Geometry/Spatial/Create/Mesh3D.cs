using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Mesh3D Mesh3D(this IEnumerable<Point3D> point3Ds, IEnumerable<int> indexes)
        {
            if(point3Ds == null || indexes == null)
            {
                return null;
            }

            int count = point3Ds.Count();
            if(count < 3)
            {
                return null;
            }

            List<Tuple<int, int, int>> tuples = new List<Tuple<int, int, int>>();

            List<int> indexes_Temp = indexes.ToList();
            for(int i =0; i < indexes_Temp.Count; i = i + 3)
            {
                int index_1 = indexes_Temp[i];
                if(index_1 < 0 || index_1 >= count)
                {
                    return null;
                }

                int index_2 = indexes_Temp[i + 1];
                if (index_2 < 0 || index_2 >= count)
                {
                    return null;
                }

                int index_3 = indexes_Temp[i + 2];
                if (index_3 < 0 || index_3 >= count)
                {
                    return null;
                }

                tuples.Add(new Tuple<int, int, int>(index_1, index_2, index_3));
            }

            return new Mesh3D(point3Ds, tuples);
        }

        public static Mesh3D Mesh3D(this IEnumerable<Triangle3D> triangle3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(triangle3Ds == null || triangle3Ds.Count() == 0)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();

            List<Tuple<int, int, int>> tuples = new List<Tuple<int, int, int>>();

            foreach (Triangle3D triangle3D in triangle3Ds)
            {
                if(triangle3D == null || !triangle3D.IsValid() ||triangle3D.GetArea() < tolerance)
                {
                    continue;
                }

                List<Point3D> point3Ds_Triangle = triangle3D.GetPoints();
                if (point3Ds_Triangle == null || point3Ds_Triangle.Count < 3)
                {
                    continue;
                }

                int index_1 = -1;
                index_1 = point3Ds.FindIndex(x => x.AlmostEquals(point3Ds_Triangle[0], tolerance));
                if(index_1 == -1)
                {
                    index_1 = point3Ds.Count;
                    point3Ds.Add(point3Ds_Triangle[0]);
                }

                int index_2 = -1;
                index_2 = point3Ds.FindIndex(x => x.AlmostEquals(point3Ds_Triangle[1], tolerance));
                if (index_2 == -1)
                {
                    index_2 = point3Ds.Count;
                    point3Ds.Add(point3Ds_Triangle[1]);
                }

                int index_3 = -1;
                index_3 = point3Ds.FindIndex(x => x.AlmostEquals(point3Ds_Triangle[2], tolerance));
                if (index_3 == -1)
                {
                    index_3 = point3Ds.Count;
                    point3Ds.Add(point3Ds_Triangle[2]);
                }

                tuples.Add(new Tuple<int, int, int>(index_1, index_2, index_3));
            }

            return new Mesh3D(point3Ds, tuples);
        }

        public static Mesh3D Mesh3D(this Mesh2D mesh2D, Plane plane)
        {
            if(mesh2D == null || plane == null)
            {
                return null;
            }

            List<Point2D> point2Ds = mesh2D.GetPoints();
            if(point2Ds == null)
            {
                return null;
            }

            List<Tuple<int, int, int>> tuples = new List<Tuple<int, int, int>>();
            List<Triangle2D> triangle2Ds = mesh2D.GetTriangles();
            foreach(Triangle2D triangle2D in triangle2Ds)
            {
                List<Point2D> point2Ds_Triangle = triangle2D.GetPoints();
                tuples.Add(new Tuple<int, int, int>(mesh2D.IndexOf(point2Ds_Triangle[0]), mesh2D.IndexOf(point2Ds_Triangle[1]), mesh2D.IndexOf(point2Ds_Triangle[2])));
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach(Point2D point2D in point2Ds)
            {
                point3Ds.Add(plane.Convert(point2D));
            }

            return new Mesh3D(point3Ds, tuples);
        }

        public static Mesh3D Mesh3D(this Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            if(shell == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell?.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Triangle3D> triangle3Ds = new List<Triangle3D>();
            foreach(Face3D face3D in face3Ds)
            {
                List<Triangle3D> triangle3Ds_Face3D = Query.Triangulate(face3D, tolerance);
                if(triangle3Ds_Face3D == null || triangle3Ds_Face3D.Count == 0)
                {
                    continue;
                }

                Vector3D normal = face3D.GetPlane().Normal;

                foreach(Triangle3D triangle3D in triangle3Ds_Face3D)
                {
                    if(!triangle3D.GetNormal().SameHalf(normal))
                    {
                        triangle3D.Reverse();
                    }

                    triangle3Ds.Add(triangle3D);
                }
            }

            return Mesh3D(triangle3Ds, tolerance);
        }

        public static Mesh3D Mesh3D(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            List<Triangle3D> triangle3Ds = face3D?.Triangulate(tolerance);
            if (triangle3Ds == null || triangle3Ds.Count == 0)
            {
                return null;
            }

            return Mesh3D(triangle3Ds, tolerance);
        }

        public static Mesh3D Mesh3D(this Polygon3D polygon3D, double tolerance = Core.Tolerance.Distance)
        {
            List<Triangle3D> triangle3Ds = polygon3D?.Triangulate(tolerance);
            if (triangle3Ds == null || triangle3Ds.Count == 0)
            {
                return null;
            }

            return Mesh3D(triangle3Ds, tolerance);
        }

        public static Mesh3D Mesh3D(this Sphere sphere, double factor, int minDensity = 2, int maxDensity = 10)
        {
            int denisty = System.Convert.ToInt32(System.Math.Ceiling(sphere.Radious / factor));
            if(denisty < minDensity)
            {
                denisty = minDensity;
            }
            else if(denisty > maxDensity)
            {
                denisty = maxDensity;
            }

            List<Triangle3D> trinagle3Ds = sphere.Triangulate(denisty);
            if(trinagle3Ds == null)
            {
                return null;
            }

            return new Mesh3D(trinagle3Ds);
        }

        public static Mesh3D Mesh3D(this Circle3D circle3D, double factor, int minDensity = 2, int maxDensity = 10)
        {
            int denisty = System.Convert.ToInt32(System.Math.Ceiling(circle3D.Radious / factor));
            if (denisty < minDensity)
            {
                denisty = minDensity;
            }
            else if (denisty > maxDensity)
            {
                denisty = maxDensity;
            }

            List<Triangle3D> trinagle3Ds = circle3D.Triangulate(denisty);
            if (trinagle3Ds == null)
            {
                return null;
            }

            return new Mesh3D(trinagle3Ds);
        }
    }
}