using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static NetTopologySuite.Geometries.Geometry ToNTS(this ISAMGeometry2D sAMGeometry2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (sAMGeometry2D == null)
            {
                return null;
            }

            if(sAMGeometry2D is Polygon2D)
            {
                return ToNTS((IClosed2D)sAMGeometry2D, tolerance);
            }

            if(sAMGeometry2D is Point2D)
            {
                Coordinate coordinate = ToNTS((Point2D)sAMGeometry2D);
                if(coordinate == null)
                {
                    return null;
                }

                return new Point(coordinate);
            }

            return ToNTS(sAMGeometry2D as dynamic, tolerance);
        }
    }
}