// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A table column. <see cref="Unit"/> is set when every value in the column shares one display unit, so a
    /// renderer can print it once in the header.
    /// </summary>
    public sealed class TableColumn
    {
        public TableColumn(string header, string unit = null, ColumnAlignment alignment = ColumnAlignment.Right, string group = null)
        {
            Header = header;
            Unit = unit;
            Alignment = alignment;
            Group = group;
        }

        public string Header { get; }

        public string Unit { get; }

        public ColumnAlignment Alignment { get; }

        /// <summary>
        /// Optional column-group heading spanning adjacent columns of the same group.
        /// </summary>
        public string Group { get; }
    }

    /// <summary>
    /// A table row: one formatted cell per column. The first column usually holds a label.
    /// </summary>
    public sealed class TableRow
    {
        public TableRow(IEnumerable<FormattedValue> cells)
        {
            Cells = (cells ?? Enumerable.Empty<FormattedValue>()).ToList().AsReadOnly();
            if (Cells.Any(x => x == null))
            {
                throw new ArgumentException("A table cell cannot be null.", nameof(cells));
            }
        }

        public TableRow(params FormattedValue[] cells)
            : this((IEnumerable<FormattedValue>)cells)
        {
        }

        public IReadOnlyList<FormattedValue> Cells { get; }
    }

    /// <summary>
    /// A table. Comparable values in a column are formatted in one shared display unit by the section builder,
    /// never per value.
    /// </summary>
    public sealed class TableBlock : DocumentBlock
    {
        public TableBlock(string id, string title, IEnumerable<TableColumn> columns, IEnumerable<TableRow> rows, bool repeatHeader = true)
            : base(id)
        {
            Title = title;
            Columns = (columns ?? Enumerable.Empty<TableColumn>()).ToList().AsReadOnly();
            Rows = (rows ?? Enumerable.Empty<TableRow>()).Where(x => x != null).ToList().AsReadOnly();
            RepeatHeader = repeatHeader;

            foreach (TableRow tableRow in Rows)
            {
                if (tableRow.Cells.Count != Columns.Count)
                {
                    throw new ArgumentException(string.Format("Table {0}: a row has {1} cells for {2} columns.", id, tableRow.Cells.Count, Columns.Count), nameof(rows));
                }
            }
        }

        public string Title { get; }

        public IReadOnlyList<TableColumn> Columns { get; }

        public IReadOnlyList<TableRow> Rows { get; }

        /// <summary>
        /// Repeat the header row when the table breaks across pages.
        /// </summary>
        public bool RepeatHeader { get; }
    }
}
