// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A value ready for a document: display text and unit text, plus the status and provenance of the value it came
    /// from. Renderers read only this - they never see engineering units and never convert.
    /// </summary>
    public sealed class FormattedValue
    {
        public FormattedValue(string text, string unit, Availability availability, Freshness? freshness = null, ReportValueSource? source = null, DateTime? sourceTimestamp = null, string note = null)
        {
            Text = text ?? string.Empty;
            Unit = unit;
            Availability = availability;
            Freshness = availability == Availability.Available ? freshness : null;
            Source = availability == Availability.Available ? source : null;
            SourceTimestamp = availability == Availability.Available ? sourceTimestamp : null;
            Note = note;
        }

        /// <summary>
        /// Display text: a formatted number, a name, or a placeholder for a missing value.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Display unit (for example "m²"), or null when the value has none or is not available.
        /// </summary>
        public string Unit { get; }

        public Availability Availability { get; }

        public Freshness? Freshness { get; }

        public ReportValueSource? Source { get; }

        public DateTime? SourceTimestamp { get; }

        public string Note { get; }

        /// <summary>
        /// True when the renderer should flag the value as out of date (a dagger in the default PDF).
        /// </summary>
        public bool IsOutOfDate => Freshness == Reporting.Freshness.OutOfDate;

        /// <summary>
        /// Plain text that is not a report value, such as a row label.
        /// </summary>
        public static FormattedValue Label(string text)
        {
            return new FormattedValue(text, null, Availability.Available);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Unit) ? Text : string.Format("{0} {1}", Text, Unit);
        }
    }
}
