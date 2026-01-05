// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(AdjacencyClusterSimulationResult)), Description("Analytical AdjacencyCluster Simulation Result Parameter")]
    public enum AdjacencyClusterSimulationResultParameter
    {
        [ParameterProperties("Load Type", "Load Type"), ParameterValue(Core.ParameterType.String)] LoadType,

        [ParameterProperties("Unmet Hours", "Unmet Hours"), IntegerParameterValue(0, 8760)] UnmetHours,
    }
}
