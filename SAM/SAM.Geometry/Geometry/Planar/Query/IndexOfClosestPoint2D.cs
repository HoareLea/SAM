using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static int IndexOfClosestPoint2D(this ISegmentable2D segmentable2D, Point2D point2D)
        {
            if (segmentable2D == null || point2D == null)
                return -1;

            List<Point2D> point2Ds = segmentable2D.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return -1;

            int index = -1;
            double distance_Min = double.MaxValue;
            for (int i = 0; i < point2Ds.Count(); i++)
            {
                if (point2Ds[i] == null)
                    continue;

                double distance = point2D.Distance(point2Ds[i]);
                if (distance < distance_Min)
                {
                    distance_Min = distance;
                    index = i;
                }
            }

            return index;
        }
    }
}