// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        /// <summary>
        /// Converts hour index to dateTime
        /// </summary>
        /// <param name="hourIndex">Value usually between 0 and 8759 representing hour in year</param>
        /// <param name="year">Year</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(int hourIndex, int year = 2018)
        {
            return new DateTime(TimeSpan.FromHours(hourIndex).Ticks + new DateTime(year, 1, 1).Ticks);
        }
    }
}
