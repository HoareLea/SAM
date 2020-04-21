﻿using System;
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
        public static LinearRing ToNetTopologySuite(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Point2D> point2Ds = polygon2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            point2Ds.Add(point2Ds.First());

            return new LinearRing(point2Ds.ToNetTopologySuite(tolerance).ToArray());
        }
    }
}
