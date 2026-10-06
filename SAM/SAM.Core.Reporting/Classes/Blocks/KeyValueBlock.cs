// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// One labelled value in a <see cref="KeyValueBlock"/>.
    /// </summary>
    public sealed class KeyValueRow
    {
        public KeyValueRow(string label, FormattedValue value, string subLabel = null)
        {
            Label = label;
            Value = value ?? throw new ArgumentNullException(nameof(value));
            SubLabel = subLabel;
        }

        public string Label { get; }

        /// <summary>
        /// Optional supporting text printed under the label in a smaller size, for example "lower RH limit".
        /// </summary>
        public string SubLabel { get; }

        public FormattedValue Value { get; }
    }

    /// <summary>
    /// A list of label / value pairs.
    /// </summary>
    public sealed class KeyValueBlock : DocumentBlock
    {
        public KeyValueBlock(string id, string title, IEnumerable<KeyValueRow> rows)
            : base(id)
        {
            Title = title;
            Rows = (rows ?? Enumerable.Empty<KeyValueRow>()).Where(x => x != null).ToList().AsReadOnly();
        }

        /// <summary>
        /// Optional sub-heading.
        /// </summary>
        public string Title { get; }

        public IReadOnlyList<KeyValueRow> Rows { get; }
    }
}
