using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        /// <summary>
        /// Split Segment2Ds  
        /// </summary>
        /// <returns>
        /// List Segment2D
        /// </returns>
        /// <param name="segment2Ds">Sermnets2Ds</param>
        /// <param name="tolerance"> tolerance (default = 0) .</param>
        public static List<Segment2D> Split(this IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null)
                return null;

            int aCount = segment2Ds.Count();

            Dictionary<int, List<Point2D>> aPointDictionary = new Dictionary<int, List<Point2D>>();

            for (int i = 0; i < aCount - 1; i++)
            {
                Segment2D segment2D_1 = segment2Ds.ElementAt(i);
                for (int j = i + 1; j < aCount; j++)
                {
                    Segment2D segment2D_2 = segment2Ds.ElementAt(j);

                    Point2D point2D_Closest1;
                    Point2D point2D_Closest2;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest1, out point2D_Closest2, tolerance);
                    if (point2D_Intersection == null || point2D_Intersection.IsNaN())
                        continue;

                    if (point2D_Closest1 != null && point2D_Closest2 != null)
                        if (point2D_Closest1.Distance(point2D_Closest2) > tolerance)
                            continue;

                    List<Point2D> aPointList;

                    if (point2D_Intersection.Distance(segment2D_1.Start) > tolerance && point2D_Intersection.Distance(segment2D_1.End) > tolerance)
                    {
                        if (!aPointDictionary.TryGetValue(i, out aPointList))
                        {
                            aPointList = new List<Point2D>();
                            aPointDictionary[i] = aPointList;
                        }

                        Point2D.Add(aPointList, point2D_Intersection, tolerance);
                    }

                    if (point2D_Intersection.Distance(segment2D_2.Start) > tolerance && point2D_Intersection.Distance(segment2D_2.End) > tolerance)
                    {
                        if (!aPointDictionary.TryGetValue(j, out aPointList))
                        {
                            aPointList = new List<Point2D>();
                            aPointDictionary[j] = aPointList;
                        }

                        Point2D.Add(aPointList, point2D_Intersection, tolerance);
                    }
                }
            }

            List<Segment2D> aResult = new List<Segment2D>();

            for (int i = 0; i < aCount; i++)
            {
                Segment2D segment2D_Temp = segment2Ds.ElementAt(i);

                List<Point2D> aPointList;
                if (!aPointDictionary.TryGetValue(i, out aPointList))
                {
                    aResult.Add(segment2D_Temp);
                    continue;
                }

                Point2D.Add(aPointList, segment2D_Temp[0], tolerance);
                Point2D.Add(aPointList, segment2D_Temp[1], tolerance);

                aPointList = Point2D.SortByDistance(segment2D_Temp[0], aPointList);

                for (int j = 0; j < aPointList.Count - 1; j++)
                    aResult.Add(new Segment2D(aPointList[j], aPointList[j + 1]));
            }

            return aResult;
        }

        public static List<Segment2D> Split(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = 0)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp == null)
                    continue;

                segment2Ds.AddRange(segment2Ds_Temp);
            }

            return Split(segment2Ds, tolerance);
        }

    }
}
