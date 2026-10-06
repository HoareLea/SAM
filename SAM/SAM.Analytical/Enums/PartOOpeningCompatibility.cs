// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical.Enums
{
    /// <summary>
    /// Whether a model's authored opening behaviour satisfies the <c>Openings Restricted</c> assumption a
    /// Part O mitigation stage states.
    /// <para>
    /// <b>Three values, not a boolean, and that is the whole point.</b> "The model disagrees with the stage"
    /// and "SAM cannot tell whether the model agrees with the stage" are different facts, and collapsing them
    /// into one boolean is how an unclassifiable opening quietly gets treated as an unrestricted one. Both
    /// are reported - but for different reasons, and the modeller is told which.
    /// </para>
    /// <para>
    /// This is a statement about the MODEL against a stage. It is never a licence to change the model: an
    /// <see cref="OpeningRestriction"/> is authored building data, and a compliance stage is a label the
    /// result is attributed under. Where they disagree, one of the two is wrong and only the modeller knows
    /// which - so this reports and never acts. It is advisory: opening behaviour is orthogonal to the
    /// mitigation stage, and a base case may legitimately mix restricted and unrestricted openings.
    /// </para>
    /// </summary>
    [Description("Part O Opening Compatibility.")]
    public enum PartOOpeningCompatibility
    {
        /// <summary>
        /// Every operable opening in the model was positively classified, and every one of them agrees with
        /// the stage's assumption. A model with no operable opening at all is also compatible: the stage's
        /// opening assumption has no subject to contradict.
        /// </summary>
        Compatible,

        /// <summary>
        /// At least one opening was positively classified and positively disagrees - a restricted opening
        /// under a stage that assumes openings are operated without restriction, or a model that restricts
        /// nothing under a stage that assumes openings are restricted. This is a proven disagreement, not a
        /// suspicion - though today's stage-asserted <see cref="Query.OpeningsRestricted"/> is itself the
        /// thing under revision, so this is reported and not acted on.
        /// </summary>
        Incompatible,

        /// <summary>
        /// At least one opening states availability behaviour that SAM cannot deterministically classify as
        /// restricted or unrestricted - today, a legacy general-valued
        /// <see cref="ProfileOpeningProperties.Profile"/> with no first-class
        /// <see cref="ProfileOpeningProperties.Schedule"/> beside it. Guessing at it would put an
        /// unreviewed inference inside a compliance identity, so it is reported as unknown instead.
        /// </summary>
        Unknown
    }
}
