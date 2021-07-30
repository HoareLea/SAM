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

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            point2Ds_Temp.Sort(new ConvexHullComparer());

            List<Point2D> point2Ds_Temp_UpperHull = new List<Point2D>();
            foreach (Point2D point2D in point2Ds_Temp)
            {
                while (point2Ds_Temp_UpperHull.Count >= 2)
                {
                    Point2D point2D_1 = point2Ds_Temp_UpperHull[point2Ds_Temp_UpperHull.Count - 1];
                    Point2D point2D_2 = point2Ds_Temp_UpperHull[point2Ds_Temp_UpperHull.Count - 2];
                    if ((point2D_1.X - point2D_2.X) * (point2D.Y - point2D_2.Y) >= (point2D_1.Y - point2D_2.Y) * (point2D.X - point2D_2.X))
                        point2Ds_Temp_UpperHull.RemoveAt(point2Ds_Temp_UpperHull.Count - 1);
                    else
                        break;
                }
                point2Ds_Temp_UpperHull.Add(point2D);
            }
            point2Ds_Temp_UpperHull.RemoveAt(point2Ds_Temp_UpperHull.Count - 1);

            IList<Point2D> point2DList_LowerHull = new List<Point2D>();
            for (int i = point2Ds_Temp.Count - 1; i >= 0; i--)
            {
                Point2D point2D = point2Ds_Temp[i];
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

            if (!(point2Ds_Temp_UpperHull.Count == 1 && Enumerable.SequenceEqual(point2Ds_Temp_UpperHull, point2DList_LowerHull)))
                point2Ds_Temp_UpperHull.AddRange(point2DList_LowerHull);
            return point2Ds_Temp_UpperHull;
        }

        public static List<Point2D> ConvexHull(this IEnumerable<Segment2D> segment2Ds)
        {
            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                point2Ds.Add(segment2D[0]);
                point2Ds.Add(segment2D[1]);
            }

            return ConvexHull(point2Ds);
        }

        public static List<Point2D> ConvexHull(this ISegmentable2D segmentable2D)
        {
            return ConvexHull(segmentable2D.GetSegments());
        }
    }
}