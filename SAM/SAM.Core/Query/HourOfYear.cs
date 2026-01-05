// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static int HourOfYear(this DateTime dateTime)
        {
            return ((dateTime.DayOfYear - 1) * 24) + dateTime.Hour;
        }

        public static int HourOfYear(int dayOfYear, int hourOfDay = 0)
        {
            return (dayOfYear * 24) + hourOfDay;
        }
    }
}
