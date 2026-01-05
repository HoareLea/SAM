// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string ResourcesDirectory()
        {
            return Core.Query.ResourcesDirectory(Core.ActiveSetting.Setting, typeof(ActiveSetting).Assembly);
        }
    }
}
