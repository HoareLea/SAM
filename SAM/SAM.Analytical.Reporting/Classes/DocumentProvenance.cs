// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Where a document's data came from, for the status footer.
    /// </summary>
    public sealed class DocumentProvenance
    {
        public DocumentProvenance(string modelName, string softwareVersion, DateTime generatedAt)
        {
            ModelName = modelName;
            SoftwareVersion = softwareVersion;
            GeneratedAt = generatedAt;
        }

        /// <summary>
        /// Name of the analytical model, or null when the model has none.
        /// </summary>
        public string ModelName { get; }

        public string SoftwareVersion { get; }

        public DateTime GeneratedAt { get; }
    }
}
