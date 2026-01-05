// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// TM59 Space Application
    /// </summary>
    public enum TM59SpaceApplication
    {
        [Description("Undefined")] Undefined,
        [Description("Sleeping")] Sleeping,
        [Description("Living")] Living,
        [Description("Cooking")] Cooking
    }
}
