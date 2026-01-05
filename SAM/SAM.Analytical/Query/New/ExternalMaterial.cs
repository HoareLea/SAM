// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial ExternalMaterial(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary)
        {
            if (hostPartitionType == null || materialLibrary == null)
                return null;

            Architectural.MaterialLayer materialLayer = ExternalMaterialLayer(hostPartitionType);
            if (materialLayer == null)
                return null;

            return Material(materialLayer, materialLibrary);
        }

        public static IMaterial ExternalMaterial(this IHostPartition hostPartition, MaterialLibrary materialLibrary)
        {
            return ExternalMaterial(hostPartition?.Type(), materialLibrary);

        }
    }
}
