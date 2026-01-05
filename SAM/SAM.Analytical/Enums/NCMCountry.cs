// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("NCM Country")]
    public enum NCMCountry
    {
        [Description("Undefined")] Undefined,
        [Description("England")] England,
        [Description("Wales")] Wales,
        [Description("Scotland")] Scotland,
    }
}
