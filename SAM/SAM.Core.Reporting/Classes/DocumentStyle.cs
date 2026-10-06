// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Renderer-neutral branding: accent colour, font family and logos. A renderer maps these onto its own styles.
    /// </summary>
    public sealed class DocumentStyle
    {
        public DocumentStyle(string accentColor = "#3C8A3E", string fontFamily = "Noto Sans", bool showSamLogo = true, byte[] companyLogoPng = null)
        {
            AccentColor = accentColor;
            FontFamily = fontFamily;
            ShowSamLogo = showSamLogo;
            this.companyLogoPng = companyLogoPng == null ? null : (byte[])companyLogoPng.Clone();
        }

        public static DocumentStyle Default { get; } = new DocumentStyle();

        /// <summary>
        /// Accent colour as #RRGGBB.
        /// </summary>
        public string AccentColor { get; }

        public string FontFamily { get; }

        public bool ShowSamLogo { get; }

        /// <summary>
        /// Optional company logo (PNG bytes). Returns a copy.
        /// </summary>
        public byte[] CompanyLogoPng => companyLogoPng == null ? null : (byte[])companyLogoPng.Clone();

        private readonly byte[] companyLogoPng;
    }
}
