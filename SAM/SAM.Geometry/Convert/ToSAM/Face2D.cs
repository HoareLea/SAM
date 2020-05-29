using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Face2D ToSAM(this Polygon polygon, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon == null || polygon.IsEmpty)
                return null;

            LinearRing linearRing = polygon.ExteriorRing as LinearRing;
            if (linearRing == null)
                return null;

            Polygon2D polygon2D = linearRing.ToSAM(tolerance);
            if (polygon2D == null || polygon2D.GetArea() < tolerance)
                return null;

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();

            LineString[] lineStrings = polygon.InteriorRings;
            if (lineStrings != null && lineStrings.Length > 0)
            {
                foreach (LineString lineString in lineStrings)
                {
                    LinearRing linearRing_Temp = lineString as LinearRing;
                    if (linearRing_Temp == null)
                        continue;

                    Polygon2D polygon2D_Temp = linearRing_Temp.ToSAM(tolerance);
                    if (polygon2D_Temp == null || polygon2D_Temp.GetArea() < tolerance)
                        continue;

                    polygon2Ds.Add(polygon2D_Temp);
                }
            }

            return Face2D.Create(polygon2D, polygon2Ds, true);
        }

        public static Face2D ToSAM(this Polygon polygon, double minArea, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon == null || polygon.IsEmpty)
                return null;

            LinearRing linearRing = polygon.ExteriorRing as LinearRing;
            if (linearRing == null)
                return null;

            Polygon2D polygon2D = linearRing.ToSAM(tolerance);
            if (polygon2D == null || polygon2D.GetArea() < minArea)
                return null;

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();

            LineString[] lineStrings = polygon.InteriorRings;
            if (lineStrings != null && lineStrings.Length > 0)
            {
                foreach (LineString lineString in lineStrings)
                {
                    LinearRing linearRing_Temp = lineString as LinearRing;
                    if (linearRing_Temp == null)
                        continue;

                    Polygon2D polygon2D_Temp = linearRing_Temp.ToSAM(tolerance);
                    if (polygon2D_Temp == null || polygon2D_Temp.GetArea() < minArea)
                        continue;

                    polygon2Ds.Add(polygon2D_Temp);
                }
            }

            return Face2D.Create(polygon2D, polygon2Ds, true);
        }
    }
}