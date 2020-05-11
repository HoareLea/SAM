using ClipperLib;
using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Point2D> ToSAM(this IEnumerable<IntPoint> intPoints, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (intPoints == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (IntPoint intPoint in intPoints)
                point2Ds.Add(intPoint.ToSAM(tolerance));

            return point2Ds;
        }

        public static List<Point2D> ToSAM(this IEnumerable<Coordinate> coordinates, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (coordinates == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Coordinate coordinate in coordinates)
                point2Ds.Add(coordinate.ToSAM(tolerance));

            return point2Ds;
        }
    }
}