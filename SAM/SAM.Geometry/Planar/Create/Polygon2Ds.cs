using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Face2D> face2Ds = Face2Ds(segmentable2Ds, tolerance);
            if(face2Ds == null)
            {
                return null;
            }

            List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_All = new List<Tuple<Polygon2D, BoundingBox2D, double>>();
            foreach (Face2D face2D in face2Ds)
            {
                foreach(Polygon2D polygon2D in face2D.Edge2Ds)
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

                    tuples_All.Add(new Tuple<Polygon2D, BoundingBox2D, double>(polygon2D, boundingBox2D, area));
                }
            }
            
            //if (segmentable2Ds == null)
            //    return null;

            //List<Segment2D> segment2Ds = new List<Segment2D>();
            //foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            //{
            //    if (segmentable2D == null)
            //        continue;

            //    List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
            //    if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
            //        segment2Ds.AddRange(segment2Ds_Temp);
            //}

            //List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            //if (polygons == null)
            //    return null;

            //List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_All = new List<Tuple<Polygon2D, BoundingBox2D, double>>();
            //foreach (Polygon polygon in polygons)
            //{
            //    List<Polygon2D> polygon2Ds = polygon.ToSAM_Polygon2Ds();
            //    if (polygon2Ds == null)
            //    {
            //        continue;
            //    }

            //    foreach(Polygon2D polygon2D in polygon2Ds)
            //    {
            //        BoundingBox2D boundingBox2D = polygon2D?.GetBoundingBox();
            //        if(boundingBox2D == null)
            //        {
            //            continue;
            //        }

            //        double area = polygon2D.GetArea();
            //        if(double.IsNaN(area) || area < tolerance)
            //        {
            //            continue;
            //        }

            //        tuples_All.Add(new Tuple<Polygon2D, BoundingBox2D, double>(polygon2D, boundingBox2D, area));
            //    }
            //}

            //tuples_All.Sort((x, y) => x.Item3.CompareTo(y.Item3));

            List<Tuple<Polygon2D, BoundingBox2D, double>> tuples = new List<Tuple<Polygon2D, BoundingBox2D, double>>();
            while (tuples_All.Count > 0)
            {
                Tuple<Polygon2D, BoundingBox2D, double> tuple = tuples_All[0];
                tuples_All.RemoveAt(0);

                List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_Area = tuples.FindAll(x => System.Math.Abs(x.Item3 - tuple.Item3) <= tolerance);
                if(tuples_Area != null && tuples_Area.Count > 0)
                {
                    Tuple<Polygon2D, BoundingBox2D, double> tuple_Area = tuples_Area.Find(x => tuple.Item2.InRange(x.Item2, tolerance) && x.Item1.Similar(tuple.Item1, tolerance));
                    if (tuple_Area != null)
                    {
                        continue;
                    }
                }

                //List<Tuple<Polygon2D, BoundingBox2D, double>> tuples_Inside = tuples.FindAll(x => tuple.Item2.InRange(x.Item2, tolerance));
                //if(tuples_Inside != null && tuples_Inside.Count > 0)
                //{
                //    tuples_Inside = tuples_Inside.FindAll(x => tuple.Item1.Inside(x.Item1, tolerance));
                //    if (tuples_Inside != null && tuples_Inside.Count > 0)
                //    {
                //        List<Polygon2D> polygon2Ds = tuples_Inside.ConvertAll(x => x.Item1).Union(tolerance);
                //        if(polygon2Ds != null && polygon2Ds.Count != 0)
                //        {
                //            double area = polygon2Ds.ConvertAll(x => x.GetArea()).Sum();
                //            if(System.Math.Abs(tuple.Item3 - area) <= tolerance)
                //            {
                //                continue;
                //            }
                //        }
                //    }

                //}

                tuples.Add(tuple);
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

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = Segment2Ds(segmentable2Ds, maxDistance, tolerance);
            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            segment2Ds = segment2Ds.Snap(true, tolerance);

            return Polygon2Ds(segment2Ds, tolerance);
        }
    }
}