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
            if (polygon == null)
                return null;

            return new Polygon2D(polygon.Coordinates.ToSAM());
        }
    }
}
