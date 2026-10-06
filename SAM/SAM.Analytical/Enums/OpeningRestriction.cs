// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Whether a Part O opening may be used for overheating ventilation, and when.
    /// <para>
    /// A closed set rather than independent booleans, so an opening cannot state a contradictory
    /// combination (e.g. "restricted at night" and "restricted always" at once) and so this says nothing
    /// about <i>why</i> an opening is restricted (acoustic, security, or otherwise) - only that it is.
    /// Provenance, if ever needed, belongs on a separate value with a real consumer, not folded in here.
    /// </para>
    /// </summary>
    [Description("Opening Restriction.")]
    public enum OpeningRestriction
    {
        /// <summary>
        /// The opening may be used for overheating ventilation at any hour. The default, and the legacy
        /// behaviour for any <c>PartOOpeningProperties</c> serialised before this member existed.
        /// </summary>
        [Description("Unrestricted")] Unrestricted,

        /// <summary>
        /// The opening remains physically openable but is unavailable for overheating ventilation during
        /// its closed hours (e.g. a bedroom window restricted for noise overnight, or an internal door kept
        /// closed at night). The available/unavailable hours are a Part O modelling preset, not a
        /// universal regulatory value.
        /// </summary>
        [Description("Night Closed")] NightClosed,

        /// <summary>
        /// The opening does not contribute to overheating ventilation at any hour. The opening may remain
        /// physically present and openable for other purposes (e.g. purge ventilation); this states only
        /// that it takes no part in the overheating strategy.
        /// </summary>
        [Description("Always Closed")] AlwaysClosed,
    }
}
