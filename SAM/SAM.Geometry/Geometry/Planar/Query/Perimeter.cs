// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static double Perimeter(this IClosed2D closed2D)
        {
            if (closed2D == null)
                return double.NaN;

            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetLength();

            throw new System.NotImplementedException();
        }
    }
}
