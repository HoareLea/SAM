using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static void SortByDistance(this List<Point3D> point3Ds, Point3D point3D)
        {
            if (point3Ds == null || point3D == null)
                return;

            point3Ds.RemoveAll(x => x == null);

            point3Ds.Sort((x, y) => point3D.Distance(x).CompareTo(point3D.Distance(y)));
        }

        public static void SortByDistance(this List<Segment3D> segment3Ds, Point3D point3D)
        {
            if (segment3Ds == null || point3D == null)
                return;

            List<Tuple<double, Segment3D>> tuples = new List<Tuple<double, Segment3D>>();
            segment3Ds.ForEach(x => tuples.Add(new Tuple<double, Segment3D>(x.Closest(point3D).Distance(point3D), x)));

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            segment3Ds.Clear();

            segment3Ds.AddRange(tuples.ConvertAll(x => x.Item2));
        }
    }
}