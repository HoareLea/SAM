using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> ConvexHull(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return null;

            if (point2Ds.Count() <= 3)
                return new List<Point2D>(point2Ds);

            List<Point2D> point2DList = new List<Point2D>(point2Ds);
            point2DList.Sort(new ConvexHullComparer());

            List<Point2D> point2DList_UpperHull = new List<Point2D>();
            foreach (Point2D point2D in point2DList)
            {
                while (point2DList_UpperHull.Count >= 2)
                {
                    Point2D point2D_1 = point2DList_UpperHull[point2DList_UpperHull.Count - 1];
                    Point2D point2D_2 = point2DList_UpperHull[point2DList_UpperHull.Count - 2];
                    if ((point2D_1.X - point2D_2.X) * (point2D.Y - point2D_2.Y) >= (point2D_1.Y - point2D_2.Y) * (point2D.X - point2D_2.X))
                        point2DList_UpperHull.RemoveAt(point2DList_UpperHull.Count - 1);
                    else
                        break;
                }
                point2DList_UpperHull.Add(point2D);
            }
            point2DList_UpperHull.RemoveAt(point2DList_UpperHull.Count - 1);

            IList<Point2D> point2DList_LowerHull = new List<Point2D>();
            for (int i = point2DList.Count - 1; i >= 0; i--)
            {
                Point2D point2D = point2DList[i];
                while (point2DList_LowerHull.Count >= 2)
                {
                    Point2D point2D_1 = point2DList_LowerHull[point2DList_LowerHull.Count - 1];
                    Point2D point2D_2 = point2DList_LowerHull[point2DList_LowerHull.Count - 2];
                    if ((point2D_1.X - point2D_2.X) * (point2D.Y - point2D_2.Y) >= (point2D_1.Y - point2D_2.Y) * (point2D.X - point2D_2.X))
                        point2DList_LowerHull.RemoveAt(point2DList_LowerHull.Count - 1);
                    else
                        break;
                }
                point2DList_LowerHull.Add(point2D);
            }
            point2DList_LowerHull.RemoveAt(point2DList_LowerHull.Count - 1);

            if (!(point2DList_UpperHull.Count == 1 && Enumerable.SequenceEqual(point2DList_UpperHull, point2DList_LowerHull)))
                point2DList_UpperHull.AddRange(point2DList_LowerHull);
            return point2DList_UpperHull;
        }

        public static List<Point2D> ConvexHull(this IEnumerable<Segment2D> segment2Ds)
        {
            List<Point2D> aPointList = new List<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                aPointList.Add(segment2D[0]);
                aPointList.Add(segment2D[1]);
            }

            return ConvexHull(aPointList);
        }

        private class ConvexHullComparer : IComparer<Point2D>
        {
            public int Compare(Point2D point2D_1, Point2D point2D_2)
            {
                if (point2D_1.X < point2D_2.X)
                    return -1;
                else if (point2D_1.X > point2D_2.X)
                    return +1;
                else if (point2D_1.Y < point2D_2.Y)
                    return -1;
                else if (point2D_1.Y > point2D_2.Y)
                    return +1;
                else
                    return 0;
            }
        }
    }
}