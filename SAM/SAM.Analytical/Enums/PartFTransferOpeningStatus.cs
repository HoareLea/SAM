// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What the model actually shows about the physical transfer opening on one internal route, as a
    /// single value a drawing can be styled from.
    /// <para>
    /// It exists to stop a calculated flow from implying a physical opening. SAM can conserve air across
    /// a dwelling and produce an exact litres-per-second figure for a route where the model carries no
    /// door and no recorded transfer device at all. Drawn the same way as a route through a real door,
    /// that number would tell a reader the air has somewhere to go, which is precisely what has not been
    /// established.
    /// </para>
    /// <para>
    /// Deliberately a <b>derived</b> value, not a stored one, and deliberately separate from
    /// <see cref="PartFTransferRouteStatus"/>. That enum answers a different question - how the FLOW was
    /// allocated between parallel paths - and a route can perfectly well run through a modelled door and
    /// still have an ambiguous split. Both axes are kept; this one collapses them in the single direction
    /// the graphics need, worst case first.
    /// </para>
    /// </summary>
    public enum PartFTransferOpeningStatus
    {
        /// <summary>Nothing has been assessed for this route.</summary>
        [Description("Not Assessed")] NotAssessed,

        /// <summary>
        /// A person has recorded the provided undercut or free area and it meets paragraph 1.25. The
        /// opening is established, not inferred.
        /// </summary>
        [Description("Confirmed Opening")] ConfirmedOpening,

        /// <summary>
        /// A door aperture is modelled on the separating element, and the flow was calculated through it.
        /// The opening exists in the model; whether it has the paragraph 1.25 free area is a separate
        /// question, answered by the undercut assessment.
        /// </summary>
        [Description("Calculated Via Modelled Door")] CalculatedViaModelledDoor,

        /// <summary>
        /// No door aperture is modelled, but a transfer device is recorded - a grille, a permanent
        /// opening, an open passage. The flow was calculated through that.
        /// </summary>
        [Description("Calculated Via Permanent Opening")] CalculatedViaPermanentOpening,

        /// <summary>
        /// The two spaces adjoin and air has to move between them, but the model carries no door aperture
        /// and no recorded transfer device. A flow may still have been calculated by conservation; it must
        /// never be drawn or reported as though an opening had been found.
        /// </summary>
        [Description("Missing Transfer Opening")] MissingTransferOpening,

        /// <summary>
        /// An opening exists, but the flow through this particular one is not fixed by the dwelling's
        /// topology - parallel paths share the route, or none could be resolved at all.
        /// </summary>
        [Description("Ambiguous Route")] AmbiguousRoute,
    }
}
