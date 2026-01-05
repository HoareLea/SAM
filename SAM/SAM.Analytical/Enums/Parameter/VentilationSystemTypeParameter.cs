// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(VentilationSystemType)), Description("Ventilation System Type Parameter")]
    public enum VentilationSystemTypeParameter
    {
        [ParameterProperties("Temperature Difference", "Supply Air And Room Remperature Difference [K]"), DoubleParameterValue(0)] TemperatureDifference,
        [ParameterProperties("Air Supply Method", "Air Supply Method"), ParameterValue(Core.ParameterType.String)] AirSupplyMethod,
    }
}
