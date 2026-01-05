// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Architectural.MaterialLayer InternalMaterialLayer(this HostPartitionType hostPartitionType)
        {
            List<Architectural.MaterialLayer> materialLayer = hostPartitionType?.MaterialLayers;
            if (materialLayer == null || materialLayer.Count == 0)
                return null;

            return materialLayer.First();
        }

        public static Architectural.MaterialLayer InternalConstructionLayer(this IHostPartition hostPartition)
        {
            return InternalMaterialLayer(hostPartition?.Type());


        }
    }
}
