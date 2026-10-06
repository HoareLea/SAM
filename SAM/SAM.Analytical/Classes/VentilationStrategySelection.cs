// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// The ventilation strategy that governs one space, or a refusal saying why none is available.
    /// <para>
    /// <b>Two outcomes, and no third.</b> Either an <c>OverheatingScenario</c> stated how the space is
    /// ventilated, or nothing did - and in the second case there is no defensible answer to fall back on.
    /// Approved Document O judges a naturally ventilated space and a mechanically ventilated one against
    /// different TM59 criteria, so guessing does not produce a slightly worse number; it produces a number
    /// measured against the wrong rule while looking exactly like a real result.
    /// </para>
    /// <para>
    /// The same shape as <see cref="SystemCapabilitySelection"/>, for the same reason: a refusal carries the
    /// sentence that explains it, so a report or a log can say what was missing rather than that something
    /// was.
    /// </para>
    /// </summary>
    public class VentilationStrategySelection
    {
        private VentilationStrategySelection(string ventilationStrategy, string reason)
        {
            VentilationStrategy = ventilationStrategy;
            Reason = reason;
        }

        /// <summary>
        /// The stated strategy - <c>NV</c>, <c>MV</c>, <c>MVRE</c>, <c>UV</c> - trimmed and upper-cased. Null
        /// where this is a refusal.
        /// </summary>
        public string VentilationStrategy { get; }

        /// <summary>Whether a strategy was stated.</summary>
        public bool IsSelected => VentilationStrategy != null;

        /// <summary>Why, in words, for a report or a log. Null on success.</summary>
        public string Reason { get; }

        internal static VentilationStrategySelection Selected(string ventilationStrategy)
        {
            return new VentilationStrategySelection(ventilationStrategy, null);
        }

        internal static VentilationStrategySelection Refused(string reason)
        {
            return new VentilationStrategySelection(null, reason);
        }

        public override string ToString()
        {
            return IsSelected ? VentilationStrategy : string.Format("Not selected: {0}", Reason);
        }
    }
}
