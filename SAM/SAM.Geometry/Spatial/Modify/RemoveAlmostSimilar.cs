using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        /// <summary>
        /// Removes segments from segment2Ds list which are similar to segmentable2D segments
        /// </summary>
        public static void RemoveAlmostSimilar(this ISegmentable3D segmentable3D, List<Segment3D> segment3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3D == null || segment3Ds == null || segment3Ds.Count() == 0)
                return;

            List<Segment3D> segment3Ds_Segmentable = segmentable3D.GetSegments();

            HashSet<int> indexes = new HashSet<int>();
            for (int i = 0; i < segment3Ds.Count; i++)
            {
                foreach (Segment3D segment3D_Segmentable in segment3Ds_Segmentable)
                {
                    if (!segment3Ds[i].AlmostSimilar(segment3D_Segmentable, tolerance))
                        continue;

                    indexes.Add(i);
                    break;
                }
            }

            if (indexes.Count == 0)
                return;

            List<int> indexes_List = indexes.ToList();
            indexes_List.Sort((x, y) => y.CompareTo(x));

            indexes_List.ForEach(x => segment3Ds.RemoveAt(x));
        }

        public static void RemoveAlmostSimilar<T>(List<T> segmentable3Ds, double tolerance = Core.Tolerance.Distance) where T : ISegmentable3D
        {
            if (segmentable3Ds == null)
                return;

            List<T> result = new List<T>();
            foreach (T segmentable3D in segmentable3Ds)
                if (result.Find(x => Query.AlmostSimilar(x, segmentable3D, tolerance)) == null)
                    result.Add(segmentable3D);

            segmentable3Ds.Clear();
            segmentable3Ds.AddRange(result);
        }

        public static void RemoveAlmostSimilar(List<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return;

            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                if (result.Find(x => x.AlmostEquals(point3D, tolerance)) == null)
                    result.Add(point3D);

            point3Ds.Clear();
            point3Ds.AddRange(result);
        }
    }
}