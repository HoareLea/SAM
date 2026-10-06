// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SAM.Analytical
{
    public static partial class Query
    {
        private static readonly Regex TM59_TrailingNumber = new Regex(@"^(.*?)[\s\-\._]*\d+\s*$", RegexOptions.Compiled);
        private static readonly Regex TM59_Punctuation = new Regex(@"[^a-z0-9\s]", RegexOptions.Compiled);
        private static readonly Regex TM59_Whitespace = new Regex(@"\s+", RegexOptions.Compiled);

        /// <summary>
        /// Normalizes a space name for deterministic TM59 keyword matching: lower-cases, strips a
        /// single trailing room number (e.g. "Kitchen4", "Bedroom 23"), and collapses hyphens,
        /// punctuation and repeated whitespace to single spaces.
        /// </summary>
        public static string TM59NormalizedName(string name)
        {
            return TM59Normalize(name, stripTrailingNumber: true);
        }

        private static string TM59Normalize(string text, bool stripTrailingNumber)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string result = text.Trim().ToLowerInvariant();

            if (stripTrailingNumber)
            {
                Match match = TM59_TrailingNumber.Match(result);
                if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    result = match.Groups[1].Value;
            }

            result = TM59_Punctuation.Replace(result, " ");
            result = TM59_Whitespace.Replace(result, " ").Trim();

            return result;
        }

        private static List<string> TM59Tokens(string normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized))
                return new List<string>();

            return normalized.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }

        /// <summary>
        /// Deterministic, whole-token/whole-phrase TM59 keyword matcher. Unlike TextMap.GetSortedKeys
        /// this never matches an alias as an accidental substring (e.g. digit "2" inside "Bedroom 23") -
        /// an alias only matches when its tokens appear as a contiguous phrase in the normalized name.
        /// Returns every match found, most specific (longest phrase) first; use TM59BestTextMapKey to
        /// resolve a single deterministic winner.
        /// </summary>
        public static List<TM59TextMapMatch> TM59TextMapMatches(this TextMap textMap, string name, IEnumerable<string> keys = null)
        {
            List<TM59TextMapMatch> result = new List<TM59TextMapMatch>();

            if (textMap == null || string.IsNullOrWhiteSpace(name))
                return result;

            IEnumerable<string> keys_Temp = keys ?? textMap.Keys;
            if (keys_Temp == null)
                return result;

            List<string> nameTokens = TM59Tokens(TM59NormalizedName(name));
            if (nameTokens.Count == 0)
                return result;

            foreach (string key in keys_Temp)
            {
                List<string> values = textMap.GetValues(key);
                if (values == null)
                    continue;

                foreach (string alias in values)
                {
                    if (string.IsNullOrWhiteSpace(alias))
                        continue;

                    string normalizedAlias = TM59Normalize(alias, stripTrailingNumber: false);
                    List<string> aliasTokens = TM59Tokens(normalizedAlias);
                    if (aliasTokens.Count == 0 || aliasTokens.Count > nameTokens.Count)
                        continue;

                    bool contains = false;
                    for (int i = 0; i <= nameTokens.Count - aliasTokens.Count; i++)
                    {
                        bool match = true;
                        for (int j = 0; j < aliasTokens.Count; j++)
                        {
                            if (nameTokens[i + j] != aliasTokens[j])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (contains)
                        result.Add(new TM59TextMapMatch(key, alias, aliasTokens.Count, normalizedAlias.Length));
                }
            }

            return result.OrderByDescending(x => x.TokenCount).ThenByDescending(x => x.CharacterCount).ToList();
        }

        /// <summary>
        /// Resolves TM59TextMapMatches to a single deterministic winning key, ranked by phrase length
        /// (tokens, then characters). Returns null when there is no match, or when two or more distinct
        /// keys tie at the top rank (an ambiguous keyword set) - callers must not guess in that case.
        /// </summary>
        public static string TM59BestTextMapKey(this TextMap textMap, string name, IEnumerable<string> keys = null)
        {
            List<TM59TextMapMatch> matches = textMap.TM59TextMapMatches(name, keys);
            if (matches == null || matches.Count == 0)
                return null;

            int topTokenCount = matches[0].TokenCount;
            List<TM59TextMapMatch> topByTokens = matches.Where(x => x.TokenCount == topTokenCount).ToList();

            List<string> distinctKeys = topByTokens.Select(x => x.Key).Distinct().ToList();
            if (distinctKeys.Count == 1)
                return distinctKeys[0];

            int topCharacterCount = topByTokens.Max(x => x.CharacterCount);
            List<string> distinctKeys_CharacterCount = topByTokens.Where(x => x.CharacterCount == topCharacterCount).Select(x => x.Key).Distinct().ToList();

            return distinctKeys_CharacterCount.Count == 1 ? distinctKeys_CharacterCount[0] : null;
        }
    }
}
