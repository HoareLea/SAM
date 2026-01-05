// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.Material)), Description("Material Parameter")]
    public enum MaterialParameter
    {
        //[ParameterProperties("Type Name", "Type Name"), ParameterValue(Core.ParameterType.String)] TypeName,
        //[ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Vapour Diffusion Factor", "Vapour Diffusion Factor"), DoubleParameterValue(0)] VapourDiffusionFactor,
    }
}
