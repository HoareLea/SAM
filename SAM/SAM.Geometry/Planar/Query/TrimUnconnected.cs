using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Segment2D> TrimUnconnected(this IEnumerable<Segment2D> segment2Ds, double minLength, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(segment2Ds == null)
            {
                return null;
            }

            List<Segment2D> result = segment2Ds.Split(tolerance);
            result = result.Snap(true, snapTolerance);

            List<Point2D> point2Ds_Unique = segment2Ds.UniquePoint2Ds(tolerance);
            if (point2Ds_Unique == null || point2Ds_Unique.Count <= 2)
            {
                return result;
            }

            bool removed = true;
            while (removed)
            {
                removed = false;
                foreach (Point2D point2D_Unique in point2Ds_Unique)
                {
                    List<Segment2D> segment2Ds_Temp = result.FindAll(x => x[0].AlmostEquals(point2D_Unique, tolerance) || x[1].AlmostEquals(point2D_Unique, tolerance));
                    if (segment2Ds_Temp.Count == 1)
                    {
                        Segment2D segment2D = segment2Ds_Temp[0];
                        if(segment2D.GetLength() < minLength)
                        {
                            result.Remove(segment2D);
                            removed = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}