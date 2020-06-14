using ClipperLib;
using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static LinearRing ToNTS(this IClosed2D closed2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;

            List<Point2D> point2Ds = segmentable2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            point2Ds.Add(point2Ds.First());

            return new LinearRing(point2Ds.ToNTS(tolerance).ToArray());
        }

        public static LinearRing ToNTS_LinearRing(this IEnumerable<IntPoint> intPoints, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Coordinate> coordinates = intPoints?.ToNTS(tolerance);
            if (coordinates == null || coordinates.Count < 2)
                return null;

            if (coordinates.Last() != coordinates.First())
                coordinates.Add(coordinates[0]);

            return new LinearRing(coordinates.ToArray());
        }

        public static LinearRing ToNTS_LinearRing(this IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Coordinate> coordinates = point2Ds?.ToNTS(tolerance);
            if (coordinates == null || coordinates.Count < 2)
                return null;

            if (coordinates.Last() != coordinates.First())
                coordinates.Add(coordinates[0]);

            return new LinearRing(coordinates.ToArray());
        }
    }
}