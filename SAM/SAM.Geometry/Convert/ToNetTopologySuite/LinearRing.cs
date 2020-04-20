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
        public static LinearRing ToNetTopologySuite(this Polygon2D polygon2D)
        {
            List<Point2D> point2Ds = polygon2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            return new LinearRing(point2Ds.ToNetTopologySuite().ToArray());
        }
    }
}
