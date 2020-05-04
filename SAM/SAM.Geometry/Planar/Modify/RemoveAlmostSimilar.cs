using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void RemoveAlmostSimilar_NTS<T>(this List<T> geometries, double tolerance = Core.Tolerance.Distance) where T : NetTopologySuite.Geometries.Geometry
        {
            if (geometries == null)
                return;

            HashSet<int> indexes_HashSet = new HashSet<int>();
            for (int i = 0; i < geometries.Count - 1; i++)
            {
                if (indexes_HashSet.Contains(i))
                    continue;

                NetTopologySuite.Geometries.Geometry geometry_1 = geometries[i];

                for (int j = i + 1; j < geometries.Count; j++)
                {
                    if (indexes_HashSet.Contains(j))
                        continue;

                    NetTopologySuite.Geometries.Geometry geometry_2 = geometries[j];

                    if (Query.AlmostSimilar(geometry_1, geometry_2, tolerance))
                        indexes_HashSet.Add(j);
                }
            }

            List<int> indexes_List = indexes_HashSet.ToList();
            indexes_List.Sort();
            indexes_List.Reverse();

            indexes_List.ForEach(x => geometries.RemoveAt(x));
        }

        /// <summary>
        /// Removes segments from segment2Ds list which are similar to segmentable2D segments
        /// </summary>
        public static void RemoveAlmostSimilar(this ISegmentable2D segmentable2D, List<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D == null || segment2Ds == null || segment2Ds.Count() == 0)
                return;

            List<Segment2D> segment2Ds_Segmentable = segmentable2D.GetSegments();

            HashSet<int> indexes = new HashSet<int>();
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                foreach (Segment2D segment2D_Segmentable in segment2Ds_Segmentable)
                {
                    if (!segment2Ds[i].AlmostSimilar(segment2D_Segmentable, tolerance))
                        continue;

                    indexes.Add(i);
                    break;
                }
            }

            if (indexes.Count == 0)
                return;

            List<int> indexes_List = indexes.ToList();
            indexes_List.Sort((x, y) => y.CompareTo(x));

            indexes_List.ForEach(x => segment2Ds.RemoveAt(x));
        }

        /// <summary>
        /// Removes segments from segment2Ds list which are similar to segmentable2D segments
        /// </summary>
        public static void RemoveAlmostSimilar<T>(List<T> segmentable2Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable2D
        {
            if (segmentable2Ds == null)
                return;

            List<T> result = new List<T>();
            foreach (T segmentable2D in segmentable2Ds)
                if (result.Find(x => Query.AlmostSimilar(x, segmentable2D, tolerance)) == null)
                    result.Add(segmentable2D);

            segmentable2Ds.Clear();
            segmentable2Ds.AddRange(result);
        }
    }
}