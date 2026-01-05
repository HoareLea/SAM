// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        /// <summary>
        /// Converts hour index to string
        /// </summary>
        /// <param name="hourIndex">Value usualy between 0 and 8759 representing hour in year</param>
        /// <param name="year">Year</param>
        /// <returns>String</returns>
        public static string ToString(int hourIndex, int year = 2018)
        {
            return ToString(ToDateTime(hourIndex, year));
        }

        public static string ToString(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy@HH:mm");
        }
    }
}
