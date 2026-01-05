// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Plane ToRhino(this Spatial.Plane plane)
        {
            return new global::Rhino.Geometry.Plane(ToRhino(plane.Origin), ToRhino(plane.AxisX), ToRhino(plane.AxisY));
        }
    }
}
