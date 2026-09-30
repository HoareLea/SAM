// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;

namespace SAM.Analytical
{
    /// <summary>
    /// One fact that makes authored ventilation plant duty-bearing, found by <c>Query.PartOAuthoredPlantDuty</c>: a
    /// design terminal airflow, an air movement or a selected product.
    /// </summary>
    public class PartOMechanicalDutyEvidence
    {
        internal PartOMechanicalDutyEvidence(PartOMechanicalDutyEvidenceKind kind, Guid guid, string name, Guid guid_Owner, string name_Owner, double value, string message)
        {
            Kind = kind;
            Guid = guid;
            Name = name;
            Guid_Owner = guid_Owner;
            Name_Owner = name_Owner;
            Value = value;
            Message = message;
        }

        public PartOMechanicalDutyEvidenceKind Kind { get; }

        /// <summary>The object that states the duty: the terminal, the movement, or the unit carrying the product.</summary>
        public Guid Guid { get; }

        public string Name { get; }

        /// <summary>
        /// The ventilation system or air handling unit the duty was found on. For a system's evidence this is the
        /// system itself, one of its units, or another system that shares one of its units.
        /// </summary>
        public Guid Guid_Owner { get; }

        /// <summary>The owner's engineer-facing name (<see cref="MechanicalSystem.FullName"/> for a system).</summary>
        public string Name_Owner { get; }

        /// <summary>The airflow in l/s for an airflow kind; NaN otherwise.</summary>
        public double Value { get; }

        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
