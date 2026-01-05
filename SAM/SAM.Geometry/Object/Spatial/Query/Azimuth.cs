// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static double Azimuth(this IFace3DObject face3DObject, Vector3D referenceDirection)
        {
            return Geometry.Spatial.Query.Azimuth(face3DObject?.Face3D, referenceDirection);
        }
    }
}
