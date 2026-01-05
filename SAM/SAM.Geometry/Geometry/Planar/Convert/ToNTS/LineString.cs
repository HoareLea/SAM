// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static LineString ToNTS(this ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Point2D> point2Ds = segmentable2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            if (segmentable2D is IClosed2D)
            {
                return ToNTS((IClosed2D)segmentable2D, tolerance);
            }

            return new LineString(point2Ds.ToNTS(tolerance).ToArray());
        }
    }
}
