// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// A document type: its identity, the ordered section builders, the footer builder and how its subject is named.
    /// </summary>
    public sealed class DocumentDefinition<TData>
    {
        public DocumentDefinition(string id, string title, DocumentScope scope, IEnumerable<ISectionBuilder<TData>> sectionBuilders, IFooterBuilder<TData> footerBuilder, Func<TData, string> subject)
        {
            Id = string.IsNullOrWhiteSpace(id) ? throw new ArgumentException("A document definition needs an id.", nameof(id)) : id;
            Title = title;
            Scope = scope;
            Sections = (sectionBuilders ?? Enumerable.Empty<ISectionBuilder<TData>>()).ToList().AsReadOnly();
            FooterBuilder = footerBuilder;
            Subject = subject;

            List<string> duplicates = Sections.GroupBy(x => x.Id).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicates.Count != 0)
            {
                throw new ArgumentException(string.Format("Duplicate section id(s): {0}", string.Join(", ", duplicates)), nameof(sectionBuilders));
            }
        }

        public string Id { get; }

        public string Title { get; }

        public DocumentScope Scope { get; }

        /// <summary>
        /// Section builders in document order.
        /// </summary>
        public IReadOnlyList<ISectionBuilder<TData>> Sections { get; }

        public IFooterBuilder<TData> FooterBuilder { get; }

        public Func<TData, string> Subject { get; }
    }
}
