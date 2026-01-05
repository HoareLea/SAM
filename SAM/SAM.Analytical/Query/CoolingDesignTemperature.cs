// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CoolingDesignTemperature(this Space space, AnalyticalModel analyticalModel)
        {
            return CoolingDesignTemperature(space, analyticalModel?.ProfileLibrary);
        }

        public static double CoolingDesignTemperature(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            return CoolingDesignTemperature(space.InternalCondition, profileLibrary);
        }

        public static double CoolingDesignTemperature(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null || profileLibrary == null)
                return double.NaN;

            Profile profile = internalCondition.GetProfile(ProfileType.Cooling, profileLibrary);

            return profile == null ? double.NaN : profile.MinValue;
        }
    }
}
