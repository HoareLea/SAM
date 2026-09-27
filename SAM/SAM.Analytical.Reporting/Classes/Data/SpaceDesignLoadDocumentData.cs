// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Typed data for the Space Design Load Summary of one space, collected by
    /// <see cref="Create.SpaceDesignLoadDocumentData(DocumentContext, Space, string)"/>. It reuses the Phase-1
    /// identity, design criteria and sizing data unchanged, and adds the heating and cooling peaks read from the
    /// stored simulation results.
    /// </summary>
    public sealed class SpaceDesignLoadDocumentData
    {
        public SpaceIdentityData Identity { get; init; }

        public SpaceDesignCriteriaData DesignCriteria { get; init; }

        public SpaceSizingData Sizing { get; init; }

        public SpaceLoadResultData Heating { get; init; }

        public SpaceLoadResultData Cooling { get; init; }

        public DocumentProvenance Provenance { get; init; }
    }
}
