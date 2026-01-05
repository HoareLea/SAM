// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Adiabatic(this HostPartitionType hostPartitionType)
        {
            List<Architectural.MaterialLayer> materialLayers = hostPartitionType?.MaterialLayers;
            if (materialLayers == null)
            {
                return false;
            }

            if (materialLayers.Count == 0)
            {
                return true;
            }

            return materialLayers.Find(x => double.IsNaN(x.Thickness)) != null;
        }

        public static bool Adiabatic(this IHostPartition hostPartition)
        {
            if (hostPartition == null)
            {
                return false;
            }

            if (Adiabatic(hostPartition.Type()))
            {
                return true;
            }

            if (!hostPartition.TryGetValue(HostPartitionParameter.Adiabatic, out bool result))
            {
                return false;
            }

            return result;
        }
    }
}
