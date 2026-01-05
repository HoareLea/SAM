// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Vertical(this IFace3DObject face3DObject, double tolerance = Core.Tolerance.Distance)
        {
            if (face3DObject == null)
                return false;

            return Geometry.Spatial.Query.Vertical(face3DObject.Face3D, tolerance);
        }
    }
}
