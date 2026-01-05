// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(HeatingSystemType)), Description("Heating System Type Parameter")]
    public enum HeatingSystemTypeParameter
    {
        [ParameterProperties("Radiant Proportion", "Radiant Proportion"), DoubleParameterValue(0, 1)] RadiantProportion,
        [ParameterProperties("View Coefficient", "View Coefficient"), DoubleParameterValue(0, 1)] ViewCoefficient,
        [ParameterProperties("Supply Circuit Temperature", "Supply Circuit Temperature"), DoubleParameterValue(0)] SupplyCircuitTemperature,
        [ParameterProperties("Return Circuit Temperature", "Return Circuit Temperature"), DoubleParameterValue(0)] ReturnCircuitTemperature,
    }
}
