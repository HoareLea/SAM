// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.IO;

namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserSAMTemporaryDirectory()
        {
            return Path.Combine(Path.GetTempPath(), "SAM");
        }
    }
}
