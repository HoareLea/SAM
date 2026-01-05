// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static HostPartitionType Type(this IHostPartition hostPartition)
        {
            return (hostPartition as dynamic)?.Type;
        }

        public static OpeningType Type(this IOpening opening)
        {
            return (opening as dynamic)?.Type;
        }
    }
}
