// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Shared helpers for section builders: group formatting through one display unit, and the "whole section is
    /// missing" notice rule.
    /// </summary>
    internal static class SectionFormat
    {
        /// <summary>
        /// Formats comparable values of one category in a single display unit chosen from the whole group.
        /// </summary>
        public static FormattedValue[] Group(IQuantityFormatter quantityFormatter, UnitCategory unitCategory, params ReportValue<Quantity>[] reportValues)
        {
            DisplayUnit displayUnit = Unit(quantityFormatter, unitCategory, reportValues);
            return reportValues.Select(x => quantityFormatter.Format(x, displayUnit)).ToArray();
        }

        /// <summary>
        /// The shared display unit of a group of values.
        /// </summary>
        public static DisplayUnit Unit(IQuantityFormatter quantityFormatter, UnitCategory unitCategory, IEnumerable<ReportValue<Quantity>> reportValues)
        {
            List<Quantity> quantities = new List<Quantity>();
            foreach (ReportValue<Quantity> reportValue in reportValues)
            {
                if (reportValue != null && reportValue.TryGetValue(out Quantity quantity))
                {
                    quantities.Add(quantity);
                }
            }

            return quantityFormatter.SelectDisplayUnit(unitCategory, quantities);
        }

        public static KeyValueRow Row(string label, FormattedValue formattedValue, string subLabel = null)
        {
            return new KeyValueRow(label, formattedValue, subLabel);
        }

        /// <summary>
        /// True when none of the values is available, in which case a section shows one notice instead of a block
        /// full of placeholders.
        /// </summary>
        public static bool AllMissing(params IReportValue[] reportValues)
        {
            return reportValues.All(x => x == null || !x.HasValue);
        }

        public static FormattedValue Label(string text)
        {
            return FormattedValue.Label(text);
        }

        /// <summary>
        /// The formatter's placeholder ("—" / "n/a") for a value that is not available, keeping its reason.
        /// </summary>
        public static FormattedValue Placeholder(IQuantityFormatter quantityFormatter, IReportValue reportValue)
        {
            string text = reportValue.Availability == Availability.NotApplicable ? quantityFormatter.NotApplicableText : quantityFormatter.NotAvailableText;
            return new FormattedValue(text, null, reportValue.Availability, note: reportValue.Note);
        }

        /// <summary>
        /// Display text for an available value that is not a quantity (a peak hour), keeping its provenance.
        /// </summary>
        public static FormattedValue Text(string text, IReportValue reportValue)
        {
            return new FormattedValue(text, null, reportValue.Availability, reportValue.Freshness, reportValue.Source, reportValue.SourceTimestamp, reportValue.Note);
        }
    }
}
