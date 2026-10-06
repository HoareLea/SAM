// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Typed engineering data for one space, collected from a saved analytical model by
    /// <see cref="Create.SpaceDocumentData(DocumentContext, Space)"/>. Values are in SAM canonical units; formatting
    /// happens later in the section builders.
    /// <para>
    /// Phase 1 is assumptions-first: nothing here is read from <c>SpaceSimulationResult</c> or a TSD.
    /// </para>
    /// </summary>
    public sealed class SpaceDocumentData
    {
        public SpaceIdentityData Identity { get; init; }

        public SpaceGeometryData Geometry { get; init; }

        public SpaceOccupancyData Occupancy { get; init; }

        public SpaceGainData Lighting { get; init; }

        public SpaceGainData EquipmentSensible { get; init; }

        public SpaceGainData EquipmentLatent { get; init; }

        public SpaceInfiltrationData Infiltration { get; init; }

        public SpaceDesignCriteriaData DesignCriteria { get; init; }

        public SpaceVentilationData Ventilation { get; init; }

        public SpaceSystemsData Systems { get; init; }

        public SpaceFabricData Fabric { get; init; }

        public SpaceSizingData Sizing { get; init; }

        public DocumentProvenance Provenance { get; init; }
    }
}
