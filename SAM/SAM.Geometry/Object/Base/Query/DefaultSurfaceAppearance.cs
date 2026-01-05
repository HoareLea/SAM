// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


namespace SAM.Geometry.Object
{
    public static partial class Query
    {
        public static SurfaceAppearance DefaultSurfaceAppearance()
        {
            CurveAppearance curveAppearance = DefaultCurveAppearance();

            return new SurfaceAppearance(System.Drawing.Color.FromArgb(128, 128, 128), curveAppearance.Color, curveAppearance.Thickness);
        }

    }
}
