using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Polygon2D> Cut(this Polygon2D polygon2D_ToBeCut, Polygon2D polygon)
        {
            if (polygon2D_ToBeCut == null || polygon == null)
                return null;

            List<Polygon2D> polygon2Ds = new PointGraph2D(new Polygon2D[] { polygon2D_ToBeCut, polygon }, true).GetPolygon2Ds();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            foreach(Polygon2D polygon2D_Temp in polygon2Ds)
            {
                if (!polygon.Inside(polygon2D_Temp.GetInternalPoint2D()))
                    result.Add(polygon2D_Temp);
            }

            return result;
        }

        public static List<Polyline2D> Cut(this Polyline2D polyline2D, Point2D point2D_1, Point2D point2D_2)
        {
            if (polyline2D == null || point2D_1 == null || point2D_2 == null)
                return null;

            int index_1 = Query.IndexOfClosest(polyline2D, point2D_1);
            if (index_1 == -1)
                return null;

            int index_2 = Query.IndexOfClosest(polyline2D, point2D_2);
            if (index_2 == -1)
                return null;

            List<Segment2D> segment2Ds = polyline2D.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;            
            
            if (index_1 > index_2)
            {
                int index = index_1;
                index_1 = index_2;
                index_2 = index;

                Point2D point2D = point2D_1;
                point2D_1 = point2D_2;
                point2D_2 = point2D;
            }
            else if(index_1 == index_2)
            {
                if(point2D_1.Distance(segment2Ds[index_1].Start) > point2D_2.Distance(segment2Ds[index_1].Start))
                {
                    Point2D point2D = point2D_1;
                    point2D_1 = point2D_2;
                    point2D_2 = point2D;
                }
            }

            List<Polyline2D> polyline2Ds = new List<Polyline2D>();

            List<Segment2D> segment2Ds_Temp = null;

            segment2Ds_Temp = new List<Segment2D>();
            for (int i = 0; i < index_1; i++)
                segment2Ds_Temp.Add(segment2Ds[i]);

            segment2Ds_Temp.Add(new Segment2D(segment2Ds[index_1].Start, point2D_1));

            polyline2Ds.Add(new Polyline2D(segment2Ds_Temp));

            segment2Ds_Temp = new List<Segment2D>();
            segment2Ds_Temp.Add(new Segment2D(point2D_2, segment2Ds[index_2].End));
            for (int i = index_2 + 1; i < segment2Ds.Count; i++)
                segment2Ds_Temp.Add(segment2Ds[i]);

            polyline2Ds.Add(new Polyline2D(segment2Ds_Temp));

            return polyline2Ds;
        }
    }
}
