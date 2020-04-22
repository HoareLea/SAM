using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetTopologySuite.Geometries;
using SAM.Geometry.Planar;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Coordinate ToNetTopologySuite(this Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2D == null)
                return null;

            return new Coordinate(Core.Modify.Round(point2D.X, tolerance), Core.Modify.Round(point2D.Y, tolerance));
        }
    }
}
