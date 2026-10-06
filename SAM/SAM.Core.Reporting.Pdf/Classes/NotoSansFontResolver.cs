// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace SAM.Core.Reporting.Pdf
{
    /// <summary>
    /// PDFsharp font resolver serving the embedded Noto Sans (SIL OFL 1.1) regular and bold faces, so the PDF does
    /// not depend on the fonts installed on the machine. Italic is simulated. Any other family is passed to the
    /// PDFsharp platform resolver, so other PDFsharp users in the same process keep their installed fonts; a family
    /// the platform cannot supply falls back to Noto Sans instead of failing.
    /// </summary>
    public sealed class NotoSansFontResolver : IFontResolver
    {
        public const string FamilyName = "Noto Sans";

        private const string FaceRegular = "NotoSans#Regular";
        private const string FaceBold = "NotoSans#Bold";

        private static readonly object lockObject = new object();

        private NotoSansFontResolver()
        {
        }

        public static NotoSansFontResolver Instance { get; } = new NotoSansFontResolver();

        /// <summary>
        /// Installs <see cref="Instance"/> as the PDFsharp global font resolver. PDFsharp allows one global resolver
        /// per process, set before the first font is used. If another resolver is already installed it is kept when
        /// it can resolve Noto Sans; otherwise an <see cref="InvalidOperationException"/> explains the conflict.
        /// </summary>
        public static void Register()
        {
            lock (lockObject)
            {
                IFontResolver fontResolver = GlobalFontSettings.FontResolver;
                if (fontResolver == Instance)
                {
                    return;
                }

                if (fontResolver == null)
                {
                    GlobalFontSettings.FontResolver = Instance;
                    return;
                }

                if (fontResolver.ResolveTypeface(FamilyName, false, false) == null)
                {
                    throw new InvalidOperationException(string.Format("Another PDFsharp font resolver ({0}) is installed for this process and cannot resolve '{1}'. Install NotoSansFontResolver.Instance, or let the other resolver serve '{1}'.", fontResolver.GetType().FullName, FamilyName));
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (string.Equals(familyName, FamilyName, StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo(bold ? FaceBold : FaceRegular, false, italic);
            }

            // Any other family: an installed font where PDFsharp can use one, else Noto Sans. MigraDoc asks for its
            // predefined error font (Courier New) on every render, which must not fail on a machine without it.
            return PlatformFontResolver.ResolveTypeface(familyName, bold, italic) ?? new FontResolverInfo(bold ? FaceBold : FaceRegular, false, italic);
        }

        public byte[] GetFont(string faceName)
        {
            switch (faceName)
            {
                case FaceRegular:
                    return Resource("NotoSans-Regular.ttf");

                case FaceBold:
                    return Resource("NotoSans-Bold.ttf");
            }

            return null;
        }

        private static byte[] Resource(string fileName)
        {
            string name = "SAM.Core.Reporting.Pdf.Fonts." + fileName;
            using (Stream stream = typeof(NotoSansFontResolver).GetTypeInfo().Assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(string.Format("Embedded font resource '{0}' is missing.", name));
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
