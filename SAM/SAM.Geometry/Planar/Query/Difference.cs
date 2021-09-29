//using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Difference of U and A, denoted U \ A, is the set of all members of U that are not members of A. The set difference {1, 2, 3} \ {2, 3, 4} is {1} , while, conversely, the set difference

        //public static List<Polygon2D> Difference(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (polygon2D_1 == null || polygon2D_2 == null)
        //        return null;

        //    List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
        //    List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_2, tolerance);

        //    Clipper clipper = new Clipper();
        //    clipper.AddPath(intPoints_1, PolyType.ptSubject, true);
        //    clipper.AddPath(intPoints_2, PolyType.ptClip, true);

        //    List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

        //    clipper.Execute(ClipType.ctDifference, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

        //    if (intPointsList == null)
        //        return null;

        //    List<Polygon2D> result = new List<Polygon2D>();
        //    if (intPointsList.Count == 0)
        //        return result;

        //    foreach (List<IntPoint> intPoints in intPointsList)
        //        result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

        //    return result;
        //}

        //public static List<Polygon2D> Difference(this Polygon2D polygon2D, IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (polygon2D == null || polygon2Ds == null)
        //        return null;

        //    if (polygon2Ds.Count() == 0)
        //        return new List<Polygon2D>() { new Polygon2D(polygon2D) };

        //    List<IntPoint> intPoints = Convert.ToClipper((ISegmentable2D)polygon2D, tolerance);

        //    List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();
        //    foreach (Polygon2D polygon2D_Temp in polygon2Ds)
        //        intPointsList.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

        //    Clipper clipper = new Clipper();
        //    clipper.AddPath(intPoints, PolyType.ptSubject, true);
        //    clipper.AddPaths(intPointsList, PolyType.ptClip, true);

        //    List<List<IntPoint>> intPointsList_Result = new List<List<IntPoint>>();

        //    clipper.Execute(ClipType.ctDifference, intPointsList_Result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

        //    if (intPointsList_Result == null)
        //        return null;

        //    List<Polygon2D> result = new List<Polygon2D>();
        //    if (intPointsList_Result.Count == 0)
        //        return result;

        //    foreach (List<IntPoint> intPoints_Result in intPointsList_Result)
        //        result.Add(new Polygon2D(intPoints_Result.ToSAM(tolerance)));

        //    return result;
        //}

        //public static List<Polygon2D> Difference(this IEnumerable<Polygon2D> polygon2Ds_1, IEnumerable<Polygon2D> polygon2Ds_2, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (polygon2Ds_1 == null || polygon2Ds_2 == null)
        //        return null;

        //    List<List<IntPoint>> intPointsList_1 = new List<List<IntPoint>>();
        //    foreach (Polygon2D polygon2D_Temp in polygon2Ds_1)
        //        intPointsList_1.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

        //    List<List<IntPoint>> intPointsList_2 = new List<List<IntPoint>>();
        //    foreach (Polygon2D polygon2D_Temp in polygon2Ds_2)
        //        intPointsList_2.Add(Convert.ToClipper((ISegmentable2D)polygon2D_Temp, tolerance));

        //    Clipper clipper = new Clipper();
        //    clipper.AddPaths(intPointsList_1, PolyType.ptSubject, true);
        //    clipper.AddPaths(intPointsList_2, PolyType.ptClip, true);

        //    List<List<IntPoint>> intPointsList_Result = new List<List<IntPoint>>();

        //    clipper.Execute(ClipType.ctDifference, intPointsList_Result, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

        //    if (intPointsList_Result == null)
        //        return null;

        //    List<Polygon2D> result = new List<Polygon2D>();
        //    if (intPointsList_Result.Count == 0)
        //        return result;

        //    foreach (List<IntPoint> intPoints_Result in intPointsList_Result)
        //        result.Add(new Polygon2D(intPoints_Result.ToSAM(tolerance)));

        //    return result;
        //}

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
            List<Face2D> result = null;
            try
            {
                result = Difference_NTS(face2D_1, face2D_2, tolerance);
            }
            catch(System.Exception exception)
            {

            }

            if(result == null)
            {
                result = Difference_SAM(face2D_1, face2D_2, tolerance);
            }

            return result;
        }

        public static List<Face2D> Difference(this Face2D face2D, Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D == null || polygon2D == null)
                return null;

            return Difference(face2D, new Face2D(polygon2D), tolerance);
        }

        public static List<Polygon2D> Difference(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Difference(polygon2D_1, new Polygon2D[] { polygon2D_2 }, tolerance);
        }

        public static List<Polygon2D> Difference(this Polygon2D polygon2D, IEnumerable<Polygon2D> polygon2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null || polygon2Ds == null)
            {
                return null;
            }

            List<Face2D> face2Ds = Difference(new Face2D(polygon2D), polygon2Ds.ToList().ConvertAll(x => new Face2D(x)), tolerance);
            if (face2Ds == null || face2Ds.Count == 0)
            {
                return null;
            }

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (Face2D face2D in face2Ds)
            {
                List<IClosed2D> edge2Ds = face2D?.Edge2Ds;
                if (edge2Ds == null || edge2Ds.Count == 0)
                {
                    continue;
                }

                foreach (IClosed2D edge2D in edge2Ds)
                {
                    ISegmentable2D segmentable2D = edge2D as ISegmentable2D;
                    if (segmentable2D == null)
                    {
                        continue;
                    }

                    result.Add(new Polygon2D(segmentable2D.GetPoints()));
                }
            }

            return result;
        }

        public static List<Polygon2D> Difference(this IEnumerable<Polygon2D> polygon2Ds_1, IEnumerable<Polygon2D> polygon2Ds_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(polygon2Ds_1 == null || polygon2Ds_2 == null)
            {
                return null;
            }

            if(polygon2Ds_2.Count() == 0)
            {
                return polygon2Ds_1.ToList().ConvertAll(x => new Polygon2D(x));
            }

            int count = polygon2Ds_1.Count();

            if (count == 0)
            {
                return new List<Polygon2D>();
            }

            if(count == 1)
            {
                return Difference(polygon2Ds_1.ElementAt(0), polygon2Ds_2, tolerance);
            }

            List<Polygon2D> result = new List<Polygon2D>();

            if (count < 10 && polygon2Ds_2.Count() < 10)
            {
                foreach(Polygon2D polygon2D in polygon2Ds_1)
                {
                    List<Polygon2D> polygon2Ds = polygon2D?.Difference(polygon2Ds_2, tolerance);
                    if(polygon2Ds != null)
                    {
                        result.AddRange(polygon2Ds);
                    }
                }
            }
            else
            {
                List<List<Polygon2D>> polygon2DsList = Enumerable.Repeat<List<Polygon2D>>(null, count).ToList();

                Parallel.For(0, count, (int i) => 
                {
                    polygon2DsList[i] = polygon2Ds_1.ElementAt(0)?.Difference(polygon2Ds_2, tolerance);
                });

                foreach(List<Polygon2D> polygon2Ds in polygon2DsList)
                {
                    if(polygon2Ds == null || polygon2Ds.Count == 0)
                    {
                        continue;
                    }

                    result.AddRange(polygon2Ds);
                }
            }

            return result;
        }

        public static List<Face2D> Difference(this Face2D face2D, IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D == null || face2Ds == null || face2D.GetArea() <= tolerance)
                return null;

            int count = face2Ds.Count();
            if (count == 1)
            {
                return Difference(face2D, face2Ds.ElementAt(0), tolerance);
            }

            List<Face2D> result = new List<Face2D>() { face2D };
            if (count == 0)
            {
                return result;
            }
            
            foreach(Face2D face2D_Temp in face2Ds)
            {
                if (face2D_Temp.GetArea() <= tolerance)
                    continue;
                
                List<Face2D> face2Ds_Temp = new List<Face2D>();
                foreach (Face2D face2D_Result in result)
                {
                    List<Face2D> face2Ds_Difference = Difference(face2D_Result, face2D_Temp, tolerance);
                    if (face2Ds_Difference != null && face2Ds_Difference.Count != 0)
                    {
                        foreach (Face2D face2D_Difference in face2Ds_Difference)
                        {
                            if (face2D_Difference != null && face2D_Difference.GetArea() > tolerance)
                                face2Ds_Temp.Add(face2D_Difference);
                        }
                    }
                }

                result = face2Ds_Temp;

                if (result == null || result.Count == 0)
                    break;
            }

            return result;
        }

        
        private static List<Face2D> Difference_NTS(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D_1 == null || face2D_2 == null)
                return null;

            double area_1 = face2D_1.GetArea();
            if (area_1 <= tolerance)
                return null;

            double area_2 = face2D_2.GetArea();
            if (area_2 <= tolerance)
                return new List<Face2D>() { new Face2D(face2D_2) };

            List<Face2D> face2Ds_1 = face2D_1.FixEdges(tolerance);
            if (face2Ds_1 == null || face2Ds_1.Count == 0)
            {
                face2Ds_1 = new List<Face2D>() { face2D_1 };
            }

            List<Face2D> face2Ds_2 = face2D_2.FixEdges(tolerance);
            if (face2Ds_2 == null || face2Ds_2.Count == 0)
            {
                face2Ds_2 = new List<Face2D>() { face2D_2 };
            }

            List<Face2D> result = null;

            if (face2Ds_1.Count != 1 || face2Ds_2.Count != 1)
            {
                result = new List<Face2D>();
                foreach (Face2D face2D in face2Ds_1)
                {
                    List<Face2D> face2Ds_Difference = face2D.Difference(face2Ds_2, tolerance);
                    if (face2Ds_Difference != null && face2Ds_Difference.Count != 0)
                    {
                        result.AddRange(face2Ds_Difference);
                    }
                }

                return result;
            }

            Polygon polygon_1 = face2Ds_1[0].ToNTS(tolerance);
            Polygon polygon_2 = face2Ds_2[0].ToNTS(tolerance);

            if (polygon_1 == null || polygon_2 == null)
                return result;

            result = new List<Face2D>();

            //test to check  NaN when createing shells from adj cluster
            //Find better way to determine EqualsTopologically for polygons which gives exception
            try
            {
                if (polygon_1.EqualsTopologically(polygon_2))
                    return result;
            }
            catch (System.Exception exception)
            {

            }

            List<Polygon> polygons_Snap = Snap(polygon_1, polygon_2, tolerance);
            if (polygons_Snap != null && polygons_Snap.Count == 2)
            {
                polygon_1 = polygons_Snap[0];
                polygon_2 = polygons_Snap[1];
            }

            //Find better way to determine EqualsTopologically for polygons which gives exception
            try
            {
                if (polygon_1.EqualsTopologically(polygon_2))
                    return result;
            }
            catch (System.Exception exception)
            {

            }

            NetTopologySuite.Geometries.Geometry geometry = polygon_1.Difference(polygon_2);
            if (geometry == null || geometry.IsEmpty)
            {
                return null;
            }

            List<NetTopologySuite.Geometries.Geometry> geometries = geometry is GeometryCollection ? ((GeometryCollection)geometry).Geometries?.ToList() : new List<NetTopologySuite.Geometries.Geometry>() { geometry };
            if (geometries == null || geometries.Count == 0)
            {
                return null;
            }

            foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
            {
                if (geometry_Temp is Polygon)
                {
                    Face2D face2D = ((Polygon)geometry_Temp).ToSAM(tolerance);
                    if (face2D != null)
                        result.Add(face2D);
                }
                else if (geometry_Temp is MultiPolygon)
                {
                    List<Polygon> polygons = Polygons((MultiPolygon)geometry_Temp);
                    if (polygons != null && polygons.Count > 0)
                    {
                        polygons.ForEach(x => result.Add(x.ToSAM(tolerance)));
                    }
                }
                else if (geometry_Temp is LinearRing)
                {
                    Polygon2D polygon2D = ((LinearRing)geometry_Temp).ToSAM(tolerance);
                    if (polygon2D != null)
                        result.Add(new Face2D(polygon2D));
                }
            }

            return result;
        }

        private static List<Face2D> Difference_SAM(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(face2D_1 == null || face2D_2 == null)
            {
                return null;
            }

            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();

            List<IClosed2D> edge2Ds_Temp = null;

            edge2Ds_Temp = face2D_1.Edge2Ds;
            if(edge2Ds_Temp == null)
            {
                return null;
            }

            if(!edge2Ds_Temp.TrueForAll(x => x is ISegmentable2D))
            {
                throw new System.NotImplementedException();
            }

            segmentable2Ds.AddRange(edge2Ds_Temp.ConvertAll(x => (ISegmentable2D)x));

            edge2Ds_Temp = face2D_2.Edge2Ds;
            if (edge2Ds_Temp == null)
            {
                return null;
            }

            if (!edge2Ds_Temp.TrueForAll(x => x is ISegmentable2D))
            {
                throw new System.NotImplementedException();
            }

            segmentable2Ds.AddRange(edge2Ds_Temp.ConvertAll(x => (ISegmentable2D)x));
            if(segmentable2Ds == null || segmentable2Ds.Count == 0)
            {
                return null;
            }

            List<Segment2D> segment2Ds = Split(segmentable2Ds, tolerance);
            segment2Ds = Snap(segment2Ds, true, tolerance);

            List<Polygon2D> polygon2Ds_All = segment2Ds.Polygon2Ds(tolerance);
            if(polygon2Ds_All == null || polygon2Ds_All.Count == 0)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            foreach(Polygon2D polygon2D in polygon2Ds_All)
            {
                Point2D point2D = polygon2D?.GetInternalPoint2D(tolerance);
                if(point2D == null)
                {
                    continue;
                }

                if (polygon2D.GetArea() < tolerance)
                {
                    continue;
                }

                if(face2D_2.Inside(point2D, tolerance))
                {
                    continue;
                }

                polygon2Ds.Add(polygon2D);
            }

            if(polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return new List<Face2D>();
            }

            return Create.Face2Ds(polygon2Ds, tolerance);
        }
    }
}