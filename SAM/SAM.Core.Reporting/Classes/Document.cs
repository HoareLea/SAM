// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A titled group of blocks, built by one section builder.
    /// </summary>
    public sealed class DocumentSection
    {
        public DocumentSection(string id, string title, IEnumerable<DocumentBlock> blocks, SectionWidth width = SectionWidth.Full)
        {
            Id = id;
            Title = title;
            Blocks = (blocks ?? Enumerable.Empty<DocumentBlock>()).Where(x => x != null).ToList().AsReadOnly();
            Width = width;
        }

        public string Id { get; }

        public string Title { get; }

        public IReadOnlyList<DocumentBlock> Blocks { get; }

        public SectionWidth Width { get; }
    }

    /// <summary>
    /// Compact footer: provenance/status lines, and the legend of the markers actually used in the document.
    /// </summary>
    public sealed class DocumentFooter
    {
        public DocumentFooter(IEnumerable<string> lines, IEnumerable<string> legend = null)
        {
            Lines = (lines ?? Enumerable.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList().AsReadOnly();
            Legend = (legend ?? Enumerable.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList().AsReadOnly();
        }

        public IReadOnlyList<string> Lines { get; }

        public IReadOnlyList<string> Legend { get; }
    }

    /// <summary>
    /// A complete, renderer-neutral document: metadata, ordered sections of formatted blocks, and a footer. It is
    /// specific to one unit system and culture, because its values are already formatted.
    /// </summary>
    public sealed class Document
    {
        public Document(string id, DocumentMetadata metadata, IEnumerable<DocumentSection> sections, DocumentFooter footer, DocumentStyle style = null)
        {
            Id = id;
            this.metadata = new DocumentMetadata(metadata);
            Sections = (sections ?? Enumerable.Empty<DocumentSection>()).Where(x => x != null).ToList().AsReadOnly();
            Footer = footer ?? new DocumentFooter(null);
            Style = style ?? DocumentStyle.Default;
        }

        private readonly DocumentMetadata metadata;

        /// <summary>
        /// Document definition identifier, for example "space-assumptions".
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A copy of the document metadata.
        /// </summary>
        public DocumentMetadata Metadata => new DocumentMetadata(metadata);

        public IReadOnlyList<DocumentSection> Sections { get; }

        public DocumentFooter Footer { get; }

        public DocumentStyle Style { get; }

        /// <summary>
        /// Every formatted value in the document's key/value and table blocks, in reading order.
        /// </summary>
        public IEnumerable<FormattedValue> FormattedValues()
        {
            foreach (DocumentSection documentSection in Sections)
            {
                foreach (DocumentBlock documentBlock in documentSection.Blocks)
                {
                    if (documentBlock is KeyValueBlock keyValueBlock)
                    {
                        foreach (KeyValueRow keyValueRow in keyValueBlock.Rows)
                        {
                            yield return keyValueRow.Value;
                        }
                    }
                    else if (documentBlock is TableBlock tableBlock)
                    {
                        foreach (TableRow tableRow in tableBlock.Rows)
                        {
                            foreach (FormattedValue formattedValue in tableRow.Cells)
                            {
                                yield return formattedValue;
                            }
                        }
                    }
                }
            }
        }
    }
}
