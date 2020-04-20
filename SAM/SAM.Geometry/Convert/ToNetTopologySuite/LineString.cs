using System.Collections.Generic;

using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;


namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static LineString ToNetTopologySuite(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Point2D> point2Ds = segmentable2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            return new LineString(point2Ds.ToNetTopologySuite().ToArray());
        }
    }
}
