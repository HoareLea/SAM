// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool Obsolete(this IGH_SAMComponent gH_SAMComponent)
        {
            if (gH_SAMComponent == null)
                return false;

            string componentVersion = gH_SAMComponent.ComponentVersion;
            string latestComponentVersion = gH_SAMComponent.LatestComponentVersion;

            if (string.IsNullOrEmpty(componentVersion) || string.IsNullOrEmpty(latestComponentVersion))
                return false;

            return componentVersion != latestComponentVersion;
        }
    }
}
