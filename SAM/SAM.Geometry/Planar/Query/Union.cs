using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Union of the sets A and B, denoted A ∪ B, is the set of all objects that are a member of A, or B, or both. The union of {1, 2, 3} and {2, 3, 4} is the set {1, 2, 3, 4}

        public static List<Polygon2D> Union(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (tolerance == 0)
                return Union(polygon2D_1, polygon2D_2);

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
                return ((MultiPolygon)geometry).Cast<Polygon>().ToList();

            if (geometry is Polygon)
                result.Add((Polygon)geometry);

            return result;
        }

        
        private static List<Polygon2D> Union(this Polygon2D polygon2D_1, Polygon2D polygon2D_2)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            return new PointGraph2D(new List<Polygon2D>() { polygon2D_1, polygon2D_2 }, true).GetPolygon2Ds_External();
        }

        private static List<Polygon2D> Union(this IEnumerable<Polygon2D> polygon2Ds)
        {
            if (polygon2Ds == null)
                return null;

            int count = polygon2Ds.Count();

            List<Polygon2D> result = new List<Polygon2D>();

            if (count == 0)
                return result;

            if (count == 1)
            {
                result.Add(new Polygon2D(polygon2Ds.First()));
                return result;
            }

            return new PointGraph2D(polygon2Ds, true).GetPolygon2Ds_External();
        }
        
        private static List<Face2D> Union(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
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