using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Polygon2D ToSAM(this LinearRing linearRing, double tolerance = Core.Tolerance.Distance)
        {
            if (linearRing == null || linearRing.IsEmpty)
                return null;

            List<Point2D> point2Ds = linearRing.Coordinates.ToSAM(tolerance);
            if (point2Ds == null || point2Ds.Count < 3)
                return null;
            
            if(tolerance != 0)
            {
                List<Point2D> point2Ds_Temp = new List<Point2D>();
                point2Ds_Temp.Add(point2Ds[0]);
                for (int i = 1; i < point2Ds.Count; i++)
                {
                    if (!point2Ds[i].AlmostEquals(point2Ds_Temp.Last()))
                    {
                        point2Ds_Temp.Add(point2Ds[i]);
                    }
                }


                if (point2Ds_Temp.Count < 3)
                    return null;

                point2Ds = point2Ds_Temp;
            }

            Polygon2D result = new Polygon2D(point2Ds);

            //List<Segment2D> segment2Ds = result.GetSegments();
            //foreach(Point2D point2D in point2Ds)
            //{
            //    Segment2D segment2D_Temp = segment2Ds.Find(x => x.On(point2D, tolerance) && x[0].Distance(point2D) >= tolerance && x[1].Distance(point2D) >= tolerance);
            //    if(segment2D_Temp != null)
            //    {
            //        segment2Ds = segment2Ds.Split(tolerance);
            //        List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds);
            //        if(polygon2Ds != null && polygon2Ds.Count != 0)
            //        {
            //            if (polygon2Ds.Count > 1)
            //                polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            //            result = polygon2Ds[0];
            //        }
                    
            //        break;
            //    }
            //}

            return result;
        }
    }
}