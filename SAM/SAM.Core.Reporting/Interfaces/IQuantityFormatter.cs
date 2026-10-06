// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Units;
using System;
using System.Collections.Generic;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Turns typed report values into display text for one unit system and culture. This is the only place where
    /// report values meet display policy (display unit, precision, placeholders); conversion math comes from
    /// SAM.Units.
    /// <para>
    /// Comparable values (a heating/cooling pair, a table column) must share one unit: choose it with
    /// <see cref="SelectDisplayUnit"/> and format each value with <see cref="Format(ReportValue{Quantity}, DisplayUnit)"/>.
    /// <see cref="Format(ReportValue{Quantity})"/> is only for stand-alone values.
    /// </para>
    /// </summary>
    public interface IQuantityFormatter
    {
        UnitStyle UnitSystem { get; }

        /// <summary>
        /// One display unit for a group of comparable values of <paramref name="unitCategory"/>, chosen from the
        /// group's largest absolute value (for example W vs kW). Invalid quantities are ignored.
        /// </summary>
        DisplayUnit SelectDisplayUnit(UnitCategory unitCategory, IEnumerable<Quantity> quantities);

        /// <summary>
        /// The default display unit of a category, for example for a column header with no values.
        /// </summary>
        DisplayUnit DisplayUnit(UnitCategory unitCategory);

        /// <summary>
        /// Formats one value in a given unit. The unit must belong to the value's category.
        /// </summary>
        FormattedValue Format(ReportValue<Quantity> reportValue, DisplayUnit displayUnit);

        /// <summary>
        /// Formats a stand-alone value in the unit its own magnitude selects.
        /// </summary>
        FormattedValue Format(ReportValue<Quantity> reportValue);

        FormattedValue Format(ReportValue<string> reportValue);

        FormattedValue Format(ReportValue<DateTime> reportValue);

        /// <summary>
        /// Text shown for a value that is not available ("—").
        /// </summary>
        string NotAvailableText { get; }

        /// <summary>
        /// Text shown for a value that is not applicable ("n/a").
        /// </summary>
        string NotApplicableText { get; }
    }
}
