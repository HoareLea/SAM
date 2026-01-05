// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using NetTopologySuite.Geometries;

namespace SAM.Geometry.Planar
{
    public static partial class Convert
    {
        public static ISAMGeometry2D ToSAM(this NetTopologySuite.Geometries.Geometry geometry, double tolerance = Core.Tolerance.Distance)
        {
            if (geometry == null || geometry is MultiPoint)
            {
                return null;
            }

            return Convert.ToSAM(geometry as dynamic, tolerance);
        }
    }
}
