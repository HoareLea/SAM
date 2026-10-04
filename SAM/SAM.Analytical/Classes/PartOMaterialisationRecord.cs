// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
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
    /// <item>
    /// Per cooled dwelling, <see cref="PartOCooledDwelling.Fingerprint_Guidance"/> - the product's whole manufacturer
    /// operating strategy (<see cref="Query.PartOCoolingGuidanceFingerprint"/>).
    /// </item>
    /// </list>
    ///
    /// <para><b>Two schemas, so uncooled records never move</b></para>
    /// <para>
    /// A record with no cooled dwelling is written exactly as before, <see cref="Schema"/>; the route is the IZAM
    /// route. A record with a cooled dwelling is <see cref="Schema_Cooled"/>, carries the route (the Systems route,
    /// for the whole model) and <see cref="CooledDwellings"/>. A v2 record without a cooled dwelling, or with an
    /// unreadable one, is invalid.
    /// </para>
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

        /// <summary>The schema of a record with at least one cooled dwelling.</summary>
        public const string Schema_Cooled = "PartOMaterialisation:v2";

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

                partOMaterialisationRecord.CooledDwellings.ForEach(x => CooledDwellings.Add(new PartOCooledDwelling(x)));
                route_Read = partOMaterialisationRecord.route_Read;
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

        /// <summary>The cooled dwellings, in zone guid order. Empty for a model with none.</summary>
        public List<PartOCooledDwelling> CooledDwellings { get; } = [];

        //The route a v2 record states when read, kept to check it against its cooled dwellings. Undefined otherwise.
        private PartOSimulationRoute route_Read = PartOSimulationRoute.Undefined;

        /// <summary>
        /// How the materialised model is simulated: the Systems route for the whole model as soon as one dwelling is
        /// cooled, the IZAM route otherwise. Derived from <see cref="CooledDwellings"/>, never chosen.
        /// </summary>
        public PartOSimulationRoute Route => CooledDwellings.Count == 0 ? PartOSimulationRoute.Izam : PartOSimulationRoute.Systems;

        /// <summary>
        /// A known schema, every fingerprint present, and a schema that matches the record's cooling: v1 with no cooled
        /// dwelling read from file, v2 with valid cooled dwellings and the Systems route. A partial record is not a record.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Fingerprint_Baseline) || string.IsNullOrEmpty(Fingerprint_Strategies) || string.IsNullOrEmpty(Fingerprint_Catalogue))
                {
                    return false;
                }

                if (SchemaRead == Schema_Cooled)
                {
                    return CooledDwellings.Count != 0 && CooledDwellings.TrueForAll(x => x is not null && x.IsValid) && route_Read == PartOSimulationRoute.Systems;
                }

                //A record built in this session starts at v1 and is written as v2 once it holds a cooled dwelling.
                return SchemaRead == Schema && CooledDwellings.TrueForAll(x => x is not null && x.IsValid);
            }
        }

        /// <summary>
        /// Whether this record still describes what materialising <paramref name="analyticalModel_Baseline"/>
        /// against <paramref name="ventilationUnitCapacityDescriptors"/> would build. Fails closed: an invalid
        /// record, a moved baseline, a changed strategy or a changed catalogue each refuse with their reason.
        /// </summary>
        public bool IsCurrent(AnalyticalModel analyticalModel_Baseline, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, out string reason)
        {
            return IsCurrent(analyticalModel_Baseline, ventilationUnitCapacityDescriptors, null, out reason);
        }

        /// <summary>
        /// As <see cref="IsCurrent(AnalyticalModel, IEnumerable{VentilationUnitCapacityDescriptor}, out string)"/>, and for a
        /// record with cooled dwellings also whether each cooled dwelling's product still carries the manufacturer
        /// guidance it was materialised with, in <paramref name="ventilationUnitTemplates"/>. A cooled record checked
        /// without the templates is not current: its cooling cannot be shown to be unchanged.
        /// </summary>
        public bool IsCurrent(AnalyticalModel analyticalModel_Baseline, IEnumerable<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors, IEnumerable<VentilationUnitTemplate> ventilationUnitTemplates, out string reason)
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

            foreach (PartOCooledDwelling partOCooledDwelling in CooledDwellings)
            {
                PartODwellingStrategy strategy = partODwellingStrategySet?.Strategy(partOCooledDwelling.ZoneGuid);
                Zone zone = analyticalModel_Baseline.AdjacencyCluster?.GetObject<Zone>(partOCooledDwelling.ZoneGuid);
                if (strategy?.CoolingStatSpaceGuid != partOCooledDwelling.CoolingStatSpaceGuid
                    || zone is null
                    || !(analyticalModel_Baseline.AdjacencyCluster.GetRelatedObjects<Space>(zone)?.Exists(x => x.Guid == partOCooledDwelling.CoolingStatSpaceGuid) ?? false))
                {
                    reason = "The recorded cooling control room is not the selected room of its dwelling. Materialise again.";
                    return false;
                }

                VentilationUnitTemplate ventilationUnitTemplate = Query.PartOCoolingTemplate(ventilationUnitTemplates, partOCooledDwelling.VentilationUnitReference);

                if (ventilationUnitTemplate is null || ventilationUnitTemplate.PartOCoolingGuidanceFingerprint() != partOCooledDwelling.Fingerprint_Guidance)
                {
                    reason = string.Format("The manufacturer cooling guidance of '{0}' has changed since the model was materialised, or was not supplied, so the cooling it was simulated with cannot be shown to be current. Materialise again.", partOCooledDwelling.VentilationUnitReference);

                    return false;
                }

                //The stored airflow is what the Systems route is configured with, so it must still be the one the
                //guidance resolves the recorded design duty to - never a figure edited or corrupted since.
                double coolingOperatingAirFlow_Lps = ventilationUnitTemplate.PartOCoolingOperatingAirFlow(partOCooledDwelling.DesignSupply_Lps, partOCooledDwelling.DesignExtract_Lps, out string refusal);
                if (refusal is not null || System.Math.Abs(coolingOperatingAirFlow_Lps - partOCooledDwelling.CoolingOperatingAirFlow_Lps) > 1e-9)
                {
                    reason = string.Format(CultureInfo.InvariantCulture, "The recorded cooling operating airflow of '{0}' ({1:0.###} l/s) is not the one its manufacturer guidance resolves the recorded design duty to, so the record cannot be trusted. Materialise again.", partOCooledDwelling.VentilationUnitReference, partOCooledDwelling.CoolingOperatingAirFlow_Lps);

                    return false;
                }
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
            CooledDwellings.Clear();
            route_Read = PartOSimulationRoute.Undefined;

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

            if (SchemaRead == Schema_Cooled)
            {
                route_Read = Enum.TryParse(Text(jsonObject, "Route") ?? string.Empty, out PartOSimulationRoute route) && Enum.IsDefined(typeof(PartOSimulationRoute), route) ? route : PartOSimulationRoute.Undefined;

                foreach (JsonNode jsonNode in jsonObject["CooledDwellings"] as JsonArray ?? [])
                {
                    CooledDwellings.Add(jsonNode is JsonObject jsonObject_Cooled ? new PartOCooledDwelling(jsonObject_Cooled) : new PartOCooledDwelling());
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

            //A known schema is written as the record's cooling states it; an unknown one is written back as read, so
            //re-saving a record from a later build cannot turn it into one this build understands. A record read as
            //v2 stays v2 with the route it stated: a truncated or contradictory cooled record is never re-saved as a
            //valid uncooled one.
            bool known = SchemaRead == Schema || SchemaRead == Schema_Cooled;
            string schema = !known ? SchemaRead ?? Schema : SchemaRead == Schema_Cooled || CooledDwellings.Count != 0 ? Schema_Cooled : Schema;

            JsonObject result = new()
            {
                ["_type"] = Core.Query.FullTypeName(this),
                ["Schema"] = schema,
                ["Fingerprint_Baseline"] = Fingerprint_Baseline,
                ["Fingerprint_Strategies"] = Fingerprint_Strategies,
                ["Fingerprint_Catalogue"] = Fingerprint_Catalogue,
                ["ZoneGuids_Assessed"] = WriteGuids(ZoneGuids_Assessed),
                ["ZoneGuids_CommonSpace"] = WriteGuids(ZoneGuids_CommonSpace),
                ["VentilationSystemGuids"] = jsonObject_Systems,
            };

            if (schema == Schema_Cooled)
            {
                JsonArray jsonArray_Cooled = [];
                CooledDwellings.ForEach(x => jsonArray_Cooled.Add(x?.ToJsonObject()));

                result["Route"] = (SchemaRead == Schema_Cooled ? route_Read : Route).ToString();
                result["CooledDwellings"] = jsonArray_Cooled;
            }

            return result;
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
