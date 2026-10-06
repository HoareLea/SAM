// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Layout hint for a section; renderers may ignore it.
    /// </summary>
    public enum SectionWidth
    {
        [Description("Full")] Full,
        [Description("Half")] Half,
    }
}
