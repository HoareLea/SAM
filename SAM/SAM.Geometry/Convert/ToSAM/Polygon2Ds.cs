using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClipperLib;

using NetTopologySuite.Geometries;

using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Polygon2D> ToSAM(this Polygon polygon)
        {
            NetTopologySuite.Geometries.Geometry geometry = polygon?.Boundary;
            if (geometry == null)
                return null;

            if (geometry is LinearRing)
                return new List<Polygon2D>() { ((LinearRing)geometry).ToSAM() };
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
                return polygon2Ds;
            }

            return null;
        }
    }
}
