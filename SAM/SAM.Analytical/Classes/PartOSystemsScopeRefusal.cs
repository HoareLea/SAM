// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One structured reason <c>Query.PartOSystemsMaterialisationScope</c> took no scope: a machine-readable
    /// <see cref="Reason"/>, the identities and values it concerns, and a sentence an engineer can act on.
    /// <para>
    /// The fields are the ones the message is written from, so a caller that must word the refusal for its own stage
    /// (Iteration 3 keeps its established wording) words it from these and never re-derives the rule.
    /// </para>
    /// </summary>
    public class PartOSystemsScopeRefusal
    {
        internal PartOSystemsScopeRefusal(
            PartOSystemsScopeRefusalReason partOSystemsScopeRefusalReason,
            string message,
            Guid guid_VentilationSystem = default,
            VentilationSystem ventilationSystem = null,
            VentilationTerminal ventilationTerminal = null,
            Space space = null)
        {
            Reason = partOSystemsScopeRefusalReason;
            Message = message;
            Guid_VentilationSystem = guid_VentilationSystem;
            Name_VentilationSystem = ventilationSystem?.Name;
            FullName_VentilationSystem = ventilationSystem?.FullName;

            if (ventilationTerminal is not null)
            {
                Guid_VentilationTerminal = ventilationTerminal.Guid;
                Name_VentilationTerminal = ventilationTerminal.Name;
                FlowClassification = ventilationTerminal.FlowClassification;
                DesignFlowRate_Lps = ventilationTerminal.DesignFlowRate_Lps;
            }

            if (space is not null)
            {
                Guid_Space = space.Guid;
                Name_Space = space.Name;
            }
        }

        public PartOSystemsScopeRefusalReason Reason { get; }

        /// <summary>The ventilation system concerned, or <see cref="Guid.Empty"/> where the refusal concerns the inputs.</summary>
        public Guid Guid_VentilationSystem { get; }

        /// <summary>The system's <see cref="MechanicalSystem.Name"/> - its type name, for example <c>MV</c>.</summary>
        public string Name_VentilationSystem { get; }

        /// <summary>The system's <see cref="MechanicalSystem.FullName"/> as an engineer sees it, for example <c>MV 1</c>.</summary>
        public string FullName_VentilationSystem { get; }

        /// <summary>The design terminal carrying the duty, or <see cref="Guid.Empty"/>.</summary>
        public Guid Guid_VentilationTerminal { get; }

        public string Name_VentilationTerminal { get; }

        public FlowClassification FlowClassification { get; } = FlowClassification.Undefined;

        /// <summary>The terminal's stated design airflow, exactly as stated (null where it states none).</summary>
        public double? DesignFlowRate_Lps { get; }

        /// <summary>The room the duty is in, or <see cref="Guid.Empty"/>.</summary>
        public Guid Guid_Space { get; }

        public string Name_Space { get; }

        public string Message { get; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Reason, Message);
        }
    }
}
