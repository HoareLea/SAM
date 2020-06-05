using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Intersection of the sets A and B, denoted A ∩ B, is the set of all objects that are members of both A and B. The intersection of {1, 2, 3} and {2, 3, 4} is the set {2, 3}

        public static List<Polygon2D> Intersection(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
            List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_2, tolerance);

            Clipper clipper = new Clipper();
            clipper.AddPath(intPoints_1, PolyType.ptSubject, true);
            clipper.AddPath(intPoints_2, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctIntersection, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints in intPointsList)
                result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

            return result;
        }

        public static Segment2D Intersection(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segment2D_1 == null || segment2D_2 == null)
                return null;

            if (!Collinear(segment2D_1, segment2D_2))
                return null;

            bool on_1 = segment2D_1.On(segment2D_2[0], tolerance);
            bool on_2 = segment2D_1.On(segment2D_2[1], tolerance);
            bool on_3 = segment2D_2.On(segment2D_1[0], tolerance);
            bool on_4 = segment2D_2.On(segment2D_1[1], tolerance);

            if (!on_1 && !on_2 && !on_3 && !on_4)
                return null;

            List<Point2D> point2Ds = new List<Point2D>() { segment2D_1[0], segment2D_1[1], segment2D_2[0], segment2D_2[1] };

            Point2D point2D_1;
            Point2D point2D_2;
            ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
                return null;

            Modify.SortByDistance(point2Ds, point2D_1);

            for (int i = 0; i < point2Ds.Count - 1; i++)
            {
                Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);
                if (segment2D_1.On(point2D_Mid) && segment2D_2.On(point2D_Mid) && point2Ds[i].Distance(point2Ds[i + 1]) > tolerance)
                    return new Segment2D(point2Ds[i], point2Ds[i + 1]);
            }

            return null;
        }

        public static List<Face2D> Intersection(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            Polygon polygon_1 = face2D_1?.ToNTS(tolerance);
            if (polygon_1 == null)
                return null;

            Polygon polygon_2 = face2D_2?.ToNTS(tolerance);
            if (polygon_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry geometry = polygon_1.Intersection(polygon_2);
            if (geometry == null)
                return null;

            if (geometry.IsEmpty)
                return null;

            if (geometry is MultiPolygon)
                return ((MultiPolygon)geometry).ToSAM();

            if (geometry is Polygon)
            {
                Face2D face2D = ((Polygon)geometry).ToSAM();
                if (face2D != null)
                    return new List<Face2D>() { face2D };
            }

            return null;
        }

        public static List<Segment2D> Intersection(this Face2D face2D, Segment2D segment2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Polygon polygon = face2D?.ToNTS(tolerance);
            if (polygon == null)
                return null;

            LineString lineString = segment2D?.ToNTS(tolerance);
            if (lineString == null)
                return null;

            NetTopologySuite.Geometries.Geometry geometry = polygon.Intersection(lineString);
            if (geometry == null)
                return null;

            if (geometry is LineString)
                return new List<Segment2D>(((LineString)geometry).ToSAM().GetSegments());

            if (geometry is MultiLineString)
            {
                List<Segment2D> result = new List<Segment2D>();
                ((MultiLineString)geometry).ToSAM().ForEach(x => result.AddRange(x));
                return result;
            }

            return null;
        }
    }
}