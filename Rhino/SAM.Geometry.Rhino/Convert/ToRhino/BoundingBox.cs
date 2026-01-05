// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static global::Rhino.Geometry.BoundingBox ToRhino(this Spatial.BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D == null)
            {
                return global::Rhino.Geometry.BoundingBox.Unset;
            }

            return new global::Rhino.Geometry.BoundingBox(boundingBox3D.Min.ToRhino(), boundingBox3D.Max.ToRhino());
        }
    }
}
