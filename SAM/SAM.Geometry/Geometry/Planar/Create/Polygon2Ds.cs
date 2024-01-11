using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face2D> face2Ds = Face2Ds(segmentable2Ds, EdgeOrientationMethod.Undefined, tolerance);
            if(face2Ds == null)
            {
                return null;
            }

            List<Tuple<Polygon2D, BoundingBox2D, double>> tuples = new List<Tuple<Polygon2D, BoundingBox2D, double>>();
            foreach (Face2D face2D in face2Ds)
            {
                Polygon2D polygon2D = face2D?.ExternalEdge2D as Polygon2D;
                if(polygon2D == null)
                {
                    continue;
                }

                BoundingBox2D boundingBox2D = polygon2D?.GetBoundingBox();
                if (boundingBox2D == null)
                {
                    continue;
                }

                double area = polygon2D.GetArea();
                if (double.IsNaN(area) || area < tolerance)
                {
                    continue;
                }

                List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_Similar = tuples.FindAll(x => System.Math.Abs(x.Item3 - area) <= tolerance);
                if(tuples_Similar != null && tuples_Similar.Count != 0)
                {
                    tuples_Similar = tuples_Similar.FindAll(x => boundingBox2D.InRange(x.Item2, tolerance));
                    if (tuples_Similar != null && tuples_Similar.Count != 0)
                    {
                        Tuple<Polygon2D, BoundingBox2D, double> tuple = tuples_Similar.Find(x => x.Item1.Similar(polygon2D, tolerance));
                        if (tuple != null)
                        {
                            continue;
                        }
                    }
                }
                
                tuples.Add(new Tuple<Polygon2D, BoundingBox2D, double>(polygon2D, boundingBox2D, area));
            }

            foreach(Face2D face2D in face2Ds)
            {
                IEnumerable<Polygon2D> polygon2Ds = face2D?.InternalEdge2Ds?.Cast<Polygon2D>();
                if(polygon2Ds == null || polygon2Ds.Count() == 0)
                {
                    continue;
                }

                foreach(Polygon2D polygon2D in polygon2Ds)
                {
                    BoundingBox2D boundingBox2D = polygon2D?.GetBoundingBox();
                    if (boundingBox2D == null)
                    {
                        continue;
                    }

                    double area = polygon2D.GetArea();
                    if (double.IsNaN(area) || area < tolerance)
                    {
                        continue;
                    }

                    List<Polygon2D> polygon2Ds_Temp = new List<Polygon2D>() { polygon2D };

                    List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_Internal = tuples.FindAll(x => area + tolerance > x.Item3);
                    if(tuples_Internal != null && tuples_Internal.Count != 0)
                    {
                        tuples_Internal = tuples_Internal.FindAll(x => boundingBox2D.Inside(x.Item2.GetCentroid(), tolerance));
                        if (tuples_Internal != null && tuples_Internal.Count != 0)
                        {
                            polygon2Ds_Temp = Query.Difference(polygon2D, tuples_Internal.ConvertAll(x => x.Item1));
                        }
                    }

                    if(polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach(Polygon2D polygon2D_Temp in polygon2Ds_Temp)
                    {
                        if(polygon2D_Temp != polygon2D)
                        {
                            boundingBox2D = polygon2D_Temp?.GetBoundingBox();
                            if (boundingBox2D == null)
                            {
                                continue;
                            }

                            area = polygon2D_Temp.GetArea();
                            if (double.IsNaN(area) || area < tolerance)
                            {
                                continue;
                            }
                        }

                        tuples.Add(new Tuple<Polygon2D, BoundingBox2D, double>(polygon2D_Temp, boundingBox2D, area));
                    }
                }

            }

            return tuples.ConvertAll(x => x.Item1);
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, bool split, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            if (split)
                return Polygon2Ds(segmentable2Ds.Split(tolerance), tolerance);

            return Polygon2Ds(segmentable2Ds, tolerance);
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, bool unconnectedOnly = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = Segment2Ds(segmentable2Ds, maxDistance, unconnectedOnly, tolerance);
            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            segment2Ds = segment2Ds.Snap(true, tolerance);

            return Polygon2Ds(segment2Ds, tolerance);
        }
    }
}