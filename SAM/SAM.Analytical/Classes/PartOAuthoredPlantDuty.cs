// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The effective-duty classification of one authored ventilation system or air handling unit
    /// (<c>Query.PartOAuthoredPlantDuty</c>). Plant with no evidence is <b>inert</b>: template scaffolding such as the
    /// <c>MV 1</c> / <c>AHU1</c> <c>Modify.AddMechanicalSystems</c> creates. It stays on the design and is left out of
    /// the Part O assessment. Plant with any evidence is <b>active</b> and keeps every existing refusal.
    /// </summary>
    public class PartOAuthoredPlantDuty
    {
        internal PartOAuthoredPlantDuty(Guid guid, string name, List<string> names_Unit, List<PartOMechanicalDutyEvidence> evidence)
        {
            Guid = guid;
            Name = name;
            UnitNames = names_Unit ?? [];
            Evidence = evidence ?? [];
        }

        /// <summary>The system's or unit's guid.</summary>
        public Guid Guid { get; }

        /// <summary>The engineer-facing name: <see cref="MechanicalSystem.FullName"/> (<c>MV 1</c>) or the unit's name (<c>AHU1</c>).</summary>
        public string Name { get; }

        /// <summary>
        /// For a system, the units it names (supply, then exhaust) that exist on the model. For a unit, empty.
        /// </summary>
        public IReadOnlyList<string> UnitNames { get; }

        /// <summary>Why the plant is duty-bearing, in a stable order. Empty for inert plant.</summary>
        public IReadOnlyList<PartOMechanicalDutyEvidence> Evidence { get; }

        /// <summary>True where nothing states duty: no design airflow, no air movement, no selected product.</summary>
        public bool IsInert => Evidence.Count == 0;

        public override string ToString()
        {
            return IsInert ? string.Format("{0}: inert", Name) : string.Format("{0}: active ({1})", Name, string.Join("; ", Evidence));
        }
    }
}
