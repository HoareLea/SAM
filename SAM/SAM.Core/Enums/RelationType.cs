// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("RelationType")]
    public enum RelationType
    {
        [Description("Undefined")] Undefined,
        [Description("One to one")] OneToOne,
        [Description("One to many")] OneToMany,
        [Description("Many to one")] ManyToOne,
        [Description("Many to many")] ManyToMany
    }
}
