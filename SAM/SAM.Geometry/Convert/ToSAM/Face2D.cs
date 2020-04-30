using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Face2D> ToSAM(this MultiPolygon multiPolygon)
        {
            if (multiPolygon == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            foreach(Polygon polygon in multiPolygon)
            {
                Face2D face2D = polygon?.ToSAM();
                if (face2D == null)
                    continue;

                result.Add(face2D);
            }

            return result;

        }
    }
}
