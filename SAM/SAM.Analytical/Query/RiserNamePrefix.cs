// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string RiserNamePrefix(this MechanicalSystemCategory mechanicalSystemCategory)
        {
            switch (mechanicalSystemCategory)
            {
                case Analytical.MechanicalSystemCategory.Cooling:
                    return "RC";

                case Analytical.MechanicalSystemCategory.Ventilation:
                    return "RV";

                case Analytical.MechanicalSystemCategory.Heating:
                    return "RH";
            }

            return null;
        }
    }
}
