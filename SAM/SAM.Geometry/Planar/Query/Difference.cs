using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Difference of U and A, denoted U \ A, is the set of all members of U that are not members of A. The set difference {1, 2, 3} \ {2, 3, 4} is {1} , while, conversely, the set difference

        public static List<Polygon2D> Difference(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
            List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_2, tolerance);

            Clipper clipper = new Clipper();
            clipper.AddPath(intPoints_1, PolyType.ptSubject, true);
            clipper.AddPath(intPoints_2, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctDifference, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints in intPointsList)
                result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

            return result;
        }

        public static List<Polygon2D> Difference(this Polygon2D polygon2D, IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null || polygon2Ds == null)
                return null;

            if (polygon2Ds.Count() == 0)
                return new List<Polygon2D>() { new Polygon2D(polygon2D) };

            List<IntPoint> intPoints = Convert.ToClipper((ISegmentable2D)polygon2D, tolerance);

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();
            foreach (Polygon2D polygon2D_Temp in polygon2Ds)
                intPointsList.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

            Clipper clipper = new Clipper();
            clipper.AddPath(intPoints, PolyType.ptSubject, true);
            clipper.AddPaths(intPointsList, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList_Result = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctDifference, intPointsList_Result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList_Result == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList_Result.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints_Result in intPointsList_Result)
                result.Add(new Polygon2D(intPoints_Result.ToSAM(tolerance)));

            return result;
        }

        public static List<Polygon2D> Difference(this IEnumerable<Polygon2D> polygon2Ds_1, IEnumerable<Polygon2D> polygon2Ds_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2Ds_1 == null || polygon2Ds_2 == null)
                return null;

            List<List<IntPoint>> intPointsList_1 = new List<List<IntPoint>>();
            foreach (Polygon2D polygon2D_Temp in polygon2Ds_1)
                intPointsList_1.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

            List<List<IntPoint>> intPointsList_2 = new List<List<IntPoint>>();
            foreach (Polygon2D polygon2D_Temp in polygon2Ds_2)
                intPointsList_2.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

            Clipper clipper = new Clipper();
            clipper.AddPaths(intPointsList_1, PolyType.ptSubject, true);
            clipper.AddPaths(intPointsList_2, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList_Result = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctDifference, intPointsList_Result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList_Result == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList_Result.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints_Result in intPointsList_Result)
                result.Add(new Polygon2D(intPoints_Result.ToSAM(tolerance)));

            return result;
        }

        public static List<Segment2D> Difference(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D_1 == null || segment2D_2 == null)
                return null;

            if (!Collinear(segment2D_1, segment2D_2))
                return new List<Segment2D>() { segment2D_1 };

            bool on_1 = segment2D_1.On(segment2D_2[0], tolerance);
            bool on_2 = segment2D_1.On(segment2D_2[1], tolerance);

            if (!on_1 && !on_2)
                return new List<Segment2D>() { segment2D_1 };

            List<Segment2D> result = new List<Segment2D>();

            List<Point2D> point2Ds = new List<Point2D>() { segment2D_1[0], segment2D_1[1], segment2D_2[0], segment2D_2[1] };
            Point2D point2D_1;
            Point2D point2D_2;
            ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            Modify.SortByDistance(point2Ds, point2D_1);

            if (on_1 && on_2)
            {
                if (point2Ds[0].Distance(point2Ds[1]) > tolerance)
                    result.Add(new Segment2D(point2Ds[0], point2Ds[1]));

                if (point2Ds[2].Distance(point2Ds[3]) > tolerance)
                    result.Add(new Segment2D(point2Ds[2], point2Ds[3]));
            }
            else
            {
                if (point2Ds[0].Equals(segment2D_2[0]) || point2Ds[0].Equals(segment2D_2[1]))
                {
                    if (point2Ds[2].Distance(point2Ds[3]) > tolerance)
                        result.Add(new Segment2D(point2Ds[2], point2Ds[3]));
                }
                else
                {
                    if (point2Ds[0].Distance(point2Ds[1]) > tolerance)
                        result.Add(new Segment2D(point2Ds[0], point2Ds[1]));
                }
            }

            return result;
        }

        public static List<Face2D> Difference(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D_1 == null || face2D_2 == null)
                return null;
            
            Polygon polygon_1 = face2D_1.ToNTS(tolerance);
            Polygon polygon_2 = face2D_2.ToNTS(tolerance);

            if (polygon_1 == null || polygon_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry geometry = polygon_1.Difference(polygon_2);

            List<Face2D> result = new List<Face2D>();
            if (geometry is Polygon)
            {
                Face2D face2D = ((Polygon)geometry).ToSAM(tolerance);
                if (face2D != null)
                    result.Add(face2D);
            }
            else if (geometry is MultiPolygon)
            {
                List<Polygon> polygons = Query.Polygons((MultiPolygon)geometry);
                if(polygons != null && polygons.Count > 0)
                {
                    polygons.ForEach(x => result.Add(x.ToSAM(tolerance)));
                }
            }
            else if (geometry is LinearRing)
            {
                Polygon2D polygon2D = ((LinearRing)geometry).ToSAM(tolerance);
                if (polygon2D != null)
                    result.Add(new Face2D(polygon2D));
            }

            return result;
        }
    }
}