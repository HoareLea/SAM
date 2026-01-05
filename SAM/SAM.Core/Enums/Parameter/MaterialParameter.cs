// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [AssociatedTypes(typeof(Material)), Description("Material Parameter")]
    public enum MaterialParameter
    {
        [ParameterProperties("Default Thickness", "Default Material Thickness"), DoubleParameterValue(0)] DefaultThickness,
    }
}
