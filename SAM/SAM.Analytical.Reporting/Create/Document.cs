// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    public static partial class Create
    {
        public const string LegendNotAvailable = "— not available";
        public const string LegendNotApplicable = "n/a not applicable";
        public const string LegendOutOfDate = "† out of date (model changed since the source run)";

        /// <summary>
        /// Builds a renderer-neutral document: runs each section builder in order, then the footer builder, and adds a
        /// legend entry only for markers that appear in the document. An exception from any builder propagates: a
        /// software failure aborts the whole document rather than producing a partial one.
        /// </summary>
        public static Document Document<TData>(DocumentDefinition<TData> documentDefinition, TData data, DocumentContext documentContext)
        {
            if (documentDefinition == null)
            {
                throw new ArgumentNullException(nameof(documentDefinition));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (documentContext == null)
            {
                throw new ArgumentNullException(nameof(documentContext));
            }

            List<DocumentSection> documentSections = new List<DocumentSection>();
            foreach (ISectionBuilder<TData> sectionBuilder in documentDefinition.Sections)
            {
                DocumentSection documentSection = sectionBuilder.Build(data, documentContext);
                if (documentSection == null)
                {
                    throw new InvalidOperationException(string.Format("Section builder {0} returned no section.", sectionBuilder.Id));
                }

                documentSections.Add(documentSection);
            }

            DocumentMetadata documentMetadata = new DocumentMetadata(documentContext.Options.Metadata)
            {
                Title = documentDefinition.Title,
                Subject = documentDefinition.Subject?.Invoke(data),
            };

            if (documentMetadata.Date == null)
            {
                documentMetadata.Date = documentContext.Provenance.GeneratedAt.Date;
            }

            Document document = new Document(documentDefinition.Id, documentMetadata, documentSections, null, documentContext.Options.Style);

            List<FormattedValue> formattedValues = document.FormattedValues().ToList();
            List<string> legend = new List<string>();
            // The legend explains the markers printed, so a not-applicable value with its own text ("not set") adds none.
            IQuantityFormatter quantityFormatter = documentContext.Formatter;
            if (formattedValues.Any(x => x.Availability == Availability.NotAvailable && x.Text == quantityFormatter.NotAvailableText))
            {
                legend.Add(LegendNotAvailable);
            }

            if (formattedValues.Any(x => x.Availability == Availability.NotApplicable && x.Text == quantityFormatter.NotApplicableText))
            {
                legend.Add(LegendNotApplicable);
            }

            if (formattedValues.Any(x => x.IsOutOfDate))
            {
                legend.Add(LegendOutOfDate);
            }

            IEnumerable<string> lines = documentDefinition.FooterBuilder?.Build(data, documentContext);

            return new Document(documentDefinition.Id, documentMetadata, documentSections, new DocumentFooter(lines, legend), documentContext.Options.Style);
        }

        /// <summary>
        /// Collects one space and builds its Space Assumptions document.
        /// </summary>
        public static Document SpaceAssumptions(DocumentContext documentContext, Space space)
        {
            SpaceDocumentData spaceDocumentData = SpaceDocumentData(documentContext, space);

            return Document(SpaceDocumentDefinitions.SpaceAssumptions, spaceDocumentData, documentContext);
        }

        /// <summary>
        /// Collects one space and builds its Space Design Load Summary document.
        /// </summary>
        /// <param name="resultSource">
        /// Passed to <see cref="SpaceDesignLoadDocumentData(DocumentContext, Space, string)"/>: only results with this
        /// source are read. Null reads every source, and more than one candidate result stays ambiguous in the document.
        /// </param>
        public static Document SpaceDesignLoadSummary(DocumentContext documentContext, Space space, string resultSource = null)
        {
            SpaceDesignLoadDocumentData spaceDesignLoadDocumentData = SpaceDesignLoadDocumentData(documentContext, space, resultSource);

            return Document(SpaceDocumentDefinitions.SpaceDesignLoadSummary, spaceDesignLoadDocumentData, documentContext);
        }
    }
}
