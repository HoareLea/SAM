// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Message Type")]
    public enum MessageType
    {
        [Description("Undefined")] Undefined,
        [Description("Information")] Information,
        [Description("Warning")] Warning,
        [Description("Error")] Error
    }
}
