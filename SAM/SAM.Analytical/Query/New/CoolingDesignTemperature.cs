// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignTemperature(this Space space, BuildingModel buildingModel)
        {
            Profile profile = buildingModel?.GetProfile(space, ProfileType.Cooling, true);
            if (profile == null)
            {
                return double.NaN;
            }

            return profile.MinValue;
        }
    }
}
