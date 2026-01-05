// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static bool Below(this IFace3DObject face3DObject, Plane plane, double tolerance = 0)
        {
            return Geometry.Spatial.Query.Below(face3DObject?.Face3D, plane, tolerance);
        }
    }
}
