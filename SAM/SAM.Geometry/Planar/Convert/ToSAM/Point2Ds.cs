using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static List<Point2D> ToSAM(this IEnumerable<Coordinate> coordinates, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (coordinates == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Coordinate coordinate in coordinates)
                point2Ds.Add(coordinate.ToSAM(tolerance));

            return point2Ds;
        }

        public static List<Point2D> ToSAM(MultiPoint multiPoint, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(multiPoint == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            foreach(Coordinate coordinate in multiPoint.Coordinates)
            {
                Point2D point2D = coordinate?.ToSAM();
                if(point2D == null)
                {
                    continue;
                }

                result.Add(point2D);
            }

            return result;
        }
    }
}