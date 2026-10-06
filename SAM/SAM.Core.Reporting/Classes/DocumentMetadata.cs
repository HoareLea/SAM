// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core.Reporting
{
    /// <summary>
    /// Document header metadata. Title and subject come from the document definition and its data; the project
    /// fields come from the host (<see cref="DocumentOptions.Metadata"/>).
    /// </summary>
    public sealed class DocumentMetadata
    {
        public DocumentMetadata()
        {
        }

        public DocumentMetadata(DocumentMetadata documentMetadata)
        {
            if (documentMetadata == null)
            {
                return;
            }

            Title = documentMetadata.Title;
            Subject = documentMetadata.Subject;
            ProjectName = documentMetadata.ProjectName;
            ProjectNumber = documentMetadata.ProjectNumber;
            PreparedBy = documentMetadata.PreparedBy;
            Date = documentMetadata.Date;
        }

        /// <summary>
        /// Document title, for example "Space Assumptions".
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// What the document is about, for example the space name.
        /// </summary>
        public string Subject { get; set; }

        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }

        public string PreparedBy { get; set; }

        public DateTime? Date { get; set; }
    }
}
