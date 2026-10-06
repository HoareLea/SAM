// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Converts the raw date/time fields of a simulation-engine reporting row to a .NET
        /// <see cref="DateTime"/> under the <b>end-of-interval</b> convention: <c>Hour</c> runs
        /// 0–24 and <c>Minute</c> 0–60, so hour 24 means midnight at the end of the day (next
        /// day 00:00), minute 60 means the full hour, and sub-hourly rows start the day at
        /// <c>Hour = 0</c> (a <c>(0, 10)</c> row is the interval ending 00:10). Rows without a
        /// year (0 or negative — sizing/design-day environments write this) fall back to
        /// <paramref name="defaultYear"/>.
        /// <para>
        /// Normalisation: validate the calendar date, then add a <see cref="TimeSpan"/> —
        /// day/month/year rollover (including December 31 24:00 and leap years) is handled by
        /// <see cref="DateTime"/> arithmetic, never by clamping 24 → 23 or 60 → 59. Fields that
        /// cannot form a valid calendar date (for example February 29 under a non-leap calendar,
        /// month/day 0 warmup rows, hour &gt; 24, minute &gt; 60) return false with a diagnostic
        /// instead of throwing, so a malformed row can be skipped and reported rather than
        /// aborting a whole result set.
        /// </para>
        /// <para>
        /// Verified against live EnergyPlus <c>eplusout.sql</c> output, which is the convention
        /// this models; it applies to any engine reporting interval ends the same way.
        /// </para>
        /// </summary>
        /// <param name="year">Year value; values ≤ 0 fall back to <paramref name="defaultYear"/>.</param>
        /// <param name="month">Month value (1–12).</param>
        /// <param name="day">Day value (1–31, validated against the resolved year's calendar).</param>
        /// <param name="hour">Hour value (0–24; end-of-interval).</param>
        /// <param name="minute">Minute value (0–60; end-of-interval).</param>
        /// <param name="second">Seconds value (0–60).</param>
        /// <param name="defaultYear">Calendar year used when the row carries no year; values ≤ 0 fall back to 2017.</param>
        /// <param name="dateTime">Normalised interval-end timestamp.</param>
        /// <param name="diagnostic">Structured description of a rejected row; null on success.</param>
        /// <returns>True when the row was normalised.</returns>
        public static bool TryGetDateTime(int year, int month, int day, int hour, int minute, int second, int defaultYear, out DateTime dateTime, out string diagnostic)
        {
            dateTime = default(DateTime);
            diagnostic = null;

            if (year <= 0)
            {
                year = defaultYear > 0 ? defaultYear : 2017;
            }

            if (month < 1 || month > 12)
            {
                diagnostic = string.Format("Invalid timestamp: month {0} is out of range (row {1}-{2}-{3} {4}:{5})", month, year, month, day, hour, minute);
                return false;
            }

            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (day < 1 || day > daysInMonth)
            {
                diagnostic = string.Format("Invalid timestamp: day {0} does not exist in {1}-{2} ({3} is {4}a leap year, {5} has {6} days) — the row was skipped, not clamped", day, year, month, year, DateTime.IsLeapYear(year) ? "" : "not ", month, daysInMonth);
                return false;
            }

            if (hour < 0 || hour > 24)
            {
                diagnostic = string.Format("Invalid timestamp: hour {0} is out of range (0–24, end-of-interval convention)", hour);
                return false;
            }

            if (minute < 0 || minute > 60)
            {
                diagnostic = string.Format("Invalid timestamp: minute {0} is out of range (0–60, end-of-interval convention)", minute);
                return false;
            }

            if (second < 0 || second > 60)
            {
                diagnostic = string.Format("Invalid timestamp: second {0} is out of range (0–60)", second);
                return false;
            }

            dateTime = new DateTime(year, month, day).Add(new TimeSpan(0, hour, minute, second));
            return true;
        }

        /// <summary>
        /// The 0-based hour-of-year index of the reporting interval ending at
        /// <paramref name="endOfInterval"/>: the interval index, not the endpoint's hour
        /// (01:00 is the end of interval 0; December 31 24:00 is the end of interval 8759).
        /// Computed by stepping one tick back into the interval, so sub-hourly rows map to
        /// their containing hour and day/month/year rollovers stay correct.
        /// </summary>
        /// <param name="endOfInterval">Interval-end timestamp, as produced by <see cref="TryGetDateTime"/>.</param>
        /// <returns>0-based hour of year, or -1 for <see cref="DateTime.MinValue"/>.</returns>
        public static int IntervalHourOfYear(DateTime endOfInterval)
        {
            if (endOfInterval == DateTime.MinValue)
            {
                return -1;
            }

            return HourOfYear(endOfInterval.AddTicks(-1));
        }
    }
}
