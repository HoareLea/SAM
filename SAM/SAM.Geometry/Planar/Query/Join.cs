using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Face2D Join(this Face2D face2D, IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D == null)
                return null;

            if (face2Ds == null || face2Ds.Count() == 0)
                return new Face2D(face2D);

            Point2D point2D = face2D.GetInternalPoint2D();
            if (point2D == null)
                return new Face2D(face2D);

            List<Face2D> face2Ds_Temp = new List<Face2D>(face2Ds);
            face2Ds_Temp.Add(face2D);

            face2Ds_Temp = face2Ds_Temp.Union(tolerance);
            if (face2Ds_Temp == null || face2Ds_Temp.Count == 0)
                return new Face2D(face2D);

            Face2D result = face2Ds_Temp.Find(x => x.Inside(point2D, tolerance));
            if (result == null)
                return new Face2D(face2D);

            return result;
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