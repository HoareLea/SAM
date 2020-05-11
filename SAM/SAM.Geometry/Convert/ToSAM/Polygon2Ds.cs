using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Polygon2D> ToSAM_Polygon2Ds(this Polygon polygon, double tolerance = Core.Tolerance.Distance)
        {
            NetTopologySuite.Geometries.Geometry geometry = polygon?.Boundary;
            if (geometry == null)
                return null;

            if (geometry is LinearRing)
                return new List<Polygon2D>() { ((LinearRing)geometry).ToSAM(tolerance) };
            else if (geometry is MultiLineString)
            {
                MultiLineString multiLineString = (MultiLineString)geometry;
                IEnumerable<NetTopologySuite.Geometries.Geometry> geometries = multiLineString?.Geometries;
                if (geometries == null)
                    return null;

                List<Polygon2D> polygon2Ds = new List<Polygon2D>();
                foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
                {
                    LinearRing linearRing = geometry_Temp as LinearRing;
                    if (linearRing == null)
                        continue;

                    polygon2Ds.Add(linearRing.ToSAM(tolerance));
                }
                return polygon2Ds;
            }

            return null;
        }
    }
}