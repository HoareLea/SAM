using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Face2D> ToSAM(this MultiPolygon multiPolygon, double tolerance = Core.Tolerance.Distance)
        {
            if (multiPolygon == null || multiPolygon.IsEmpty)
                return null;

            List<Face2D> result = new List<Face2D>();
            foreach (Polygon polygon in multiPolygon)
            {
                Face2D face2D = polygon?.ToSAM(tolerance);
                if (face2D == null)
                    continue;

                result.Add(face2D);
            }

            return result;
        }
    }
}