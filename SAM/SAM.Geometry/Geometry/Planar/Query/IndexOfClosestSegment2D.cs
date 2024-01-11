using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static int IndexOfClosestSegment2D(this IEnumerable<Segment2D> segment2Ds, Point2D point2D)
        {
            if (segment2Ds == null || point2D == null)
                return -1;

            int index = -1;
            double distance_Min = double.MaxValue;
            for (int i = 0; i < segment2Ds.Count(); i++)
            {
                Segment2D segment2D = segment2Ds.ElementAt(i);
                if (segment2D == null)
                    continue;

                double distance = segment2D.Closest(point2D).Distance(point2D);
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