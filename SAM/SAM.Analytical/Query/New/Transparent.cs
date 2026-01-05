// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Transparent(this HostPartitionType hostPartitionType, MaterialLibrary materialLibrary = null)
        {
            MaterialType materialType = Architectural.Query.MaterialType(hostPartitionType?.MaterialLayers, materialLibrary);
            return materialType == Core.MaterialType.Transparent;
        }
    }
}
