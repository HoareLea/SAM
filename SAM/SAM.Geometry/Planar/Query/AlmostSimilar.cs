using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //public static bool AlmostSimilar(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        //{
        //    if (segment2D_1 == segment2D_2)
        //        return true;

        // if (segment2D_1 == null || segment2D_2 == null) return false;

        //    return (segment2D_1[0].AlmostEquals(segment2D_2[0], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[1], tolerance)) || (segment2D_1[0].AlmostEquals(segment2D_2[1], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[0], tolerance));
        //}

        public static bool AlmostSimilar(this ISegmentable2D segmentable2D_1, ISegmentable2D segmentable2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2D_1 == segmentable2D_2)
                return true;

            if (segmentable2D_1 == null || segmentable2D_2 == null)
                return false;

            List<Point2D> point2Ds = null;

            point2Ds = segmentable2D_1.GetPoints();
            foreach (Point2D point2D in point2Ds)
                if (!segmentable2D_2.On(point2D, tolerance))
                    return false;

            point2Ds = segmentable2D_2.GetPoints();
            foreach (Point2D point2D in point2Ds)
                if (!segmentable2D_1.On(point2D, tolerance))
                    return false;

            return true;
        }

        /// <summary>
        /// This method finds similar shapes by comparing point between two NetTopologySuite
        /// polygons to be continue WIP
        /// </summary>
        public static bool AlmostSimilar(this NetTopologySuite.Geometries.Geometry geometry_1, NetTopologySuite.Geometries.Geometry geometry_2, double tolerance = Core.Tolerance.Distance)
        {
            if (geometry_1 == geometry_2)
                return true;

            if (geometry_1 == null || geometry_2 == null)
                return false;

            Coordinate[] coordinates = null;

            coordinates = geometry_1.Coordinates;
            foreach (Coordinate coordinate in coordinates)
                if (geometry_2.Distance(new Point(coordinate)) > tolerance)
                    return false;

            coordinates = geometry_2.Coordinates;
            foreach (Coordinate coordinate in coordinates)
                if (geometry_1.Distance(new Point(coordinate)) > tolerance)
                    return false;

            return true;
        }
    }
}