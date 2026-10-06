// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Optional data attached to a <see cref="VentilationTerminal"/>.
    /// <para>
    /// The terminal itself is generic - it is a supply or extract terminal with a design duty and,
    /// one day, a place. Anything standard-specific hangs off it here, which is the same arrangement
    /// <see cref="SpaceParameter.PartFSpaceData"/> uses to attach Approved Document F data to the
    /// generic <see cref="Space"/> without <see cref="Space"/> knowing what Part F is.
    /// </para>
    /// </summary>
    [AssociatedTypes(typeof(VentilationTerminal)), Description("Ventilation Terminal Parameter")]
    public enum VentilationTerminalParameter
    {
        /// <summary>
        /// The Approved Document F requirement this terminal was created to realize, if any. Absent on
        /// a terminal that realizes no regulatory requirement - one a designer added themselves, or one
        /// belonging to a building Part F does not cover.
        /// </summary>
        [ParameterProperties("PartF Terminal Reference", "PartF Terminal Reference"), SAMObjectParameterValue(typeof(PartFTerminalReference))] PartFTerminalReference,
    }
}
