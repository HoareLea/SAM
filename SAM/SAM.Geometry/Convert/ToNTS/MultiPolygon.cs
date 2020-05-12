using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static MultiPolygon ToNTS(this IEnumerable<Face2D> face2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2Ds == null || face2Ds.Count() == 0)
                return null;

            List<Polygon> polygons = new List<Polygon>();
            foreach(Face2D face2D in face2Ds)
            {
                if (face2D == null)
                    continue;

                Polygon polygon = face2D.ToNTS(tolerance);
                if (polygon == null)
                    continue;

                polygons.Add(polygon);
            }

            return new MultiPolygon(polygons.ToArray());

        }
    }
}