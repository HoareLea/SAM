// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Polygon2D Polygon2D(this IClosed2D closed2D)
        {
            if (closed2D == null)
            {
                return null;
            }

            if (closed2D is ISegmentable2D)
            {
                return new Polygon2D(((ISegmentable2D)closed2D).GetPoints());
            }

            return null;
        }
    }
}
