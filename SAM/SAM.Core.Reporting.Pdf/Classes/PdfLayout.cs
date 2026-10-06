// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using MigraDoc.DocumentObjectModel;
using System.Globalization;

namespace SAM.Core.Reporting.Pdf
{
    /// <summary>
    /// Page geometry, type sizes and colours of the default PDF (documentation/Reporting-LAYOUT.md, approved v2).
    /// </summary>
    internal static class PdfLayout
    {
        // Page: A4 portrait, 15 mm margins. The bottom margin also holds the footer.
        public const double PageWidth = 210;
        public const double PageHeight = 297;
        public const double Margin = 15;
        public const double BottomMargin = 26;
        public const double FooterDistance = 10;
        public const double HeaderDistance = 8;

        public const double ContentWidth = PageWidth - (2 * Margin);

        /// <summary>
        /// Height available to the body on a page, in mm.
        /// </summary>
        public const double BodyHeight = PageHeight - Margin - BottomMargin;

        /// <summary>
        /// Gap between side-by-side sections and columns, in mm.
        /// </summary>
        public const double Gap = 6;

        /// <summary>
        /// Side-by-side content taller than this fraction of the body height is stacked instead, so it can break
        /// across pages (a side-by-side row is placed as one unit).
        /// </summary>
        public const double MaxSideBySideFraction = 0.5;

        // Type sizes, in pt.
        public const double Title = 16;
        public const double Subject = 13;
        public const double Body = 9;
        public const double SectionHeading = 8.5;
        public const double BlockHeading = 7.5;
        public const double TableHeader = 7.5;
        public const double SubLabel = 7;
        public const double NoticeText = 8;
        public const double Note = 7;
        public const double Footer = 7;
        public const double Mark = 11;

        // Unit column of key/value blocks, in mm: a fixed width so numbers align down a block. Side-by-side blocks
        // (the three-column row) size it to their widest unit, from this minimum.
        public const double UnitColumn = 15;
        public const double UnitColumnNarrow = 6;

        /// <summary>
        /// Horizontal cell padding, in mm.
        /// </summary>
        public const double CellPadding = 0.8;

        /// <summary>
        /// Vertical cell padding, in mm.
        /// </summary>
        public const double RowPadding = 0.3;

        public static readonly Color Text = Color.FromRgb(0x1F, 0x23, 0x28);
        public static readonly Color Muted = Color.FromRgb(0x6B, 0x72, 0x80);
        public static readonly Color Rule = Color.FromRgb(0xDD, 0xE1, 0xE4);
        public static readonly Color HeaderRule = Color.FromRgb(0xB8, 0xBE, 0xC4);
        public static readonly Color NoticeShading = Color.FromRgb(0xF1, 0xF4, 0xF1);
        public static readonly Color WarningShading = Color.FromRgb(0xFD, 0xF5, 0xE3);
        public static readonly Color WarningBar = Color.FromRgb(0xC7, 0x77, 0x00);
        public static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);

        public static readonly Color DefaultAccent = Color.FromRgb(0x3C, 0x8A, 0x3E);

        /// <summary>
        /// Parses "#RRGGBB"; anything else gives the default accent.
        /// </summary>
        public static Color Accent(string hex)
        {
            if (!string.IsNullOrWhiteSpace(hex))
            {
                string value = hex.Trim().TrimStart('#');
                if (value.Length == 6 && uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rgb))
                {
                    return Color.FromRgb((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
                }
            }

            return DefaultAccent;
        }
    }
}
