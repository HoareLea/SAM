// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// What an Approved Document O assessment is about: a dwelling, or a common space that belongs to no
    /// dwelling.
    /// <para>
    /// These are the two outputs of <c>Query.PartOClassifyAssessmentZones</c>, named so a scenario or a
    /// result can say which of them it concerns. <b>Both are assessed.</b> A communal corridor is common
    /// space because it belongs to no flat, not because nothing needs to be said about it - TM59 has a
    /// corridor criterion of its own. What must never happen is a corridor being attributed to a dwelling.
    /// </para>
    /// <para>
    /// The scope is half of a scenario's identity, the other half being the design zone's guid. It is
    /// carried explicitly rather than inferred from the zone, so a corridor scenario and a dwelling
    /// scenario over the same zone can never derive the same key by accident.
    /// </para>
    /// </summary>
    public enum PartOAssessmentScope
    {
        /// <summary>
        /// No scope has been stated. A scenario in this state describes nothing assessable and
        /// <c>OverheatingScenario.IsValid</c> is false.
        /// </summary>
        Undefined,

        /// <summary>
        /// A dwelling, exactly as <c>Query.PartFDwellingZones</c> selects one - the single source of truth
        /// for what a dwelling is.
        /// </summary>
        Dwelling,

        /// <summary>
        /// A communal corridor, stair or landlord area: assessed in its own right, attributed to no
        /// dwelling.
        /// </summary>
        CommonSpace
    }
}
