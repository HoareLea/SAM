// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// What a materialised mixed model was built from, stamped on it as
    /// <c>AnalyticalModelParameter.PartOMaterialisationRecord</c> by
    /// <c>Modify.MaterialisePartODwellingStrategies</c>.
    ///
    /// <para><b>Three fingerprints, so a refusal names which input moved</b></para>
    /// <list type="bullet">
    /// <item>
    /// <see cref="Fingerprint_Baseline"/> - <see cref="SimulationResultProvenance.Fingerprint(AnalyticalModel)"/>
    /// over the clean baseline, the existing model digest rather than a parallel one. The baseline carries
    /// its strategy collection, which serialises canonically and without instance guids, so it is covered too.
    /// </item>
    /// <item>
    /// <see cref="Fingerprint_Strategies"/> - the selected strategies of the assessed dwellings
    /// (<see cref="Query.PartOStrategyFingerprint"/>), which names a strategy edit on its own.
    /// </item>
    /// <item>
    /// <see cref="Fingerprint_Catalogue"/> - every selection-relevant catalogue field
    /// (<see cref="Query.PartOCatalogueFingerprint"/>): identity, maximum supply, maximum extract and rank.
    /// </item>
    /// </list>
    ///
    /// <para><b>Staleness fails closed at three levels</b></para>
    /// <list type="number">
    /// <item>baseline, strategies or catalogue ≠ this record → materialise again (<see cref="IsCurrent"/>);</item>
    /// <item>the materialised model ≠ its <see cref="SimulationResultProvenance"/> → simulate again (existing);</item>
    /// <item>a rebuilt model always needs a fresh simulation: every generated object has a new guid, so its model
    /// fingerprint differs from any earlier run's provenance, even where its engineering state is identical.</item>
    /// </list>
    ///
    /// <para>
    /// <b>The prepared design is persisted.</b> <see cref="VentilationSystemGuids"/> maps each MVHR dwelling to
    /// the system it was materialised with, so the design under assessment is no longer session-only state
    /// (blocker C5).
    /// </para>
    /// </summary>
    public class PartOMaterialisationRecord : IJSAMObject, IAnalyticalObject
    {
        /// <summary>The persisted schema this build reads and writes.</summary>
        public const string Schema = "PartOMaterialisation:v1";

        public PartOMaterialisationRecord()
        {
        }

        public PartOMaterialisationRecord(PartOMaterialisationRecord partOMaterialisationRecord)
        {
            if (partOMaterialisationRecord is not null)
            {
                SchemaRead = partOMaterialisationRecord.SchemaRead;
                Fingerprint_Baseline = partOMaterialisationRecord.Fingerprint_Baseline;
                Fingerprint_Strategies = partOMaterialisationRecord.Fingerprint_Strategies;
                Fingerprint_Catalogue = partOMaterialisationRecord.Fingerprint_Catalogue;
                ZoneGuids_Assessed.AddRange(partOMaterialisationRecord.ZoneGuids_Assessed);
                ZoneGuids_CommonSpace.AddRange(partOMaterialisationRecord.ZoneGuids_CommonSpace);

                foreach (KeyValuePair<Guid, Guid> keyValuePair in partOMaterialisationRecord.VentilationSystemGuids)
                {
                    VentilationSystemGuids[keyValuePair.Key] = keyValuePair.Value;
                }
            }
        }

        public PartOMaterialisationRecord(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        public string SchemaRead { get; private set; } = Schema;

        public string Fingerprint_Baseline { get; set; } = string.Empty;

        public string Fingerprint_Strategies { get; set; } = string.Empty;

        public string Fingerprint_Catalogue { get; set; } = string.Empty;

        /// <summary>The dwelling zones materialised, in guid order.</summary>
        public List<Guid> ZoneGuids_Assessed { get; } = [];

        /// <summary>The common-space zones assessed automatically (communal corridors), in guid order.</summary>
        public List<Guid> ZoneGuids_CommonSpace { get; } = [];

        /// <summary>Each MVHR dwelling zone → the ventilation system it was materialised with.</summary>
        public SortedDictionary<Guid, Guid> VentilationSystemGuids { get; } = [];

        /// <summary>A known schema and every fingerprint present. A partial record is not a record.</summary>
        public bool IsValid => SchemaRead == Schema && !string.IsNullOrEmpty(Fingerprint_Baseline) && !string.IsNullOrEmpty(Fingerprint_Strategies) && !string.IsNullOrEmpty(Fingerprint_Catalogue);

        /// <summary>
        /// Whether this record still describes what materialising <paramref name="analyticalModel_Baseline"/>
        /// against <paramref name="ventilationUnitCapacityDescriptors"/> would build. Fails closed: an invalid
        /// record, a moved baseline, a changed strategy or a changed catalogue each refuse with their reason.
        /// </summary>
        public bool IsCurrent(AnalyticalModel analyticalModel_Baseline, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, out string reason)
        {
            reason = null;

            if (!IsValid)
            {
                reason = "The materialisation record is incomplete or of an unknown schema, so it cannot state what the model was built from.";

                return false;
            }

            if (analyticalModel_Baseline is null)
            {
                reason = "No baseline was supplied to compare the materialisation record with.";

                return false;
            }

            PartODwellingStrategySet partODwellingStrategySet = analyticalModel_Baseline.GetValue<PartODwellingStrategySet>(AnalyticalModelParameter.PartODwellingStrategies);

            List<PartODwellingStrategy> strategies = [];
            foreach (Guid guid in ZoneGuids_Assessed)
            {
                PartODwellingStrategy partODwellingStrategy = partODwellingStrategySet?.Strategy(guid);
                if (partODwellingStrategy is not null)
                {
                    strategies.Add(partODwellingStrategy);
                }
            }

            if (Query.PartOStrategyFingerprint(strategies, ZoneGuids_Assessed) != Fingerprint_Strategies)
            {
                reason = "A selected dwelling strategy has changed since the model was materialised. Materialise again.";

                return false;
            }

            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors_ProjectTest = analyticalModel_Baseline.GetValue<PartOProjectTestVentilationUnit>(AnalyticalModelParameter.PartOProjectTestVentilationUnit)?.CapacityDescriptors();

            if (Query.PartOCatalogueFingerprint(ventilationUnitCapacityDescriptors, ventilationUnitCapacityDescriptors_ProjectTest) != Fingerprint_Catalogue)
            {
                reason = "The ventilation unit catalogue has changed since the model was materialised - a product's identity, capacity or rank, any of which can change the unit selected. Materialise again.";

                return false;
            }

            if (SimulationResultProvenance.Fingerprint(analyticalModel_Baseline) != Fingerprint_Baseline)
            {
                reason = "The baseline has changed since the model was materialised. Materialise again.";

                return false;
            }

            return true;
        }

        public bool FromJsonObject(JsonObject jsonObject)
        {
            ZoneGuids_Assessed.Clear();
            ZoneGuids_CommonSpace.Clear();
            VentilationSystemGuids.Clear();

            if (jsonObject is null)
            {
                return false;
            }

            SchemaRead = Text(jsonObject, "Schema");
            Fingerprint_Baseline = Text(jsonObject, "Fingerprint_Baseline") ?? string.Empty;
            Fingerprint_Strategies = Text(jsonObject, "Fingerprint_Strategies") ?? string.Empty;
            Fingerprint_Catalogue = Text(jsonObject, "Fingerprint_Catalogue") ?? string.Empty;

            ReadGuids(jsonObject["ZoneGuids_Assessed"] as JsonArray, ZoneGuids_Assessed);
            ReadGuids(jsonObject["ZoneGuids_CommonSpace"] as JsonArray, ZoneGuids_CommonSpace);

            if (jsonObject["VentilationSystemGuids"] is JsonObject jsonObject_Systems)
            {
                foreach (KeyValuePair<string, JsonNode> keyValuePair in jsonObject_Systems)
                {
                    if (Guid.TryParse(keyValuePair.Key, out Guid guid_Zone) && keyValuePair.Value is JsonValue jsonValue && jsonValue.TryGetValue(out string text) && Guid.TryParse(text, out Guid guid_System))
                    {
                        VentilationSystemGuids[guid_Zone] = guid_System;
                    }
                }
            }

            return true;
        }

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject_Systems = [];
            foreach (KeyValuePair<Guid, Guid> keyValuePair in VentilationSystemGuids)
            {
                jsonObject_Systems[keyValuePair.Key.ToString("D", CultureInfo.InvariantCulture)] = keyValuePair.Value.ToString("D", CultureInfo.InvariantCulture);
            }

            return new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Schema"] = SchemaRead ?? Schema,
                ["Fingerprint_Baseline"] = Fingerprint_Baseline,
                ["Fingerprint_Strategies"] = Fingerprint_Strategies,
                ["Fingerprint_Catalogue"] = Fingerprint_Catalogue,
                ["ZoneGuids_Assessed"] = WriteGuids(ZoneGuids_Assessed),
                ["ZoneGuids_CommonSpace"] = WriteGuids(ZoneGuids_CommonSpace),
                ["VentilationSystemGuids"] = jsonObject_Systems,
            };
        }

        private static void ReadGuids(JsonArray jsonArray, List<Guid> guids)
        {
            foreach (JsonNode jsonNode in jsonArray ?? [])
            {
                if (jsonNode is JsonValue jsonValue && jsonValue.TryGetValue(out string text) && Guid.TryParse(text, out Guid guid))
                {
                    guids.Add(guid);
                }
            }
        }

        private static JsonArray WriteGuids(List<Guid> guids)
        {
            JsonArray result = [];
            foreach (Guid guid in guids)
            {
                result.Add(guid.ToString("D", CultureInfo.InvariantCulture));
            }

            return result;
        }

        private static string Text(JsonObject jsonObject, string name)
        {
            return jsonObject[name] is JsonValue jsonValue && jsonValue.TryGetValue(out string result) ? result : null;
        }
    }
}
