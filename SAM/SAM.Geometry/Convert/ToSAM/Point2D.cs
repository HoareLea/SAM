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
        public static Point2D ToSAM(this IntPoint intPoint, double tolerance = Core.Tolerance.MicroDistance)
        {
            return new Point2D(intPoint.X * tolerance, intPoint.Y * tolerance);
        }

        public static Point2D ToSAM(this Coordinate coordinate)
        {
            return new Point2D(coordinate.X, coordinate.Y);
        }
    }
}
