// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(OpeningType)), Description("OpeningType Parameter")]
    public enum OpeningTypeParameter
    {
        [ParameterProperties("Partition Analytical Type", "Partition Analytical Type"), ParameterValue(Core.ParameterType.String)] PartitionAnalyticalType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Default Frame Width", "Default Frame Width"), ParameterValue(Core.ParameterType.Double)] DefaultFrameWidth,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
    }
}
