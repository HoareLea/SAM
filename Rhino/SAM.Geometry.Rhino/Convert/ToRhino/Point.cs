// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.Point ToRhino_Point(this Spatial.Point3D point3D)
        {
            if (point3D == null)
            {
                return null;
            }

            return new global::Rhino.Geometry.Point(point3D.ToRhino());
        }
    }
}
