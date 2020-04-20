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
        public static Polygon2D ToSAM(this Polygon polygon)
        {
            List<Point2D> point2Ds = polygon?.Coordinates.ToSAM();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            point2Ds.RemoveAt(point2Ds.Count - 1);

            return new Polygon2D(point2Ds);
        }

        public static Polygon2D ToSAM(this LinearRing linearRing)
        {
            if (linearRing == null)
                return null;

            return new Polygon2D(linearRing.Coordinates.ToSAM());
        }
    }
}
