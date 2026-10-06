// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Core.Reporting;
using System;
using System.Reflection;

namespace SAM.Analytical.Reporting
{
    public static partial class Create
    {
        /// <summary>
        /// Builds the context for one document run. The caller passes a model snapshot it will not modify while the
        /// document is built.
        /// </summary>
        public static DocumentContext DocumentContext(AnalyticalModel analyticalModel, DocumentOptions documentOptions = null)
        {
            if (analyticalModel == null)
            {
                throw new ArgumentNullException(nameof(analyticalModel));
            }

            documentOptions = new DocumentOptions(documentOptions ?? new DocumentOptions());

            string softwareVersion = documentOptions.SoftwareVersion;
            if (string.IsNullOrWhiteSpace(softwareVersion))
            {
                Assembly assembly = typeof(AnalyticalModel).Assembly;
                softwareVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString();
            }

            DocumentProvenance documentProvenance = new DocumentProvenance(
                string.IsNullOrWhiteSpace(analyticalModel.Name) ? null : analyticalModel.Name,
                softwareVersion,
                documentOptions.GeneratedAt ?? DateTime.Now);

            return new DocumentContext(analyticalModel, documentOptions, new QuantityFormatter(documentOptions), documentProvenance, new Log());
        }
    }
}
