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

            IEnumerable<NetTopologySuite.Geometries.Geometry> geometries = multiPolygon.Geometries;
            if (geometries == null)
                return null;

            List<Face2D> result = new List<Face2D>();
            foreach (NetTopologySuite.Geometries.Geometry geometry in multiPolygon.Geometries)
            {
                if(geometry is Polygon)
                {
                    Face2D face2D = ((Polygon)geometry).ToSAM(tolerance);
                    if (face2D == null)
                        continue;

                    result.Add(face2D);
                }
                else if(geometry is MultiPolygon)
                {
                    List<Face2D> face2Ds = ((MultiPolygon)geometry).ToSAM(tolerance);
                    if (face2Ds == null)
                        continue;

                    result.AddRange(face2Ds);
                }
                else if (geometry is LinearRing)
                {
                    result.Add(new Polygon((LinearRing)geometry).ToSAM(tolerance));
                }
            }

            return result;
        }
    }
}