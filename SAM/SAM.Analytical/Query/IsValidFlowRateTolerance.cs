// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Whether a flow-rate tolerance [l/s] is one a comparison can safely be made against: finite and
        /// not negative.
        /// <para>
        /// <b>Why this is a guard and not a detail.</b> Every Iteration 2 safety rule is expressed as a
        /// comparison against a tolerance - is this room below its Approved Document F floor, is this
        /// dwelling out of balance, can this product move this duty. <see cref="double.NaN"/> makes
        /// <i>every</i> one of those comparisons evaluate false, so a caller passing it silently switches
        /// off the derived balancing allocation, the imbalance refusal and the capacity check at once, and
        /// the result reports success. An infinite tolerance makes every comparison pass, which is the same
        /// failure wearing the opposite mask. A negative tolerance is not a margin at all.
        /// </para>
        /// <para>
        /// Zero is valid and simply means exact comparison.
        /// </para>
        /// <para>
        /// <b>Refused, never clamped.</b> Substituting a default for a nonsense tolerance would hide the
        /// caller's mistake behind an answer that looks right, and the answer is a compliance statement.
        /// The public entry points that take a tolerance check it here and refuse; the shared sentence
        /// they refuse with is <see cref="FlowRateToleranceRefusal"/>, so the same defect always reads the
        /// same way.
        /// </para>
        /// </summary>
        public static bool IsValidFlowRateTolerance(double tolerance_Lps)
        {
            return !double.IsNaN(tolerance_Lps) && !double.IsInfinity(tolerance_Lps) && tolerance_Lps >= 0;
        }

        /// <summary>
        /// The one sentence every Iteration 2 entry point refuses an unusable tolerance with, naming the
        /// value it was given.
        /// </summary>
        public static string FlowRateToleranceRefusal(double tolerance_Lps)
        {
            return string.Format(
                "A flow rate tolerance of {0} l/s cannot be compared against - it has to be a finite, non-negative number of litres per second, and zero means exact. Every Approved Document F floor, balance and capacity check in this iteration is a comparison against it, so nothing was done rather than have those checks quietly pass or quietly fail.",
                tolerance_Lps);
        }
    }
}
