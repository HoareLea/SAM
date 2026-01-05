// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Zone)), Description("Analytical Zone Parameter")]
    public enum ZoneParameter
    {
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,

        [ParameterProperties("Zone Category", "Zone Category"), ParameterValue(Core.ParameterType.String)] ZoneCategory,
    }
}
