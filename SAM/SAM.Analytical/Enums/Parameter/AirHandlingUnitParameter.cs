// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Optional data attached to an <see cref="AirHandlingUnit"/>.
    /// <para>
    /// The unit itself is the generic plant instance serving one service. Anything that identifies it as
    /// a particular product hangs off it here, which is the same arrangement
    /// <see cref="SpaceParameter.PartFSpaceData"/> and
    /// <see cref="VentilationTerminalParameter.PartFTerminalReference"/> use - so the generic unit has no
    /// member of a catalogue type, and a unit nobody has selected a product for simply carries nothing.
    /// </para>
    /// </summary>
    [AssociatedTypes(typeof(AirHandlingUnit)), Description("AirHandlingUnit Parameter")]
    public enum AirHandlingUnitParameter
    {
        /// <summary>
        /// The reusable ventilation unit product this instance has been selected to be, if any. Absent
        /// until a selection is made, which is the normal state throughout Iteration 1a.
        /// <para>
        /// <b>Identity, never capability and never duty.</b> The product's maximum airflows stay in the
        /// catalogue - see <see cref="Analytical.VentilationUnitReference"/> - and the unit's duties are
        /// derived from the design terminals of the system it supplies, never stored beside it.
        /// </para>
        /// </summary>
        [ParameterProperties("Ventilation Unit Reference", "Ventilation Unit Reference"), SAMObjectParameterValue(typeof(VentilationUnitReference))] VentilationUnitReference,
    }
}
