// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Weekday(this Week week)
        {
            if (week == Week.Undefined)
                return false;

            return !Weekend(week);
        }
    }
}
