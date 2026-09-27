// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The selected Approved Document O strategy of every dwelling that has one, persisted on the clean
    /// baseline as <c>AnalyticalModelParameter.PartODwellingStrategies</c>.
    ///
    /// <para><b>At most one strategy per dwelling</b></para>
    /// <para>
    /// Keyed by zone guid. <see cref="Set"/> replaces; a persisted collection holding two strategies for one
    /// zone is <b>not</b> quietly reduced to either - it loads with <see cref="Conflicts"/> set and
    /// <see cref="IsValid"/> false, and materialisation refuses it.
    /// </para>
    ///
    /// <para><b>Versioned, and fail-closed on a version it does not know</b></para>
    /// <para>
    /// Written with <see cref="Schema"/>. A collection of an unknown schema loads as invalid rather than
    /// being reinterpreted, so a later format can never be materialised under this one's rules.
    /// </para>
    ///
    /// <para><b>Absent means legacy, not error</b></para>
    /// <para>
    /// A model without this parameter is an ordinary legacy model: nothing reads it as a mixed model, nothing
    /// migrates it into one, and no strategy is inferred from its systems, scenarios, report text or names.
    /// </para>
    ///
    /// <para><b>Canonical serialisation</b></para>
    /// <para>
    /// Strategies are written in zone-guid order and carry no instance guids, so two logically identical sets
    /// write the same bytes whatever order they were built in, and the baseline's model fingerprint does not
    /// move when a user interface rebuilds the set.
    /// </para>
    /// </summary>
    public class PartODwellingStrategySet : IJSAMObject, IAnalyticalObject
    {
        /// <summary>The persisted schema this build reads and writes.</summary>
        public const string Schema = "PartODwellingStrategies:v1";

        private readonly SortedDictionary<Guid, PartODwellingStrategy> dictionary = [];

        //The extra strategies of a conflicted dwelling, kept exactly as read so that re-saving an invalid set
        //writes the conflict back rather than silently resolving it to whichever strategy came first.
        private readonly List<PartODwellingStrategy> strategies_Conflicting = [];

        public PartODwellingStrategySet()
        {
        }

        public PartODwellingStrategySet(IEnumerable<PartODwellingStrategy> partODwellingStrategies)
        {
            foreach (PartODwellingStrategy partODwellingStrategy in partODwellingStrategies ?? [])
            {
                Set(partODwellingStrategy);
            }
        }

        public PartODwellingStrategySet(PartODwellingStrategySet partODwellingStrategySet)
        {
            if (partODwellingStrategySet is not null)
            {
                foreach (PartODwellingStrategy partODwellingStrategy in partODwellingStrategySet.dictionary.Values)
                {
                    dictionary[partODwellingStrategy.ZoneGuid] = new PartODwellingStrategy(partODwellingStrategy);
                }

                SchemaRead = partODwellingStrategySet.SchemaRead;
                Conflicts.AddRange(partODwellingStrategySet.Conflicts);
                strategies_Conflicting.AddRange(partODwellingStrategySet.strategies_Conflicting.ConvertAll(x => new PartODwellingStrategy(x)));
            }
        }

        public PartODwellingStrategySet(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        /// <summary>The schema the collection was read with - <see cref="Schema"/> unless it was loaded from something else.</summary>
        public string SchemaRead { get; private set; } = Schema;

        /// <summary>Zones for which the persisted collection carried more than one strategy. Non-empty makes the set invalid.</summary>
        public List<Guid> Conflicts { get; } = [];

        /// <summary>Whether the set can be materialised from at all: a known schema and no duplicated dwelling.</summary>
        public bool IsValid => SchemaRead == Schema && Conflicts.Count == 0;

        public int Count => dictionary.Count;

        /// <summary>Copies of the strategies, in zone-guid order.</summary>
        public List<PartODwellingStrategy> Strategies
        {
            get
            {
                List<PartODwellingStrategy> result = [];
                foreach (PartODwellingStrategy partODwellingStrategy in dictionary.Values)
                {
                    result.Add(new PartODwellingStrategy(partODwellingStrategy));
                }

                return result;
            }
        }

        /// <summary>Selects (or replaces) the strategy of one dwelling. A strategy with no zone is ignored.</summary>
        public bool Set(PartODwellingStrategy partODwellingStrategy)
        {
            if (partODwellingStrategy is null || partODwellingStrategy.ZoneGuid == Guid.Empty)
            {
                return false;
            }

            dictionary[partODwellingStrategy.ZoneGuid] = new PartODwellingStrategy(partODwellingStrategy);

            //An explicit selection is a decision, so it - and only it - resolves a conflict read from disk.
            Conflicts.Remove(partODwellingStrategy.ZoneGuid);
            strategies_Conflicting.RemoveAll(x => x.ZoneGuid == partODwellingStrategy.ZoneGuid);

            return true;
        }

        /// <summary>Removes a dwelling's strategy - the dwelling is then undecided, not natural.</summary>
        public bool Remove(Guid guid_Zone) => dictionary.Remove(guid_Zone);

        /// <summary>The strategy selected for a dwelling, as a copy, or null where none is.</summary>
        public PartODwellingStrategy Strategy(Guid guid_Zone)
        {
            return dictionary.TryGetValue(guid_Zone, out PartODwellingStrategy result) ? new PartODwellingStrategy(result) : null;
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            dictionary.Clear();
            Conflicts.Clear();
            strategies_Conflicting.Clear();

            if (jsonObject is null)
            {
                return false;
            }

            SchemaRead = jsonObject["Schema"] is JsonValue jsonValue && jsonValue.TryGetValue(out string schema) ? schema : null;

            if (jsonObject["Strategies"] is JsonArray jsonArray)
            {
                foreach (JsonNode jsonNode in jsonArray)
                {
                    if (jsonNode is not JsonObject jsonObject_Strategy)
                    {
                        continue;
                    }

                    PartODwellingStrategy partODwellingStrategy = new(jsonObject_Strategy);

                    if (dictionary.ContainsKey(partODwellingStrategy.ZoneGuid))
                    {
                        if (!Conflicts.Contains(partODwellingStrategy.ZoneGuid))
                        {
                            Conflicts.Add(partODwellingStrategy.ZoneGuid);
                        }

                        strategies_Conflicting.Add(partODwellingStrategy);

                        continue;
                    }

                    //An unreadable strategy is kept, not dropped: dropping it would turn a decision nobody can
                    //read into a dwelling nobody decided about, and the materialisation refuses it by zone.
                    dictionary[partODwellingStrategy.ZoneGuid] = partODwellingStrategy;
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonArray jsonArray = [];

            foreach (PartODwellingStrategy partODwellingStrategy in dictionary.Values)
            {
                jsonArray.Add(partODwellingStrategy.ToJsonObject());
            }

            foreach (PartODwellingStrategy partODwellingStrategy in strategies_Conflicting)
            {
                jsonArray.Add(partODwellingStrategy.ToJsonObject());
            }

            return new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Schema"] = SchemaRead ?? Schema,
                ["Strategies"] = jsonArray,
            };
        }
    }
}
