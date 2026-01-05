// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D SplitEdges(this Face3D face3D_Split, Face3D face3D_Splitting, double tolerance = Core.Tolerance.Distance)
        {
            TrySplitEdges(face3D_Split, face3D_Splitting, out Face3D result, tolerance);

            return result;
        }
    }
}
