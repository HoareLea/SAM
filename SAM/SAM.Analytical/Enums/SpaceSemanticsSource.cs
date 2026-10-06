// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Where a <see cref="SpaceSemantics"/> classification came from, in descending order of
    /// authority. The resolver tries each source in turn and stops at the first that resolves, so
    /// the source is also the reason the classification won - which is what SAM_UI shows the user
    /// and what makes an unexpected mapping traceable.
    /// <para>
    /// Deliberately excludes unrestricted substring matching: the highest numbered matching source
    /// is a whole-token or whole-phrase match, so a name can never be classified because it happens
    /// to contain a fragment of an alias (e.g. "Server Room" must not resolve to Living because both
    /// contain "room").
    /// </para>
    /// </summary>
    public enum SpaceSemanticsSource
    {
        /// <summary>Nothing was resolved.</summary>
        [Description("None")] None,

        /// <summary>
        /// An explicit user override held in SpaceParameter.SpaceUseOverride. Always wins, and is
        /// never second guessed by name matching.
        /// </summary>
        [Description("User Override")] UserOverride,

        /// <summary>
        /// The space carries an InternalCondition whose name is recognised, normally because the
        /// user mapped it in the SAM_UI internal condition mapping dialog. An explicit, deliberate
        /// classification, so it outranks any matching of the space's own name.
        /// </summary>
        [Description("Internal Condition")] InternalCondition,

        /// <summary>
        /// The normalised space name is exactly equal to a configured synonym. The most specific
        /// name based match available.
        /// </summary>
        [Description("Exact Synonym")] ExactSynonym,

        /// <summary>
        /// A configured synonym appears in the normalised space name as a contiguous whole-token
        /// phrase. The longest phrase wins; two different classifications tying at the top rank are
        /// treated as ambiguous and left unclassified rather than guessed at.
        /// </summary>
        [Description("Phrase Match")] PhraseMatch,

        /// <summary>
        /// Nothing resolved, or two classifications tied ambiguously. Always reported so the
        /// engineer can rename the space, extend the synonym set, or override the mapping.
        /// </summary>
        [Description("Unclassified")] Unclassified,
    }
}
