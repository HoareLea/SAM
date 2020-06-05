using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Split Segment2Ds
        /// </summary>
        /// <returns>List Segment2D</returns>
        /// <param name="segment2Ds">Sermnets2Ds</param>
        /// <param name="tolerance">tolerance (default = 0) .</param>
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

                        Planar.Point2D.Add(aPointList, point2D_Intersection, tolerance);
                    }

                    if (point2D_Intersection.Distance(segment2D_2.Start) > tolerance && point2D_Intersection.Distance(segment2D_2.End) > tolerance)
                    {
                        if (!aPointDictionary.TryGetValue(j, out aPointList))
                        {
                            aPointList = new List<Point2D>();
                            aPointDictionary[j] = aPointList;
                        }

                        Planar.Point2D.Add(aPointList, point2D_Intersection, tolerance);
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

                Planar.Point2D.Add(aPointList, segment2D_Temp[0], tolerance);
                Planar.Point2D.Add(aPointList, segment2D_Temp[1], tolerance);

                aPointList = Planar.Point2D.SortByDistance(segment2D_Temp[0], aPointList);

                for (int j = 0; j < aPointList.Count - 1; j++)
                    aResult.Add(new Segment2D(aPointList[j], aPointList[j + 1]));
            }

            return aResult;
        }

        public static List<Segment2D> Split(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
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

        public static List<Segment2D> Split(this ISegmentable2D segmentable2D, IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || segment2Ds == null)
                return null;

            if (segment2Ds.Count() == 0)
                return new List<Segment2D>();

            List<Segment2D> result = new List<Segment2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                List<Point2D> point2Ds = segmentable2D.Intersections(segment2D, tolerance);
                if (point2Ds == null || point2Ds.Count == 0)
                {
                    result.Add(new Segment2D(segment2D));
                    continue;
                }

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[0])) == null)
                    point2Ds.Add(segment2D[0]);

                if (point2Ds.Find(x => x.AlmostEquals(segment2D[1])) == null)
                    point2Ds.Add(segment2D[1]);

                Modify.SortByDistance(point2Ds, segment2D[0]);

                List<Segment2D> segment2Ds_Temp = Create.Segment2Ds(point2Ds, false);
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
                    result.AddRange(segment2Ds_Temp);
            }

            return result;
        }
        
        public static List<Face2D> Split(this IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2Ds == null)
                return null;

            if (face2Ds.Count() <= 1)
                return new List<Face2D>(face2Ds);

            MultiPolygon multiPolygon = face2Ds.ToNTS(tolerance);
            if (multiPolygon == null || multiPolygon.IsEmpty)
                return null;

            List<Polygon> polygons = multiPolygon.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            return polygons.ConvertAll(x => x.ToSAM(tolerance));
        }

        /// <summary>
        /// Generates additional points between two points where point2D_1 is first point on list and point2D_2 is last point.
        /// </summary>
        /// <param name="point2D_1">First point</param>
        /// <param name="point2D_2">Last point</param>
        /// <param name="count">Number of additional points</param>
        /// <returns></returns>
        public static IEnumerable<Point2D> Split(this Point2D point2D_1, Point2D point2D_2, int count)
        {
            if (point2D_1 == null || point2D_2 == null)
                return null;

            if (count <= 0)
                return null;

            if (count == 1)
                return new Point2D[] { point2D_1, point2D_2 };

            Vector2D vector2D = new Vector2D(point2D_1, point2D_2);
            double aLength_Split = vector2D.Length / count;
            vector2D = vector2D.Unit * aLength_Split;

            Point2D[] aResult = new Point2D[count + 1];

            aResult[0] = new Point2D(point2D_1);
            for (int i = 0; i < count; i++)
                aResult[i + 1] = aResult[i].GetMoved(vector2D);

            aResult[count] = new Point2D(point2D_2);

            return aResult;
        }
    }
}