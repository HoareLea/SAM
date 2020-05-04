using ClipperLib;
using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<IntPoint> ToClipper(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2D == null)
                return null;

            return ToClipper(segmentable2D.GetPoints(), tolerance);
        }

        public static List<IntPoint> ToClipper(this IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2Ds == null)
                return null;

            if (point2Ds.Count() == 0)
                return new List<IntPoint>();

            List<IntPoint> result = new List<IntPoint>();
            foreach (Point2D point2D in point2Ds)
                result.Add(point2D.ToClipper(tolerance));

            return result;
        }

        public static List<IntPoint> ToClipper(this IEnumerable<Coordinate> coordinates, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (coordinates == null)
                return null;

            if (coordinates.Count() == 0)
                return new List<IntPoint>();

            List<IntPoint> result = new List<IntPoint>();
            foreach (Coordinate coordinate in coordinates)
                result.Add(coordinate.ToClipper(tolerance));

            return result;
        }

        public static List<IntPoint> ToClipper(this LinearRing linearRing, double tolerance = Core.Tolerance.MicroDistance)
        {
            return linearRing?.Coordinates?.ToClipper(tolerance);
        }
    }
}