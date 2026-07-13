// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static bool Obsolete(this IGH_SAMComponent gH_SAMComponent)
        {
            return GetObsoleteSeverity(gH_SAMComponent) != SAM.Core.Grasshopper.ObsoleteSeverity.None;
        }

        public static ObsoleteSeverity GetObsoleteSeverity(this IGH_SAMComponent gH_SAMComponent)
        {
            if (gH_SAMComponent == null)
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.None;
            }

            string componentVersion = gH_SAMComponent.ComponentVersion;
            string latestComponentVersion = gH_SAMComponent.LatestComponentVersion;

            if (string.IsNullOrEmpty(componentVersion) || string.IsNullOrEmpty(latestComponentVersion))
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.None;
            }

            if (componentVersion == latestComponentVersion)
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.None;
            }

            if (!Version.TryParse(componentVersion, out Version version_Component))
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.Standard;
            }

            if (!Version.TryParse(latestComponentVersion, out Version version_Latest))
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.Standard;
            }

            if (version_Component.Major != version_Latest.Major)
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.Breaking;
            }

            if (version_Component.Minor != version_Latest.Minor)
            {
                return SAM.Core.Grasshopper.ObsoleteSeverity.Standard;
            }

            return SAM.Core.Grasshopper.ObsoleteSeverity.Advisory;
        }
    }
}
