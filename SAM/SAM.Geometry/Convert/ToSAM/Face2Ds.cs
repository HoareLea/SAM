using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Face2D ToSAM(this Polygon polygon)
        {
            if (polygon == null)
                return null;

            LinearRing linearRing = polygon.ExteriorRing as LinearRing;
            if (linearRing == null)
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

                    polygon2Ds.Add(linearRing_Temp.ToSAM());
                }
            }

            return Face2D.Create(linearRing.ToSAM(), polygon2Ds, true);
        }
    }
}