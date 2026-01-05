// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static string UserSAMDirectory()
        {
            return System.IO.Path.Combine(UserDocumentsDirectory(), "SAM");
        }
    }
}
