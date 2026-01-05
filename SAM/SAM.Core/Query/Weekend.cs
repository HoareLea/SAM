// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Weekend(this Week week)
        {
            return week == Week.Saturday || week == Week.Sunday;
        }
    }
}
