// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static VentilationSystemType VentilationSystemType(System.Guid guid, string name, string description, AirSupplyMethod airSupplyMethod = AirSupplyMethod.Undefined, double temperatureDifference = double.NaN)
        {
            VentilationSystemType result = new VentilationSystemType(guid, name, description);

            if (airSupplyMethod != AirSupplyMethod.Undefined)
                result.SetValue(VentilationSystemTypeParameter.AirSupplyMethod, airSupplyMethod.ToString());

            if (!double.IsNaN(temperatureDifference))
                result.SetValue(VentilationSystemTypeParameter.TemperatureDifference, temperatureDifference);

            return result;
        }
    }
}
