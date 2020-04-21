using System.Collections.Generic;
using System.Linq;
using ClipperLib;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Difference of U and A, denoted U \ A, is the set of all members of U that are not members of A. The set difference {1, 2, 3} \ {2, 3, 4} is {1} , while, conversely, the set difference
        
        private static List<Polygon2D> Difference(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (tolerance == 0)
                return Difference(polygon2D_1, polygon2D_2);

            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
            List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);

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

        private static List<Polygon2D> Difference(this Polygon2D polygon2D, IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null || polygon2Ds == null)
                return null;

            if (polygon2Ds.Count() == 0)
                return new List<Polygon2D>() { new Polygon2D(polygon2D)};

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

        private static List<Polygon2D> Difference(this Polygon2D polygon2D_1, Polygon2D polygon2D_2)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();

            List<Polygon2D> polygon2Ds = new PointGraph2D(new List<Polygon2D>() { polygon2D_1, polygon2D_2 }, true).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return result;

            BoundingBox2D boundingBox2D_1 = polygon2D_1.GetBoundingBox();
            BoundingBox2D boundingBox2D_2 = polygon2D_1.GetBoundingBox();

            foreach (Polygon2D polygon2D in polygon2Ds)
            {
                Point2D point2D = polygon2D.GetInternalPoint2D();

                if (!boundingBox2D_1.Inside(point2D) || !boundingBox2D_2.Inside(point2D))
                {
                    result.Add(polygon2D);
                    continue;
                }

                if (polygon2D_1.Inside(point2D) && polygon2D_2.Inside(point2D))
                    continue;

                result.Add(polygon2D);
            }

            return result;
        }
    }
}
