// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core.Grasshopper
{
    [Flags]
    public enum ParamVisibility
    {
        Voluntary = 0,
        Mandatory = 1,
        Default = 2,
        Binding = 3
    }
}
