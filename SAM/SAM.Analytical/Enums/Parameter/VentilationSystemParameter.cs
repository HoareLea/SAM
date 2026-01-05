// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(VentilationSystem)), Description("Ventilation System Parameter")]
    public enum VentilationSystemParameter
    {
        [ParameterProperties("Supply Unit Name", "Supply Unit Name"), ParameterValue(Core.ParameterType.String)] SupplyUnitName,
        [ParameterProperties("Exhaust Unit Name", "Exhaust Unit Name"), ParameterValue(Core.ParameterType.String)] ExhaustUnitName,
    }
}
