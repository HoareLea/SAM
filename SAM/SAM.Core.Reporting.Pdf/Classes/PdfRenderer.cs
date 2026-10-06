// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.IO;

namespace SAM.Core.Reporting.Pdf
{
    /// <summary>
    /// Renders a <see cref="Document"/> to PDF (A4 portrait, Noto Sans) with MigraDoc + PDFsharp. It lays out the
    /// formatted text and units it is given; it never converts units and never reads engineering data. Content is
    /// never shrunk, truncated or clipped to fit a page: long content continues on further pages.
    /// </summary>
    public sealed class PdfRenderer : IDocumentRenderer
    {
        /// <summary>
        /// The section with this id is printed in the header band under the title and subject, not as a body section.
        /// </summary>
        public const string HeaderSectionId = "identity";

        public string FileExtension => ".pdf";

        public void Render(Document document, Stream stream)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream is not writable.", nameof(stream));
            }

            NotoSansFontResolver.Register();

            MigraDoc.Rendering.PdfDocumentRenderer pdfDocumentRenderer = new MigraDoc.Rendering.PdfDocumentRenderer()
            {
                Document = MigraDocBuilder.Build(document),
            };

            pdfDocumentRenderer.RenderDocument();
            pdfDocumentRenderer.PdfDocument.Info.Creator = "SAM";
            pdfDocumentRenderer.PdfDocument.Save(stream, false);
        }

        /// <summary>
        /// Renders to a byte array.
        /// </summary>
        public byte[] Render(Document document)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Render(document, memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
