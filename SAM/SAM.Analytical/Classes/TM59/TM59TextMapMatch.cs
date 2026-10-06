// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    /// <summary>
    /// A single deterministic keyword match produced by Query.TM59TextMapMatches
    /// </summary>
    public class TM59TextMapMatch
    {
        /// <summary>TextMap key (e.g. an InternalCondition name) that matched</summary>
        public string Key { get; }

        /// <summary>The alias phrase, as written in the TextMap, that matched</summary>
        public string Alias { get; }

        /// <summary>Number of whitespace-separated tokens in the matched alias phrase</summary>
        public int TokenCount { get; }

        /// <summary>Character length of the normalized alias phrase (tie-break after TokenCount)</summary>
        public int CharacterCount { get; }

        public TM59TextMapMatch(string key, string alias, int tokenCount, int characterCount)
        {
            Key = key;
            Alias = alias;
            TokenCount = tokenCount;
            CharacterCount = characterCount;
        }
    }
}
