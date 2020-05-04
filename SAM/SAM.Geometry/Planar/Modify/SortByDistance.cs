using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void SortByDistance(this List<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return;

            point2Ds.RemoveAll(x => x == null);

            point2Ds.Sort((x, y) => point2D.Distance(x).CompareTo(point2D.Distance(y)));
        }

        public static void SortByDistance(this List<Segment2D> segment2Ds, Point2D point2D)
        {
            if (segment2Ds == null || point2D == null)
                return;

            List<Tuple<double, Segment2D>> tuples = new List<Tuple<double, Segment2D>>();
            segment2Ds.ForEach(x => tuples.Add(new Tuple<double, Segment2D>(x.Closest(point2D).Distance(point2D), x)));

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            segment2Ds.Clear();

            segment2Ds.AddRange(tuples.ConvertAll(x => x.Item2));
        }
    }
}