// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Units
{
    public enum UnitCategory
    {
        [Description("Undefined")] Undefined,
        [Description("Temperature")] Temperature,
        [Description("Humidity Ratio")] HumidityRatio,
        [Description("Density")] Density,
        [Description("Specific Volume")] SpecificVolume,
        [Description("Pressure")] Pressure,
        [Description("Air Flow")] AirFlow,
        [Description("Relative Humidity")] RelativeHumidity,
        [Description("Efficiency")] Efficiency,
        [Description("Enthalpy")] Enthaply,
        [Description("Specific Enthalpy")] SpecificEnthaply,
        [Description("Power")] Power,
    }
}
