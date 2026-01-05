// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [AssociatedTypes(typeof(Location)), Description("Location Parameter")]
    public enum LocationParameter
    {
        [ParameterProperties("Time Zone", "UTC TimeZone"), ParameterValue(ParameterType.String)] TimeZone,
    }
}
