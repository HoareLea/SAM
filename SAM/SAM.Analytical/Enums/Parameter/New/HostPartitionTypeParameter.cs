// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(HostPartitionType)), Description("HostPartitionType Parameter")]
    public enum HostPartitionTypeParameter
    {
        [ParameterProperties("Partition Analytical Type", "Partition Analytical Type"), ParameterValue(Core.ParameterType.String)] PartitionAnalyticalType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color
    }
}
