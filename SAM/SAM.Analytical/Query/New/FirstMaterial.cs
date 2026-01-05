// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial FirstMaterial(this IHostPartition hostPartition, Vector3D direction, MaterialLibrary materialLibrary)
        {
            if (hostPartition == null || direction == null || materialLibrary == null)
                return null;

            Architectural.MaterialLayer materialLayer = FirstMaterialLayer(hostPartition, direction);
            if (materialLayer == null)
                return null;

            return Material(materialLayer, materialLibrary);
        }
    }
}
