// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using PdfSharp.Drawing;
using System.Collections.Generic;

namespace SAM.Core.Reporting.Pdf
{
    /// <summary>
    /// Measures single-line text widths with the PDF fonts, used to size table columns to their content.
    /// </summary>
    internal sealed class TextMeasure
    {
        private const double PointsPerMillimeter = 72.0 / 25.4;

        private readonly string fontFamily;
        private readonly XGraphics xGraphics;
        private readonly Dictionary<(double, bool), XFont> fonts = new Dictionary<(double, bool), XFont>();

        public TextMeasure(string fontFamily)
        {
            this.fontFamily = fontFamily;
            xGraphics = XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);
        }

        /// <summary>
        /// Width of the text on one line, in mm.
        /// </summary>
        public double Width(string text, double size, bool bold = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (!fonts.TryGetValue((size, bold), out XFont xFont))
            {
                xFont = new XFont(fontFamily, size, bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);
                fonts[(size, bold)] = xFont;
            }

            return xGraphics.MeasureString(text, xFont).Width / PointsPerMillimeter;
        }
    }
}
