// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Analytical Profile Group.")]
    public enum ProfileGroup
    {
        [Description("Undefined")] Undefined,
        [Description("Gain")] Gain,
        [Description("Thermostat")] Thermostat,
        [Description("Humidistat")] Humidistat,
        [Description("Ventilation")] Ventilation,
    }
}
