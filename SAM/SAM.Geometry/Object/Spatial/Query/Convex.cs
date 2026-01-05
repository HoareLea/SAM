// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {

        public static bool Convex(this IFace3DObject face3Dobject, bool externalEdge = true, bool internalEdges = true)
        {
            return Geometry.Spatial.Query.Convex(face3Dobject?.Face3D, externalEdge, internalEdges);
        }
    }
}
