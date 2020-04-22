using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Face2D ToSAM(this Polygon polygon)
        {
            NetTopologySuite.Geometries.Geometry geometry = polygon?.Boundary;
            if (geometry == null)
                return null;

            if (geometry is LinearRing)
                return new Face2D(((LinearRing)geometry).ToSAM());
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
                    
                    polygon2Ds.Add(linearRing.ToSAM());
                }
                return SAM.Geometry.Planar.Create.Face2Ds(polygon2Ds).First();
            }

            return null;
        }
    }
}
