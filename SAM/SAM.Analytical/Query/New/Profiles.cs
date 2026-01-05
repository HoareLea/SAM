// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IEnumerable<Profile> Profiles(this BuildingModel buildingModel, ProfileLibrary profileLibrary, bool includeProfileGroup = true)
        {
            if (buildingModel == null || profileLibrary == null)
                return null;

            return Profiles(buildingModel.GetSpaces(), profileLibrary, includeProfileGroup);
        }
    }
}
