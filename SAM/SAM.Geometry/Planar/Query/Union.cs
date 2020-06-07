using ClipperLib;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Union of the sets A and B, denoted A ∪ B, is the set of all objects that are a member of A, or B, or both. The union of {1, 2, 3} and {2, 3, 4} is the set {1, 2, 3, 4}

        public static List<Polygon2D> Union(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
            List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_2, tolerance);

            Clipper clipper = new Clipper();
            clipper.AddPath(intPoints_1, PolyType.ptSubject, true);
            clipper.AddPath(intPoints_2, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctUnion, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints in intPointsList)
                result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

            return result;
        }

        public static List<Polygon2D> Union(this IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (tolerance == 0)
                return Union(polygon2Ds);

            if (polygon2Ds == null)
                return null;

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

            foreach (Polygon2D polygon in polygon2Ds)
            {
                List<IntPoint> intPoints = Convert.ToClipper((ISegmentable2D)polygon, tolerance);
                if (intPoints != null)
                    intPointsList.Add(intPoints);
            }

            Clipper clipper = new Clipper();
            clipper.AddPaths(intPointsList, PolyType.ptSubject, true);

            intPointsList = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctUnion, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints in intPointsList)
                result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

            return result;
        }

        public static List<Polygon> Union(this IEnumerable<Polygon> polygons)
        {
            if (polygons == null)
                return null;

            List<Polygon> result = new List<Polygon>();
            if (polygons.Count() == 0)
                return result;

            MultiPolygon multiPolygon = new MultiPolygon(polygons.ToArray());
            NetTopologySuite.Geometries.Geometry geometry = multiPolygon.Union();
            if (geometry == null)
                return null;

            if (geometry is MultiPolygon)
                return ((MultiPolygon)geometry).Geometries.Cast<Polygon>().ToList();

            if (geometry is Polygon)
                result.Add((Polygon)geometry);

            return result;
        }

        public static List<Segment2D> Union(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segment2D_1 == null || segment2D_2 == null)
                return null;

            if (!Collinear(segment2D_1, segment2D_2))
                return new List<Segment2D>() { segment2D_1, segment2D_2 };

            bool on_1 = segment2D_1.On(segment2D_2[0], tolerance);
            bool on_2 = segment2D_1.On(segment2D_2[1], tolerance);
            bool on_3 = segment2D_2.On(segment2D_1[0], tolerance);
            bool on_4 = segment2D_2.On(segment2D_1[1], tolerance);

            if (!on_1 && !on_2 && !on_3 && !on_4)
                return new List<Segment2D>() { segment2D_1, segment2D_2};

            Point2D point2D_1;
            Point2D point2D_2;
            ExtremePoints(new List<Point2D>() { segment2D_1[0], segment2D_1[1], segment2D_2[0], segment2D_2[1] }, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
                return null;

            return new List<Segment2D>() { new Segment2D(point2D_1, point2D_2) };
        }

        public static List<Segment2D> Union(this IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            if (segment2Ds.Count() == 0)
                return result;

            if (segment2Ds.Count() == 1)
            {
                result.Add(segment2Ds.First());
                return result;
            }

            List<Segment2D> segment2Ds_Temp = new List<Segment2D>(segment2Ds);

            while(segment2Ds_Temp.Count > 0)
            {
                Segment2D segment2D = segment2Ds_Temp[0];
                segment2Ds_Temp.RemoveAt(0);

                List<Segment2D> segment2Ds_Collinear = segment2Ds_Temp.FindAll(x => x.Collinear(segment2D, tolerance));


            }

            throw new NotImplementedException();
        }

        public static List<Face2D> Union(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            Polygon polygon_1 = face2D_1?.ToNTS(tolerance);
            if (polygon_1 == null)
                return null;

            Polygon polygon_2 = face2D_2?.ToNTS(tolerance);
            if (polygon_2 == null)
                return null;

            NetTopologySuite.Geometries.Geometry geometry = polygon_1.Union(polygon_2);
            if (geometry == null)
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
    }
}