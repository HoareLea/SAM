// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Normalizes a space name for deterministic, standard-neutral semantic matching.
        /// <para>
        /// Standard-neutral entry point to the matcher introduced for CIBSE TM59 in
        /// <see cref="TM59NormalizedName(string)"/>. That implementation is shared, not duplicated,
        /// and is deliberately left untouched: SAM_Tas and the TM59 test suite pin its exact
        /// behaviour. Approved Document F, Approved Document O and SAM_UI should call these
        /// standard-neutral names so no caller has to reach into TM59-named API to classify a room.
        /// </para>
        /// </summary>
        public static string SemanticNormalizedName(string name)
        {
            return TM59NormalizedName(name);
        }

        /// <summary>
        /// Deterministic, whole-token/whole-phrase matcher. An alias only matches when its tokens
        /// appear as a contiguous phrase in the normalized name, so a name is never classified because
        /// it merely contains a fragment of an alias. Returns every match, most specific first.
        /// <para>See <see cref="SemanticNormalizedName(string)"/> for why this forwards to the
        /// TM59-named implementation.</para>
        /// </summary>
        public static List<TM59TextMapMatch> SemanticTextMapMatches(this TextMap textMap, string name, IEnumerable<string> keys = null)
        {
            return textMap.TM59TextMapMatches(name, keys);
        }

        /// <summary>
        /// Resolves <see cref="SemanticTextMapMatches"/> to a single deterministic winning key, ranked
        /// by phrase length (tokens, then characters). Returns null when nothing matched, or when two
        /// or more distinct keys tie at the top rank - callers must report that ambiguity rather than
        /// guess at it.
        /// </summary>
        public static string SemanticBestTextMapKey(this TextMap textMap, string name, IEnumerable<string> keys = null)
        {
            return textMap.TM59BestTextMapKey(name, keys);
        }
    }
}
