// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Core
{
    [Description("Log Record Type")]
    public enum LogRecordType
    {
        [Description("Undefined")] Undefined,
        [Description("Message")] Message,
        [Description("Warning")] Warning,
        [Description("Error")] Error
    }
}
