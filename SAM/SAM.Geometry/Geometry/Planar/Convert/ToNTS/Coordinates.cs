﻿using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static List<Coordinate> ToNTS(this IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2Ds == null)
                return null;

            return point2Ds.ToList().ConvertAll(x => x.ToNTS(tolerance));
        }
    }
}