// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static InternalConditionParameter? SystemTypeInternalConditionParameter(this MechanicalSystemCategory mechanicalSystemCategory)
        {

            switch (mechanicalSystemCategory)
            {
                case Analytical.MechanicalSystemCategory.Cooling:
                    return Analytical.InternalConditionParameter.CoolingSystemTypeName;

                case Analytical.MechanicalSystemCategory.Heating:
                    return Analytical.InternalConditionParameter.HeatingSystemTypeName;

                case Analytical.MechanicalSystemCategory.Ventilation:
                    return Analytical.InternalConditionParameter.VentilationSystemTypeName;
            }

            return null;
        }
    }
}
