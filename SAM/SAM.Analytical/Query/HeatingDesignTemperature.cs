// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double HeatingDesignTemperature(this Space space, AnalyticalModel analyticalModel)
        {
            return HeatingDesignTemperature(space, analyticalModel?.ProfileLibrary);
        }

        public static double HeatingDesignTemperature(this Space space, ProfileLibrary profileLibrary)
        {
            if (space == null || profileLibrary == null)
                return double.NaN;

            return HeatingDesignTemperature(space.InternalCondition, profileLibrary);
        }

        public static double HeatingDesignTemperature(this InternalCondition internalCondition, ProfileLibrary profileLibrary)
        {
            if (internalCondition == null)
                return double.NaN;

            Profile profile = internalCondition.GetProfile(ProfileType.Heating, profileLibrary);

            return profile == null ? double.NaN : profile.MaxValue;
        }
    }
}
