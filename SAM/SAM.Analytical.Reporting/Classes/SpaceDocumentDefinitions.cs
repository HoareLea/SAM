// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// The space document types.
    /// </summary>
    public static class SpaceDocumentDefinitions
    {
        /// <summary>
        /// Phase 1 Space Assumptions: identity, geometry, design criteria, internal condition, ventilation, systems,
        /// fabric and persisted Tas design loads, then the provenance footer. Built from the saved model only.
        /// The order and the <see cref="SectionWidth"/> hints give the approved v2 layout
        /// (documentation/Reporting-LAYOUT.md): consecutive half-width sections are paired side by side.
        /// </summary>
        public static DocumentDefinition<SpaceDocumentData> SpaceAssumptions { get; } = new DocumentDefinition<SpaceDocumentData>(
            "space-assumptions",
            "Space Assumptions",
            DocumentScope.Space,
            new ISectionBuilder<SpaceDocumentData>[]
            {
                new SpaceIdentitySectionBuilder(),
                new SpaceGeometrySectionBuilder(),
                new SpaceDesignCriteriaSectionBuilder(),
                new SpaceInternalConditionSectionBuilder(),
                new SpaceVentilationSectionBuilder(),
                new SpaceSystemsSectionBuilder(),
                new SpaceFabricSectionBuilder(),
                new SpaceSizingSectionBuilder(),
            },
            new SpaceFooterBuilder(),
            data => data.Identity.Name.TryGetValue(out string name) ? name : data.Identity.Guid.ToString("D"));

        /// <summary>
        /// Phase 2 Space Design Load Summary: the Phase-1 identity, design criteria and sizing sections, then heating
        /// and cooling, each with its design-day and full-year peaks side by side and never merged, then the results
        /// status and the Phase-1 footer. Built from <see cref="SpaceDesignLoadDocumentData"/> only
        /// (documentation/Reporting-Phase2-ResultAuthority.md §9.1).
        /// </summary>
        public static DocumentDefinition<SpaceDesignLoadDocumentData> SpaceDesignLoadSummary { get; } = new DocumentDefinition<SpaceDesignLoadDocumentData>(
            "space-design-load-summary",
            "Space Design Load Summary",
            DocumentScope.Space,
            new ISectionBuilder<SpaceDesignLoadDocumentData>[]
            {
                new SpaceDesignLoadPhase1SectionBuilder(new SpaceIdentitySectionBuilder()),
                new SpaceDesignLoadPhase1SectionBuilder(new SpaceDesignCriteriaSectionBuilder()),
                new SpaceDesignLoadPhase1SectionBuilder(new SpaceSizingSectionBuilder()),
                new SpaceLoadResultSectionBuilder(LoadType.Heating),
                new SpaceLoadResultSectionBuilder(LoadType.Cooling),
                new SpaceLoadResultsSectionBuilder(),
            },
            new SpaceDesignLoadFooterBuilder(),
            data => data.Identity.Name.TryGetValue(out string name) ? name : data.Identity.Guid.ToString("D"));
    }
}
