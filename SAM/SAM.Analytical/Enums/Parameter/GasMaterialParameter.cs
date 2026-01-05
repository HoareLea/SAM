// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.GasMaterial)), Description("GasMaterial Parameter")]
    public enum GasMaterialParameter
    {
        [ParameterProperties("Heat Transfer Coefficient", "Heat Transfer Coefficient"), DoubleParameterValue(0)] HeatTransferCoefficient,
        [ParameterProperties("Default Gas Type", "Default Gas Type"), ParameterValue(Core.ParameterType.String)] DefaultGasType

    }
}
