using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool Join(this List<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null || segment2Ds.Count < 2)
                return false;

            List<Segment2D> result = new List<Segment2D>();
            result.Add(segment2Ds[0]);
            for (int i = 1; i < segment2Ds.Count; i++)
            {
                Segment2D segment_Previous = result.Last();
                Segment2D segment = segment2Ds[i];

                Point2D point2D_Intersection = segment_Previous.Intersection(segment, false, tolerance);
                if (point2D_Intersection == null)
                {
                    result.Add(new Segment2D(segment_Previous[0], segment[0]));
                    result.Add(new Segment2D(segment[0], segment[1]));
                }
                else
                {
                    result[result.Count - 1] = new Segment2D(segment_Previous[0], point2D_Intersection);
                    result.Add(new Segment2D(point2D_Intersection, segment[1]));
                }
            }

            //if(close)
            //{
            //    Segment2D segment_Previous = result[result.Count - 1];
            //    Segment2D segment = result[0];

            // Point2D point2D_Intersection = segment.Intersection(segment_Previous, false,
            // tolerance); if (point2D_Intersection == null) { result.Add(new
            // Segment2D(segment_Previous[1], segment[0])); } else { result[result.Count - 1] = new
            // Segment2D(segment_Previous[0], point2D_Intersection); result[0] = new
            // Segment2D(point2D_Intersection, segment[1]); }

            //}

            segment2Ds.Clear();
            segment2Ds.AddRange(result);

            return true;
        }

        public static Polyline2D Join(this Polyline2D polyline2D_1, Polyline2D polyline2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D_1 == null || polyline2D_2 == null)
                return null;

            List<Segment2D> segment2Ds_1 = polyline2D_1.GetSegments();
            List<Segment2D> segment2Ds_2 = polyline2D_2.GetSegments();

            if (segment2Ds_1 == null || segment2Ds_2 == null || segment2Ds_1.Count == 0 || segment2Ds_2.Count == 0)
                return null;

            if (segment2Ds_1[0].Start.Distance(segment2Ds_2[0].Start) < tolerance)
            {
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[0].Start, segment2Ds_2[0].End);
                segment2Ds_2.Reverse();
                segment2Ds_2.ForEach(x => x.Reverse());

                segment2Ds_2.AddRange(segment2Ds_1);
                return new Polyline2D(segment2Ds_2);
            }

            int count_1 = segment2Ds_1.Count;

            if (segment2Ds_1[count_1 - 1].End.Distance(segment2Ds_2[0].Start) < tolerance)
            {
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[count_1 - 1].End, segment2Ds_2[0].End);

                segment2Ds_1.AddRange(segment2Ds_2);
                return new Polyline2D(segment2Ds_1);
            }

            int count_2 = segment2Ds_2.Count;

            if (segment2Ds_1[count_1 - 1].End.Distance(segment2Ds_2[count_2 - 1].End) < tolerance)
            {
                segment2Ds_2.Reverse();
                segment2Ds_2.ForEach(x => x.Reverse());
                segment2Ds_2[0] = new Segment2D(segment2Ds_1[count_1 - 1].End, segment2Ds_2[0].End);

                segment2Ds_1.AddRange(segment2Ds_2);
                return new Polyline2D(segment2Ds_1);
            }

            if (segment2Ds_1[0].Start.Distance(segment2Ds_2[count_2 - 1].End) < tolerance)
            {
                segment2Ds_2[count_2 - 1] = new Segment2D(segment2Ds_2[count_2 - 1].Start, segment2Ds_1[0].Start);

                segment2Ds_2.AddRange(segment2Ds_1);
                return new Polyline2D(segment2Ds_2);
            }

            return null;
        }
    }
}