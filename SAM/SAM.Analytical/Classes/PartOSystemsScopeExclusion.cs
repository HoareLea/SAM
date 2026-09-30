// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One authored ventilation system <c>Query.PartOSystemsMaterialisationScope</c> left out of the Systems
    /// materialisation input because it states no effective mechanical duty. It stays on the design and in the thermal
    /// model; it is only not handed to SAM_Systems.
    /// </summary>
    public class PartOSystemsScopeExclusion
    {
        internal PartOSystemsScopeExclusion(VentilationSystem ventilationSystem, int count_VentilationTerminal, string message)
        {
            Guid_VentilationSystem = ventilationSystem?.Guid ?? Guid.Empty;
            Name_VentilationSystem = ventilationSystem?.Name;
            FullName_VentilationSystem = ventilationSystem?.FullName;
            Count_VentilationTerminal = count_VentilationTerminal;
            Message = message;
        }

        public Guid Guid_VentilationSystem { get; }

        /// <summary>The system's <see cref="MechanicalSystem.Name"/> - its type name, for example <c>NV</c>.</summary>
        public string Name_VentilationSystem { get; }

        /// <summary>The system's <see cref="MechanicalSystem.FullName"/> as an engineer sees it, for example <c>NV 1</c>.</summary>
        public string FullName_VentilationSystem { get; }

        /// <summary>How many design terminals the system carries - none of which states an effective design airflow.</summary>
        public int Count_VentilationTerminal { get; }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
