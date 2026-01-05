// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int DayOfYear(int hourOfYear)
        {
            return System.Convert.ToInt32(Math.Truncate(System.Convert.ToDouble(hourOfYear) / 24.0));
        }
    }
}
