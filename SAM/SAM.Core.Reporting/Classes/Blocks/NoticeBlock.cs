// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core.Reporting
{
    /// <summary>
    /// A one-line notice, for example "No ventilation system assigned" in place of a section with no data.
    /// </summary>
    public sealed class NoticeBlock : DocumentBlock
    {
        public NoticeBlock(string id, string text, NoticeLevel level = NoticeLevel.Information)
            : base(id)
        {
            Text = text;
            Level = level;
        }

        public string Text { get; }

        public NoticeLevel Level { get; }
    }

    /// <summary>
    /// A paragraph of plain text.
    /// </summary>
    public sealed class TextBlock : DocumentBlock
    {
        public TextBlock(string id, string text)
            : base(id)
        {
            Text = text;
        }

        public string Text { get; }
    }

    /// <summary>
    /// An image (PNG bytes) with an optional caption, for example a chart rendered by the host.
    /// </summary>
    public sealed class ImageBlock : DocumentBlock
    {
        private readonly byte[] png;

        public ImageBlock(string id, byte[] png, string caption = null, double widthFraction = 1.0)
            : base(id)
        {
            this.png = png == null ? null : (byte[])png.Clone();
            Caption = caption;
            WidthFraction = widthFraction <= 0 || widthFraction > 1 ? 1.0 : widthFraction;
        }

        /// <summary>
        /// Image bytes (PNG). Returns a copy.
        /// </summary>
        public byte[] Png => png == null ? null : (byte[])png.Clone();

        public string Caption { get; }

        /// <summary>
        /// Width as a fraction of the available width, in (0, 1].
        /// </summary>
        public double WidthFraction { get; }
    }
}
