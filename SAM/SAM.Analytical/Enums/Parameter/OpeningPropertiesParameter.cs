// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(IOpeningProperties)), Description("OpeningProperties Parameter")]
    public enum OpeningPropertiesParameter
    {
        [ParameterProperties("Function", "Function"), ParameterValue(Core.ParameterType.String)] Function,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
    }
}
