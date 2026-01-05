// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Line2D Line2D(Point2D origin, double angle)
        {
            if (origin == null || double.IsNaN(angle) || double.IsInfinity(angle))
            {
                return null;
            }

            return new Line2D(origin, Vector2D(angle));

        }
    }
}
