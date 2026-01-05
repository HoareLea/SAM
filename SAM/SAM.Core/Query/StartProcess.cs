// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Diagnostics;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Process StartProcess(this string path, string arguments = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true,
                Verb = "open",
                Arguments = arguments
            };

            return Process.Start(processStartInfo);
        }
    }
}
