// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.IO;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Writes a <see cref="Document"/> to a stream in one output format (PDF, HTML, ...). A renderer lays out the
    /// formatted blocks it is given; it never converts units and never reads engineering data.
    /// </summary>
    public interface IDocumentRenderer
    {
        /// <summary>
        /// File extension including the dot, for example ".pdf".
        /// </summary>
        string FileExtension { get; }

        void Render(Document document, Stream stream);
    }
}
