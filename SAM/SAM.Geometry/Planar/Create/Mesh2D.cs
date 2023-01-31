using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Mesh2D Mesh2D(this IEnumerable< Triangle2D> triangle2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (triangle2Ds == null || triangle2Ds.Count() == 0)
            {
                return null;
            }

            List<Point2D> point2Ds = new List<Point2D>();

            List<Tuple<int, int, int>> tuples = new List<Tuple<int, int, int>>();

            foreach (Triangle2D triangle2D in triangle2Ds)
            {
                if (triangle2D == null || !triangle2D.IsValid() || triangle2D.GetArea() < tolerance)
                {
                    continue;
                }

                List<Point2D> point2Ds_Triangle = triangle2D.GetPoints();
                if (point2Ds_Triangle == null || point2Ds_Triangle.Count < 3)
                {
                    continue;
                }

                int index_1 = -1;
                index_1 = point2Ds.FindIndex(x => x.AlmostEquals(point2Ds_Triangle[0], tolerance));
                if (index_1 == -1)
                {
                    index_1 = point2Ds.Count;
                    point2Ds.Add(point2Ds_Triangle[0]);
                }

                int index_2 = -1;
                index_2 = point2Ds.FindIndex(x => x.AlmostEquals(point2Ds_Triangle[1], tolerance));
                if (index_2 == -1)
                {
                    index_2 = point2Ds.Count;
                    point2Ds.Add(point2Ds_Triangle[1]);
                }

                int index_3 = -1;
                index_3 = point2Ds.FindIndex(x => x.AlmostEquals(point2Ds_Triangle[2], tolerance));
                if (index_3 == -1)
                {
                    index_3 = point2Ds.Count;
                    point2Ds.Add(point2Ds_Triangle[2]);
                }

                tuples.Add(new Tuple<int, int, int>(index_1, index_2, index_3));
            }

            return new Mesh2D(point2Ds, tuples);
        }
    }
}