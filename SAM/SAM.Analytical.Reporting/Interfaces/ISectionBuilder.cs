// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using System.Collections.Generic;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Builds one document section from typed data. This is the only place where engineering data meets layout:
    /// values are formatted through <see cref="DocumentContext.Formatter"/>, comparable values through one shared
    /// display unit.
    /// </summary>
    public interface ISectionBuilder<TData>
    {
        /// <summary>
        /// Stable section identifier, for example "geometry".
        /// </summary>
        string Id { get; }

        DocumentSection Build(TData data, DocumentContext documentContext);
    }

    /// <summary>
    /// Builds the provenance/status lines of the document footer. The legend is added by
    /// <see cref="Create.Document{TData}(DocumentDefinition{TData}, TData, DocumentContext)"/> from the markers
    /// actually used.
    /// </summary>
    public interface IFooterBuilder<TData>
    {
        IEnumerable<string> Build(TData data, DocumentContext documentContext);
    }
}
