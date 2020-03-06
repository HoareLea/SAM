using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Closest(this ISegmentable2D segmentable2D, Point2D point2D)
        {
            if (segmentable2D == null || point2D == null)
                return null;

            double min = double.MaxValue;
            Point2D result = null;
            foreach(Segment2D segment2D in segmentable2D.GetSegments())
            {
                Point2D point2D_Closest = segment2D.Closest(point2D);
                double distance = point2D.Distance(point2D_Closest);
                if(distance < min)
                {
                    result = point2D_Closest;
                    min = distance;
                }
            }

            return result;
        }

        public static Point2D Closest(this IEnumerable<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return null;
            
            double distance_Min = double.MaxValue;
            Point2D result = null;
            foreach (Point2D point2D_Temp in point2Ds)
            {
                double distance = point2D.Distance(point2D_Temp);
                if (distance < distance_Min)
                {
                    result = point2D_Temp;
                    if (distance == 0)
                        return result;

                    distance_Min = distance;
                }
            }
            return result;
        }

    }
}
