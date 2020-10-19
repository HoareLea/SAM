using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (Polygon polygon in polygons)
            {
                List<Polygon2D> polygon2Ds = polygon.ToSAM_Polygon2Ds();
                if (polygon2Ds == null)
                    continue;

                //result.AddRange(polygon2Ds);

                //Removing duplicated polygon2Ds
                foreach (Polygon2D polygon2D in polygon2Ds)
                    if (result.Find(x => x.Similar(polygon2D)) == null)
                        result.AddRange(polygon2Ds);
            }

            
            for(int i =0; i < result.Count; i++)
            {
                List<Point2D> point2Ds = result[i]?.Points;
                if (point2Ds == null || point2Ds.Count == 0)
                    continue;

                for(int j=0; j < point2Ds.Count; j++)
                {
                    Point2D point2D = point2Ds[j];

                    Segment2D segment2D = null;

                    segment2D = segment2Ds.Find(x => x[0].AlmostEquals(point2D, tolerance));
                    if(segment2D != null)
                    {
                        point2Ds[j] = segment2D[0];
                        continue;
                    }

                    segment2D = segment2Ds.Find(x => x[1].AlmostEquals(point2D, tolerance));
                    if (segment2D != null)
                    {
                        point2Ds[j] = segment2D[1];
                        continue;
                    }

                    double distance = double.MaxValue;
                    foreach(Segment2D segment2D_Temp in segment2Ds)
                    {
                        Point2D point2D_Temp = segment2D_Temp.Closest(point2D);
                        double distance_Temp = point2D_Temp.Distance(point2D);
                        if(distance_Temp < distance)
                        {
                            point2Ds[j] = point2D_Temp;
                            distance = distance_Temp;
                        }

                    }
                }

                result[i] = new Polygon2D(point2Ds);
            }

            return result;
        }
    }
}