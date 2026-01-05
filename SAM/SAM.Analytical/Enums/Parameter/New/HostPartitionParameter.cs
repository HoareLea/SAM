// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(IHostPartition)), Description("HostPartition Parameter")]
    public enum HostPartitionParameter
    {
        [ParameterProperties("Adiabatic", "Adiabatic"), ParameterValue(Core.ParameterType.Boolean)] Adiabatic,
    }
}
