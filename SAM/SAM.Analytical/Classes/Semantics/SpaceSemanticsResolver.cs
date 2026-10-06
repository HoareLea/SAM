// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Resolves a <see cref="Space"/> to its shared <see cref="SpaceSemantics"/>, deterministically.
    /// This is the single room-recognition entry point for Approved Document F, Approved Document O,
    /// CIBSE TM59 and the SAM_UI internal condition mapping, so all four agree on what a space is.
    /// <para>
    /// Resolution order, highest authority first:
    /// </para>
    /// <list type="number">
    /// <item>an explicit user override on <see cref="SpaceParameter.SpaceUseOverride"/>;</item>
    /// <item>an explicit override already stored on <see cref="SpaceParameter.SpaceSemantics"/> by a
    /// previous mapping, so a space mapped once can be reused by every standard;</item>
    /// <item>the space name, matched as an exact synonym and then as a whole-token phrase;</item>
    /// <item>the space's InternalCondition name, matched the same way, where the space name resolved to
    /// nothing;</item>
    /// <item>unclassified, always reported.</item>
    /// </list>
    /// <para>
    /// An InternalCondition is deliberately NOT allowed to override a space name that resolves to
    /// something different - see the comment in ResolveCore for the evidence behind that. The conflict is
    /// reported rather than silently resolved, and an explicit override forces either answer.
    /// </para>
    /// <para>
    /// Unrestricted substring matching is deliberately absent. The previous Part F matcher used
    /// TextMap.GetSortedKeys, which scores tokens with a bidirectional Contains, so "Server Room"
    /// scored against the "living room" and "shower room" aliases on the shared token "room" and was
    /// classified from whichever tied first. Here an alias only ever matches as a whole token or a
    /// whole contiguous phrase, and two different uses tying at the top rank are reported as ambiguous
    /// rather than guessed at.
    /// </para>
    /// <para>
    /// Read-only with respect to the model: nothing here writes to a Space. Callers that want to
    /// persist the result assign it to <see cref="SpaceParameter.SpaceSemantics"/> themselves.
    /// </para>
    /// </summary>
    public class SpaceSemanticsResolver
    {
        private readonly TextMap textMap;

        private readonly List<string> keys;

        //Classification is a pure function of the name plus the overrides read off the Space, and the
        //same resolver instance is reused across a whole model, so results are cached per Guid. The
        //Name each entry was computed from is kept alongside it: a caller can only "rename" a Space by
        //constructing a new one with the same Guid (Name has no setter), and a stale entry must not
        //then be returned forever. Mirrors TM59InternalConditionResolver's cache discipline.
        private readonly Dictionary<Guid, SpaceSemantics> cache = [];

        private readonly Dictionary<Guid, string> cacheKey = [];

        /// <param name="textMap">
        /// Maps a <see cref="SpaceUse"/> name to its synonyms. Keys that are not
        /// <see cref="SpaceUse"/> values are ignored, so an unrelated TextMap cannot inject a
        /// classification that the shared vocabulary does not define.
        /// </param>
        public SpaceSemanticsResolver(TextMap textMap)
        {
            this.textMap = textMap;

            keys = [];
            foreach (string key in textMap?.Keys ?? Enumerable.Empty<string>())
            {
                if (Enum.TryParse(key, out SpaceUse _))
                {
                    keys.Add(key);
                }
            }
        }

        /// <summary>Space uses this resolver can actually resolve, given its TextMap.</summary>
        public IEnumerable<string> Keys => keys;

        /// <summary>
        /// Resolves the shared semantic classification of a space. Never returns null: an
        /// unrecognised space resolves to <see cref="SpaceUse.Undefined"/> with
        /// <see cref="SpaceSemanticsSource.Unclassified"/> and a diagnostic naming the reason.
        /// </summary>
        public SpaceSemantics Resolve(Space space)
        {
            if (space is null)
            {
                return Create.SpaceSemantics(SpaceUse.Undefined, SpaceSemanticsSource.Unclassified, null, "Space is null.");
            }

            if (cache.TryGetValue(space.Guid, out SpaceSemantics cached)
                && cacheKey.TryGetValue(space.Guid, out string cachedKey)
                && string.Equals(cachedKey, CacheKey(space), StringComparison.Ordinal))
            {
                return cached;
            }

            SpaceSemantics result = ResolveCore(space);

            cache[space.Guid] = result;
            cacheKey[space.Guid] = CacheKey(space);

            return result;
        }

        /// <summary>
        /// Everything Resolve reads off the Space, so a change to any of it invalidates the cached
        /// classification rather than being masked by it.
        /// </summary>
        private static string CacheKey(Space space)
        {
            return string.Join(
                "",
                space.Name,
                space.GetValue<string>(SpaceParameter.SpaceUseOverride),
                space.GetValue<SpaceSemantics>(SpaceParameter.SpaceSemantics)?.SpaceUse.ToString(),
                space.InternalCondition?.Name);
        }

        private SpaceSemantics ResolveCore(Space space)
        {
            //1. Explicit user override. Never second guessed by name matching.
            string @override = space.GetValue<string>(SpaceParameter.SpaceUseOverride);
            if (!string.IsNullOrWhiteSpace(@override))
            {
                if (Enum.TryParse(@override.Trim(), true, out SpaceUse spaceUse_Override))
                {
                    return Create.SpaceSemantics(spaceUse_Override, SpaceSemanticsSource.UserOverride, @override);
                }

                //A typed override that names nothing real must be reported, not silently ignored -
                //otherwise the space falls through to name matching and looks correctly classified.
                return Create.SpaceSemantics(
                    SpaceUse.Undefined,
                    SpaceSemanticsSource.Unclassified,
                    @override,
                    string.Format("Space Use Override '{0}' is not a recognised space use. Recognised values: {1}.", @override, string.Join(", ", Enum.GetNames(typeof(SpaceUse)))));
            }

            //2. A classification already stored by a previous mapping. Lets a space be mapped once and
            //   reused by Part F, Part O and TM59, which is the point of the shared layer.
            SpaceSemantics stored = space.GetValue<SpaceSemantics>(SpaceParameter.SpaceSemantics);
            if (stored is not null && stored.SpaceUse != SpaceUse.Undefined && stored.Source == SpaceSemanticsSource.UserOverride)
            {
                return new SpaceSemantics(stored);
            }

            //3. The space's own name, and 4. its InternalCondition.
            //
            //An InternalCondition is consulted, but it does NOT silently override a space name that
            //resolves to something different. An InternalCondition records a thermal condition, not a
            //room use, and it is routinely assigned in bulk: in the SAM_zoningAM example model the TM59
            //"Studio" condition is applied to spaces named Bathroom_2, Ensuite_5 and Corridor_1. Trusting
            //it over those names turned each of them into a habitable supply space, removed the only
            //extract in the flat, and left supply and extract unbalanced - a ventilation calculation must
            //not lose a wet room's extract that way. The CIBSE TM59 condition set also has no bathroom,
            //ensuite, sanitary accommodation or utility room condition at all, so it cannot express the
            //distinctions Approved Document F needs.
            //
            //So: where both resolve and they disagree, the space name wins and the conflict is reported.
            //Where only the InternalCondition resolves, it classifies the space - that is the case an
            //explicit mapping is genuinely useful for, a space whose own name means nothing. An explicit
            //Space Use Override above still beats both, and is the way to force either answer.
            SpaceSemantics fromName = Match(space.Name, SpaceSemanticsSource.ExactSynonym);

            string internalConditionName = space.InternalCondition?.Name;
            SpaceSemantics fromInternalCondition = string.IsNullOrWhiteSpace(internalConditionName)
                ? null
                : Match(internalConditionName, SpaceSemanticsSource.InternalCondition);

            bool resolved_Name = fromName is not null && fromName.SpaceUse != SpaceUse.Undefined;
            bool resolved_InternalCondition = fromInternalCondition is not null && fromInternalCondition.SpaceUse != SpaceUse.Undefined;

            SpaceUse spaceUse_Name = resolved_Name ? fromName.SpaceUse : SpaceUse.Undefined;
            SpaceUse spaceUse_InternalCondition = resolved_InternalCondition ? fromInternalCondition.SpaceUse : SpaceUse.Undefined;

            bool sourcesDiffer = resolved_Name && resolved_InternalCondition && spaceUse_Name != spaceUse_InternalCondition;

            //Not every disagreement is a genuine conflict: one source can be a more specific refinement
            //of the other (e.g. Circulation / Communal Circulation say the same thing at different
            //specificity, not two different things). That resolves to the more specific value, whichever
            //source produced it, with no conflict reported.
            if (sourcesDiffer)
            {
                if (Query.IsCompatibleSpaceUseRefinement(spaceUse_InternalCondition, spaceUse_Name))
                {
                    fromName.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                    return fromName;
                }

                if (Query.IsCompatibleSpaceUseRefinement(spaceUse_Name, spaceUse_InternalCondition))
                {
                    fromInternalCondition.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                    return fromInternalCondition;
                }
            }

            bool conflict = sourcesDiffer;

            if (conflict)
            {
                SpaceSemantics result_Conflict = Create.SpaceSemantics(
                    fromName.SpaceUse,
                    fromName.Source,
                    fromName.MatchedAlias,
                    string.Format("CONFLICT: the space name indicates {0} but the internal condition '{1}' indicates {2}. The space name has been used, because it is the higher-priority source: an internal condition records a thermal condition rather than a room use and is often assigned in bulk across a whole flat. Both source values are preserved. Set a Space Use Override to force either answer.", spaceUse_Name, internalConditionName, spaceUse_InternalCondition));

                result_Conflict.SetSources(spaceUse_Name, spaceUse_InternalCondition, true);
                return result_Conflict;
            }

            if (resolved_Name)
            {
                fromName.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                return fromName;
            }

            if (resolved_InternalCondition)
            {
                fromInternalCondition.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                return fromInternalCondition;
            }

            //Neither resolved cleanly. Prefer whichever produced a diagnostic (an ambiguous match) over
            //nothing, so the reason survives to the caller.
            if (fromName is not null)
            {
                fromName.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                return fromName;
            }

            if (fromInternalCondition is not null)
            {
                fromInternalCondition.SetSources(spaceUse_Name, spaceUse_InternalCondition, false);
                return fromInternalCondition;
            }

            //6. Unclassified, always reported.
            return Create.SpaceSemantics(
                SpaceUse.Undefined,
                SpaceSemanticsSource.Unclassified,
                null,
                string.Format("'{0}' matches no configured space use synonym as a whole word or phrase. Rename the space, add a synonym to the space use text map, or set a Space Use Override.", space.Name));
        }

        /// <summary>
        /// Runs the exact-synonym then whole-phrase match over one piece of text. Returns null when
        /// nothing matched; returns an Unclassified result with a diagnostic when the text matched two
        /// different space uses equally well, because that is a genuine conflict the matcher must not
        /// resolve by guessing.
        /// </summary>
        private SpaceSemantics Match(string text, SpaceSemanticsSource source)
        {
            if (textMap is null || keys.Count == 0 || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string normalized = Query.SemanticNormalizedName(text);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            //Exact synonym first: the most specific evidence available, and never ambiguous unless the
            //text map itself lists the same synonym under two uses.
            List<string> keys_Exact = [];
            foreach (string key in keys)
            {
                foreach (string alias in textMap.GetValues(key) ?? [])
                {
                    if (string.Equals(Query.SemanticNormalizedName(alias), normalized, StringComparison.Ordinal))
                    {
                        keys_Exact.Add(key);
                        break;
                    }
                }
            }

            if (keys_Exact.Count == 1)
            {
                return Create.SpaceSemantics((SpaceUse)Enum.Parse(typeof(SpaceUse), keys_Exact[0]), source, normalized);
            }

            if (keys_Exact.Count > 1)
            {
                return Create.SpaceSemantics(
                    SpaceUse.Undefined,
                    SpaceSemanticsSource.Unclassified,
                    normalized,
                    string.Format("'{0}' is listed as an exact synonym of more than one space use ({1}). Remove the duplicate from the space use text map, or set a Space Use Override.", text, string.Join(", ", keys_Exact.OrderBy(x => x))));
            }

            //Whole-token/phrase match, longest phrase wins. A null return from
            //SemanticBestTextMapKey means either nothing matched or two uses tied at the top rank.
            string key_Best = textMap.SemanticBestTextMapKey(text, keys);
            if (key_Best is not null)
            {
                SpaceSemanticsSource source_Phrase = source == SpaceSemanticsSource.InternalCondition
                    ? SpaceSemanticsSource.InternalCondition
                    : SpaceSemanticsSource.PhraseMatch;

                List<TM59TextMapMatch> matches = textMap.SemanticTextMapMatches(text, [key_Best]);
                string alias = matches.Count == 0 ? normalized : matches[0].Alias;

                return Create.SpaceSemantics((SpaceUse)Enum.Parse(typeof(SpaceUse), key_Best), source_Phrase, alias);
            }

            List<TM59TextMapMatch> matches_All = textMap.SemanticTextMapMatches(text, keys);
            if (matches_All.Count == 0)
            {
                return null;
            }

            int topTokenCount = matches_All[0].TokenCount;
            List<string> keys_Tied = [.. matches_All.Where(x => x.TokenCount == topTokenCount).Select(x => x.Key).Distinct().OrderBy(x => x)];

            return Create.SpaceSemantics(
                SpaceUse.Undefined,
                SpaceSemanticsSource.Unclassified,
                normalized,
                string.Format("'{0}' matches {1} equally well, so it has not been classified. Rename the space or set a Space Use Override.", text, string.Join(" and ", keys_Tied)));
        }
    }
}
