using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D SelfIntersectionPoint2D(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            List<Point2D> result = new List<Point2D>();

            PointGraph2D pointGraph2D = new PointGraph2D(polygon2D.GetSegments(), true, tolerance);
            for (int i = 0; i < pointGraph2D.Count; i++)
            {
                int count = pointGraph2D.ConnectionsCount(i);
                if (count > 2)
                    return pointGraph2D[i];
            }

            return null;
        }
    }
}