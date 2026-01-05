// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Function Type.")]
    public enum FunctionType
    {
        [Description("Undefined")] Undefined,
        [Description("tcmvc")] tcmvc,
        [Description("tcmvn")] tcmvn,
        [Description("tmmvn")] tmmvn,
        [Description("tcbvc")] tcbvc,
        [Description("Other")] Other
    }
}
