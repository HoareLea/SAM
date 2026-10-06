// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Weather;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Analytical
{
    /// <summary>
    /// The CIBSE TM52 and TM59 overheating assessment, over simulated space data already stored on the
    /// model. <b>Analytical-domain code with no engine dependency of any kind.</b>
    /// <para>
    /// It was extracted from <c>SAM.Analytical.Tas.OverheatingCalculator</c>, which never called TAS: it
    /// read two named hourly series off each <c>Space</c> and produced <c>TM5x</c> results. Sitting
    /// in the TAS assembly meant its tests needed a licensed TAS install to run, for no reason other than
    /// where the file happened to live. That class remains, as a thin wrapper over this one, so every
    /// existing Grasshopper and user-interface caller is unaffected.
    /// </para>
    /// <para>
    /// <b>Named <c>TMOverheatingCalculator</c>, not <c>OverheatingCalculator</c>.</b> A type of the same
    /// name in a parent namespace shadows the child's in every file importing both - the mistake that broke
    /// seven call sites during the Part F work - and <c>SAM.Analytical.Tas</c> is nested inside
    /// <c>SAM.Analytical</c>. The name also matches the <c>TMResult</c> / <c>TMExtendedResult</c> hierarchy
    /// it produces.
    /// </para>
    /// <para>
    /// Approved Document O's dynamic method follows the TM59 methodology, so this is the calculation a Part
    /// O assessment rests on. It is TM59:2017 / current Approved Document O; nothing here anticipates a
    /// later edition.
    /// </para>
    /// </summary>
    public class TMOverheatingCalculator
    {
        private TextMap textMap = Query.DefaultInternalConditionTextMap_TM59();

        private List<string> ventilationStrategyRefusals = [];

        private List<string> hourlySeriesRefusals = [];

        private List<System.Guid> spaceGuids_HourlySeriesRefused = [];

        public TMOverheatingCalculator(AnalyticalModel analyticalModel)
        {
            AnalyticalModel = analyticalModel;
        }

        public TM52BuildingCategory TM52BuildingCategory { get; set; } = TM52BuildingCategory.CategoryII;

        public AnalyticalModel AnalyticalModel { get; set; } = null;

        /// <summary>
        /// The key under which each space's hourly operative-temperature series is stored in its
        /// <c>ParameterSet</c> - a key returning a <c>JsonArray</c> of simulated values.
        /// <para>
        /// Configurable and not a constant because the writing side chooses it: the analytical vocabulary
        /// and the TAS conversion do not agree on every key, and the assessment must read what was actually
        /// stored rather than what this assembly would have called it. Defaults to the analytical
        /// vocabulary; the TAS wrapper supplies TAS's. Two keys, both defaulted - deliberately not a
        /// general series-lookup framework.
        /// </para>
        /// </summary>
        public string ResultantTemperatureSeriesKey { get; set; } = Core.Query.Name(SpaceSimulationResultParameter.ResultantTemperature);

        /// <summary>
        /// The key for each space's hourly occupancy-gain series, used only to decide which hours were
        /// occupied.
        /// <para>
        /// <b>This is the key the two vocabularies disagree about.</b> The analytical vocabulary says
        /// "Occupancy Sensible Gain"; the TAS conversion writes "Occupant Sensible Gain". Reading the wrong
        /// one is silent - the space simply produces no assessment - so the key is supplied rather than
        /// assumed. Reconciling the two is deliberately left as separate work, and no stored data is
        /// migrated here.
        /// </para>
        /// </summary>
        public string OccupancySensibleGainSeriesKey { get; set; } = Core.Query.Name(SpaceSimulationResultParameter.OccupancySensibleGain);

        /// <summary>
        /// How many hourly values each series must carry for a space to be assessed at all. <b>0 - the
        /// default - enforces nothing</b>, and every equal-length pair of series is assessed however short
        /// it is.
        ///
        /// <para><b>Why this is stated by the caller and not decided here</b></para>
        /// <para>
        /// This class is the TM52 and TM59 calculation over whatever series it is given, and the length a
        /// series has to be is a property of the RUN rather than of the calculation. A Grasshopper component
        /// handed four hours of a test model, or a TM52 assessment of a summer window, is doing something
        /// legitimate; refusing it because a year is 8760 hours long would break a calculation that is
        /// correct for its input.
        /// </para>
        /// <para>
        /// Approved Document O is the case where a full year IS the contract - its dynamic method assesses
        /// annual and summer criteria, and a verdict from part of a year is not the verdict the document
        /// asks for. <c>PartOTM59Assessment</c> therefore sets this from the WEATHER YEAR the results were
        /// produced against, which is the same authority the comfort band is derived from, rather than from
        /// a literal 8760: a year's hour count is whatever its weather data actually holds.
        /// </para>
        /// <para>
        /// <b>Shorter is refused; longer is not.</b> A series with fewer hours than the run needs is missing
        /// data. A series with more - a leap-year simulation's 8784 against a 365-day weather year - is not,
        /// and the surplus hours are already excluded by <see cref="Collect"/>, which refuses any hour the
        /// comfort band does not cover rather than assessing it against a 0 degC limit.
        /// </para>
        /// </summary>
        public int HourCount_Expected { get; set; } = 0;

        /// <summary>
        /// Where a result says it came from. <b>Provenance only</b> - it names no object, owns no result and
        /// takes no part in any scenario, equipment or result identity.
        /// </summary>
        public string Source
        {
            get
            {
                string result = AnalyticalModel?.Name;

                return string.IsNullOrWhiteSpace(result) ? SourceFallback : result;
            }
        }

        /// <summary>
        /// What <see cref="Source"/> reports where the model is unnamed. Settable so the TAS wrapper can
        /// keep stamping its own assembly name, as it always has.
        /// </summary>
        public string SourceFallback { get; set; } = Core.Query.Name(typeof(TMOverheatingCalculator).Assembly);

        /// <summary>
        /// Which ventilation strategy governs which space, as stated by <c>OverheatingScenario</c>. Where this
        /// is supplied it is <b>authoritative</b> for the TM59 criterion.
        /// <para>
        /// Supplied, not derived, and it replaces the derivation rather than seeding it. The space's internal
        /// condition, the zone-name lookup and the natural-ventilation default in
        /// <see cref="SystemTypeName"/> are all bypassed, and a space the map refuses produces <b>no
        /// assessment</b> with its reason in <see cref="VentilationStrategyRefusals"/> - never an assessment
        /// against a fallback criterion. A gap is visible; a number measured against the wrong TM59 rule is
        /// not.
        /// </para>
        /// <para>
        /// <b>Left null the old derivation applies, unchanged.</b> Every existing caller - the Grasshopper
        /// components, the user interface, <c>OverheatingCalculator</c> - keeps the behaviour it had, because
        /// none of them has a scenario to state yet. Making the fallback unreachable is a later step, once
        /// there is a path that always supplies one.
        /// </para>
        /// <para>
        /// <b>Held by reference, and live.</b> A caller that keeps adding scenarios after assigning this will
        /// change what the next <see cref="Calculate_TM59"/> decides. That is deliberate - a map is built up
        /// scenario by scenario and is not an identity - but it is the opposite of the copy-in discipline
        /// <c>OverheatingScenario</c> follows, so it is stated rather than left to be discovered.
        /// </para>
        /// </summary>
        public VentilationStrategyMap VentilationStrategyMap { get; set; } = null;

        /// <summary>
        /// Why spaces were left out of the last <see cref="Calculate_TM59"/> because
        /// <see cref="VentilationStrategyMap"/> refused them, one sentence each. A copy; replaced by every
        /// <see cref="Calculate_TM59"/> call, and empty where no map was supplied or nothing was refused.
        /// <para>
        /// The refusals are reported rather than thrown so that one unstated dwelling does not lose the
        /// assessment of every other dwelling in the building - but they are reported, which is the whole
        /// difference between this and the silent default it replaces.
        /// </para>
        /// <para>
        /// <b><see cref="Calculate_TM52"/> deliberately does not clear these.</b> TM52 selects no criterion and
        /// so can neither produce nor answer a ventilation refusal; clearing them would let a TM52 run erase a
        /// TM59 run's record of which dwellings went unassessed. They belong to the last TM59 call and nothing
        /// else touches them.
        /// </para>
        /// <para>
        /// A space listed twice is refused twice, because it was asked about twice - the same way the no-map
        /// path assesses it twice. De-duplicating only the refusals would misreport the input.
        /// </para>
        /// </summary>
        public List<string> VentilationStrategyRefusals => [.. ventilationStrategyRefusals];

        /// <summary>
        /// Spaces left out of the last calculation because their two hourly series could not be assessed
        /// together, one sentence each - a series absent, a series empty, or the two series of DIFFERENT
        /// LENGTHS.
        ///
        /// <para><b>What this replaces</b></para>
        /// <para>
        /// <see cref="Collect"/> walks both arrays with one counter and used to bound the walk by the
        /// shorter of the two. That stopped a truncated result throwing out of the whole run - which is
        /// worth keeping, and is kept - but it also meant a space whose resultant-temperature series ended
        /// early was assessed over the hours the two happened to share and its verdict reported as though it
        /// were a verdict about the space. Which of two unequal series is the truncated one is not knowable
        /// here, so neither can be trusted, and an overheating verdict measured over part of a room's hours
        /// is not a verdict about that room.
        /// </para>
        /// <para>
        /// So the space is refused instead: no result, and a reason. Nothing throws, and every other space
        /// in the building is still assessed - the same trade the shared-range walk was making, with the
        /// unassessable space now visible rather than silently reported.
        /// </para>
        ///
        /// <para><b>Length equality only, at this level</b></para>
        /// <para>
        /// Two equal-length series are assessed however long they are. This class is the TM52 and TM59
        /// calculation over whatever series it is given, and a caller assessing a deliberately short run -
        /// the Grasshopper components, a summer-only TM52 window - is doing something legitimate that
        /// nothing here should refuse. Whether a series is long enough to be a full year is a question about
        /// the RUN, and it is asked where a full year is actually the contract: Approved Document O requires
        /// one, and <c>PartOTM59Assessment</c> refuses a short series on the strength of the weather year
        /// the results were produced against.
        /// </para>
        /// <para>A copy, so a reporting layer cannot edit the record of what went unassessed.</para>
        /// </summary>
        public List<string> HourlySeriesRefusals => [.. hourlySeriesRefusals];

        /// <summary>
        /// The <see cref="Space.Guid"/> of every space named in <see cref="HourlySeriesRefusals"/>, in the
        /// same order.
        /// <para>
        /// Identities as well as prose, because a caller that has to keep a refused room OUT of a pass needs
        /// to name it rather than parse a sentence. <c>PartOTM59Assessment</c> maps these back to their
        /// design spaces and counts them as unassessed, which is what stops an Approved Document O run
        /// reporting a pass over the rooms whose data happened to survive.
        /// </para>
        /// </summary>
        public List<System.Guid> SpaceGuids_HourlySeriesRefused => [.. spaceGuids_HourlySeriesRefused];

        public TextMap TextMap
        {
            get
            {
                return textMap;
            }

            set
            {
                textMap = value;
            }
        }

        public List<TM52ExtendedResult> Calculate_TM52(IEnumerable<Space> spaces, int startHourOfYear = 2880, int endHourOfYear = 6528)
        {
            //The hourly-series refusals ARE cleared here, unlike the ventilation ones: TM52 reads the same
            //two series and so can produce these, whereas it selects no criterion and can produce none of
            //those. Each call therefore reports its own unusable series and never an earlier call's.
            ClearHourlySeriesRefusals();

            if (AnalyticalModel == null || spaces == null)
            {
                return null;
            }

            IndexedDoubles maxIndoorComfortTemperatures = GetMaxIndoorComfortTemperatures();
            IndexedDoubles minIndoorComfortTemperatures = GetMinIndoorComfortTemperatures();

            //ONE resolution index for this call. See Dictionary_Space: the model's space list was rebuilt -
            //and, on this path, every space in it copied - once per assessed room, which made a five
            //thousand room assessment quadratic before a single hourly series had been read.
            Dictionary<System.Guid, Space> dictionary_Space = Dictionary_Space(AnalyticalModel.GetSpaces());

            List<TM52ExtendedResult> result = [];
            foreach (Space space in spaces)
            {
                if (!dictionary_Space.TryGetValue(space.Guid, out Space space_Temp) || space_Temp == null)
                {
                    continue;
                }

                if (!TryGetHourlyValues(space_Temp, out JsonArray jArray_OccupancySensibleGain, out JsonArray jArray_ResultantTemperature))
                {
                    continue;
                }

                Collect(
                    jArray_OccupancySensibleGain,
                    jArray_ResultantTemperature,
                    maxIndoorComfortTemperatures,
                    minIndoorComfortTemperatures,
                    startHourOfYear,
                    endHourOfYear,
                    out HashSet<int> occupiedHourIndices,
                    out IndexedDoubles minAcceptableTemperatures,
                    out IndexedDoubles maxAcceptableTemperatures,
                    out IndexedDoubles operativeTemperatures);

                result.Add(new TM52ExtendedResult(space_Temp.Name, Source, space.Guid.ToString(), TM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures));
            }

            return result;
        }

        public List<TM59ExtendedResult> Calculate_TM59(IEnumerable<Space> spaces)
        {
            //Cleared even where the call is about to fail, so a stale refusal from an earlier call can never
            //be read as belonging to this one.
            ventilationStrategyRefusals = [];

            ClearHourlySeriesRefusals();

            if (AnalyticalModel == null || spaces == null || textMap == null)
            {
                return null;
            }

            TM59Manager tM59Manager = new(textMap);

            IndexedDoubles maxIndoorComfortTemperatures = GetMaxIndoorComfortTemperatures();
            IndexedDoubles minIndoorComfortTemperatures = GetMinIndoorComfortTemperatures();

            AdjacencyCluster adjacencyCluster = AnalyticalModel.AdjacencyCluster;

            //ONE resolution index for this call, built from the single GetSpaces() the loop used to make per
            //room. See Dictionary_Space.
            Dictionary<System.Guid, Space> dictionary_Space = Dictionary_Space(adjacencyCluster?.GetSpaces());

            List<TM59ExtendedResult> result = [];
            foreach (Space space in spaces)
            {
                if (!dictionary_Space.TryGetValue(space.Guid, out Space space_Temp) || space_Temp == null)
                {
                    continue;
                }

                if (!TryGetVentilationStrategy(adjacencyCluster, space_Temp, out string systemTypeName))
                {
                    //Refused, and the reason is recorded. No criterion applies, so no result is produced -
                    //deliberately, in place of the "NV" default that made this the wrong assessment rather
                    //than an absent one.
                    continue;
                }

                //The applications come from the space the MODEL now holds - space_Temp - not from the space
                //instance the caller listed. Explicitly scoped assessments receive the map's retained
                //simulation-space instances, which predate RestoreDesignInternalConditions, so the caller's
                //`space` can still carry no internal condition while space_Temp carries the restored design
                //one. Classifying the stale instance would pick the wrong TM59 result type or corridor
                //fallback.
                List<TM59SpaceApplication> tM59SpaceApplications = tM59Manager.TM59SpaceApplications(space_Temp?.InternalCondition);
                if (tM59SpaceApplications == null || tM59SpaceApplications.Count == 0)
                {
                    tM59SpaceApplications = tM59Manager.TM59SpaceApplications(space_Temp);
                }

                if (!TryGetHourlyValues(space_Temp, out JsonArray jArray_OccupancySensibleGain, out JsonArray jArray_ResultantTemperature))
                {
                    continue;
                }

                Collect(
                    jArray_OccupancySensibleGain,
                    jArray_ResultantTemperature,
                    maxIndoorComfortTemperatures,
                    minIndoorComfortTemperatures,
                    int.MinValue,
                    int.MaxValue,
                    out HashSet<int> occupiedHourIndices,
                    out IndexedDoubles minAcceptableTemperatures,
                    out IndexedDoubles maxAcceptableTemperatures,
                    out IndexedDoubles operativeTemperatures);

                TM59ExtendedResult tM59ExtendedResult;

                if (tM59SpaceApplications == null || tM59SpaceApplications.Count == 0 || (!string.IsNullOrWhiteSpace(systemTypeName) && systemTypeName.Equals("UV")))
                {
                    tM59ExtendedResult = new TM59CorridorExtendedResult(space_Temp.Name, Source, space.Guid.ToString(), TM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures);
                }
                else if (!string.IsNullOrWhiteSpace(systemTypeName) && systemTypeName.Equals("NV"))
                {
                    tM59ExtendedResult = tM59SpaceApplications.Contains(TM59SpaceApplication.Sleeping)
                        ? new TM59NaturalVentilationBedroomExtendedResult(space_Temp.Name, Source, space.Guid.ToString(), TM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
                        : new TM59NaturalVentilationExtendedResult(space_Temp.Name, Source, space.Guid.ToString(), TM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, tM59SpaceApplications?.ToArray());
                }
                else
                {
                    tM59ExtendedResult = new TM59MechanicalVentilationExtendedResult(space_Temp.Name, Source, space.Guid.ToString(), TM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, tM59SpaceApplications?.ToArray());
                }

                if (tM59ExtendedResult == null)
                {
                    continue;
                }

                result.Add(tM59ExtendedResult);
            }

            return result;
        }

        public IndexedDoubles GetMaxIndoorComfortTemperatures(Period period = Period.Hourly)
        {
            WeatherYear weatherYear = WeatherYear();

            List<double> values = weatherYear == null ? null : Query.MaxIndoorComfortTemperatures(weatherYear, TM52BuildingCategory);

            return values == null || values.Count == 0 ? null : new IndexedDoubles(values).Repeat(period, Period.Daily);
        }

        public IndexedDoubles GetMaxIndoorComfortTemperatures(int startDayIndex, int endDayIndex, Period period = Period.Hourly)
        {
            WeatherYear weatherYear = WeatherYear();

            List<double> values = weatherYear == null ? null : Query.MaxIndoorComfortTemperatures(weatherYear, TM52BuildingCategory, startDayIndex, endDayIndex);

            return values == null || values.Count == 0 ? null : new IndexedDoubles(values, startDayIndex).Repeat(period, Period.Daily);
        }

        public IndexedDoubles GetMinIndoorComfortTemperatures(Period period = Period.Hourly)
        {
            WeatherYear weatherYear = WeatherYear();

            List<double> values = weatherYear == null ? null : Query.MinIndoorComfortTemperatures(weatherYear, TM52BuildingCategory);

            return values == null || values.Count == 0 ? null : new IndexedDoubles(values).Repeat(period, Period.Daily);
        }

        public IndexedDoubles GetMinIndoorComfortTemperatures(int startDayIndex, int endDayIndex, Period period = Period.Hourly)
        {
            WeatherYear weatherYear = WeatherYear();

            List<double> values = weatherYear == null ? null : Query.MinIndoorComfortTemperatures(weatherYear, TM52BuildingCategory, startDayIndex, endDayIndex);

            return values == null || values.Count == 0 ? null : new IndexedDoubles(values, startDayIndex).Repeat(period, Period.Daily);
        }

        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        /// <summary>
        /// One assessment's space identities, indexed once.
        ///
        /// <para><b>What it replaces, and why that was quadratic</b></para>
        /// <para>
        /// Both assessments resolve every room they are asked about against the model before reading it -
        /// correctly, because the caller is handed the simulated space instances the map retained and those
        /// predate <c>TM59AssessmentCalculator.RestoreDesignInternalConditions</c>, so the instance the model
        /// now holds is the one that carries the restored design internal condition. The resolution was
        /// <c>GetSpaces().Find(...)</c>, taken <b>inside</b> the loop over the rooms:
        /// <c>AdjacencyCluster.GetSpaces()</c> rebuilds the whole space list on every call, and the TM52 path
        /// went through <c>AnalyticalModel.GetSpaces()</c>, which additionally <i>copies every space in the
        /// model</i>. A five thousand room assessment therefore built five thousand space lists - and, on
        /// TM52, twenty five million <c>Space</c> copies - before a single hourly series had been read.
        /// </para>
        ///
        /// <para><b>Identity only, and request scoped</b></para>
        /// <para>
        /// This holds which instance the model carries for a guid and nothing else - no series, no
        /// classification, no criterion, no result. It is built at the top of one <c>Calculate_*</c> call
        /// and dropped when that call returns; nothing on either loop writes to the model or to a space, so
        /// it cannot go stale within the traversal it serves. It is the same contract
        /// <see cref="PartFIndex"/> states, for the same reason. Nothing is cached between calls: two
        /// assessments of one model cost exactly twice one.
        /// </para>
        /// <para>
        /// <b>First occurrence wins</b>, because <c>List.Find</c> - which every one of these resolutions was
        /// - returns the first match, so a model that somehow carried two spaces on one guid keeps answering
        /// what it answered before.
        /// </para>
        /// </summary>
        private static Dictionary<System.Guid, Space> Dictionary_Space(IEnumerable<Space> spaces)
        {
            Dictionary<System.Guid, Space> result = [];

            foreach (Space space in spaces ?? [])
            {
                //A null cannot reach here from GetSpaces - RelationCluster.GetObjects<T> keeps only what IS
                //a T - and the Find this replaces would have thrown on one. Skipped rather than indexed so
                //a caller-supplied list cannot make a null the answer for a guid.
                if (space != null && !result.ContainsKey(space.Guid))
                {
                    result[space.Guid] = space;
                }
            }

            return result;
        }

        private WeatherYear WeatherYear()
        {
            return AnalyticalModel != null && AnalyticalModel.TryGetValue(AnalyticalModelParameter.WeatherData, out WeatherData weatherData) && weatherData != null
                ? weatherData.WeatherYears?.FirstOrDefault()
                : null;
        }

        /// <summary>
        /// Both hourly series, in a state they can be assessed together in - or false, with the reason on
        /// <see cref="HourlySeriesRefusals"/> and the space on
        /// <see cref="SpaceGuids_HourlySeriesRefused"/>.
        /// <para>
        /// Four states are refused: either series absent, either series empty, and the two series of
        /// different lengths. See <see cref="HourlySeriesRefusals"/> for why unequal lengths are a refusal
        /// rather than a shared-range assessment, and why length equality is all that is judged here.
        /// </para>
        /// <para>
        /// A refusal is REPORTED, never thrown. One space with unusable data must not cost every other
        /// space in the building its assessment - that was the reason the walk was bounded by the shorter
        /// series in the first place, and it still holds.
        /// </para>
        /// </summary>
        private bool TryGetHourlyValues(Space space, out JsonArray jsonArray_OccupancySensibleGain, out JsonArray jsonArray_ResultantTemperature)
        {
            jsonArray_ResultantTemperature = null;

            bool hasOccupancy = Core.Query.TryGetValue(space, OccupancySensibleGainSeriesKey, out jsonArray_OccupancySensibleGain)
                && jsonArray_OccupancySensibleGain != null;

            bool hasResultant = Core.Query.TryGetValue(space, ResultantTemperatureSeriesKey, out jsonArray_ResultantTemperature)
                && jsonArray_ResultantTemperature != null;

            if (!hasOccupancy || !hasResultant)
            {
                //Which one is missing, because "no result for this room" with no reason is what this
                //replaces and the two have different causes: the occupancy key is the one the analytical and
                //TAS vocabularies disagree about (see OccupancySensibleGainSeriesKey), so a whole model
                //missing only that one is a key that was not supplied rather than a damaged results file.
                RefuseHourlySeries(space, string.Format(
                    "Space '{0}' carries no {1} hourly series, so it could not be assessed and was left out.",
                    space?.Name ?? "?",
                    !hasOccupancy && !hasResultant
                        ? string.Format("'{0}' or '{1}'", OccupancySensibleGainSeriesKey, ResultantTemperatureSeriesKey)
                        : string.Format("'{0}'", hasOccupancy ? ResultantTemperatureSeriesKey : OccupancySensibleGainSeriesKey)));

                return false;
            }

            if (jsonArray_OccupancySensibleGain.Count == 0 || jsonArray_ResultantTemperature.Count == 0)
            {
                RefuseHourlySeries(space, string.Format(
                    "Space '{0}' carries an EMPTY hourly series ('{1}' has {2} values, '{3}' has {4}), so there is nothing to assess and it was left out. An empty series is a results file that was not written for this room, not a room with no exceedances.",
                    space?.Name ?? "?",
                    OccupancySensibleGainSeriesKey,
                    jsonArray_OccupancySensibleGain.Count,
                    ResultantTemperatureSeriesKey,
                    jsonArray_ResultantTemperature.Count));

                return false;
            }

            if (jsonArray_OccupancySensibleGain.Count != jsonArray_ResultantTemperature.Count)
            {
                RefuseHourlySeries(space, string.Format(
                    "Space '{0}' carries hourly series of different lengths ('{1}' has {2} values, '{3}' has {4}), so one of them is truncated and which is not knowable. It was refused rather than assessed over the {5} hours the two share, because an overheating verdict over part of a room's hours is not a verdict about that room.",
                    space?.Name ?? "?",
                    OccupancySensibleGainSeriesKey,
                    jsonArray_OccupancySensibleGain.Count,
                    ResultantTemperatureSeriesKey,
                    jsonArray_ResultantTemperature.Count,
                    System.Math.Min(jsonArray_OccupancySensibleGain.Count, jsonArray_ResultantTemperature.Count)));

                return false;
            }

            //Length agreed; is it enough of a year? Only where the caller said what "enough" is - see
            //HourCount_Expected. Both counts are equal here, so either may be compared.
            if (HourCount_Expected > 0 && jsonArray_ResultantTemperature.Count < HourCount_Expected)
            {
                RefuseHourlySeries(space, string.Format(
                    "Space '{0}' carries only {1} of the {2} hourly values this assessment requires, so it was refused rather than assessed over a partial year. Both of its series are this length, so nothing here is a mismatch - the results file itself is short, and neither a pass nor a failure may be produced from part of a year.",
                    space?.Name ?? "?",
                    jsonArray_ResultantTemperature.Count,
                    HourCount_Expected));

                return false;
            }

            //Length agreed and long enough; is every hour of it actually a number? Only where a full year
            //was asked for - see below for why the answer differs from the ordinary path's.
            if (HourCount_Expected > 0)
            {
                if (!IsUsableSeries(jsonArray_ResultantTemperature, HourCount_Expected, out int index_Unusable))
                {
                    RefuseHourlySeries(space, string.Format(
                        "Space '{0}' carries an unusable value in its '{1}' hourly series at hour {2} - it is absent, not a number, or not finite. The series is the right length, so the hour is missing rather than the file being short, and the room was refused rather than assessed over the hours that survived.",
                        space?.Name ?? "?",
                        ResultantTemperatureSeriesKey,
                        index_Unusable));

                    return false;
                }

                if (!IsUsableSeries(jsonArray_OccupancySensibleGain, HourCount_Expected, out index_Unusable))
                {
                    RefuseHourlySeries(space, string.Format(
                        "Space '{0}' carries an unusable value in its '{1}' hourly series at hour {2} - it is absent, not a number, or not finite. An hour with NO stated occupancy is not an unoccupied hour: it is an hour nothing is known about, and counting it as empty would quietly shrink the occupied hours a TM59 verdict is measured over.",
                        space?.Name ?? "?",
                        OccupancySensibleGainSeriesKey,
                        index_Unusable));

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether every value in the series is a finite number, and the first index that is not.
        ///
        /// <para><b>Why this is asked only where a full year was requested</b></para>
        /// <para>
        /// <see cref="Collect"/> skips an hour whose resultant temperature will not convert or is NaN, and
        /// treats an occupancy value it cannot read as an unoccupied hour. Both are the right behaviour for
        /// an ordinary TM52/TM59 run over whatever a caller has: an hour with no data is not evidence of an
        /// exceedance, and refusing a whole room over one gap would lose an assessment that is substantially
        /// sound.
        /// </para>
        /// <para>
        /// It is the wrong behaviour for Approved Document O, and silently so, because the criteria are
        /// COUNTS. A room whose occupancy series has a hundred unreadable hours is assessed over a hundred
        /// fewer occupied hours than it really has, and the proportion of them it may exceed in shrinks to
        /// match - so the verdict is computed against a year the building never had, and reads as a normal
        /// pass. The length check alone does not catch it: the file is exactly the right size.
        /// </para>
        /// <para>
        /// <b>Zero occupancy is a value.</b> An empty hour is stated and usable; what is refused is an hour
        /// that states nothing - a JSON null, a string, a NaN or an infinity - which is a different fact and
        /// must not be read as "nobody was in".
        /// </para>
        /// </summary>
        private static bool IsUsableSeries(JsonArray jsonArray, int hourCount_Expected, out int index_Unusable)
        {
            index_Unusable = -1;

            //ONLY the hours the assessment requires. A series LONGER than the requested year - a leap-year
            //simulation's 8784 against a 365-day one - is deliberately accepted, and Collect then excludes
            //the surplus hours because the comfort band does not cover them. Validating them here would
            //refuse the whole room over an hour that is never assessed, which contradicts the rule this
            //check is part of.
            int count = System.Math.Min(jsonArray.Count, hourCount_Expected);

            for (int i = 0; i < count; i++)
            {
                //The node's own JSON kind FIRST, and not only whether it converts. Core.Query.TryConvert
                //routes a JSON boolean through its bool-to-double conversion, so `true` reads as 1 and
                //`false` as 0 - which means a corrupted series of booleans converted cleanly, passed as
                //finite numbers, and produced an ordinary Part O verdict from occupancy and temperatures
                //that were never measurements at all. An hourly value has to BE a number.
                //
                //Asked here and not in TryGetHourlyValue, which Collect shares: on the ordinary path a value
                //that converts is read exactly as it always was, and this stricter rule applies only where a
                //caller has asked for a complete year of evidence.
                if (!IsUsableHour(jsonArray[i]))
                {
                    index_Unusable = i;

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Whether one hour of a series is a genuine finite numeric measurement.
        /// <para>
        /// The node's own JSON kind is asked FIRST, and not only whether it converts.
        /// <c>Core.Query.TryConvert</c> routes a JSON boolean through its bool-to-double conversion, so
        /// <c>true</c> reads as 1 and <c>false</c> as 0 - which means a corrupted series of booleans
        /// converted cleanly, passed as finite numbers, and produced an ordinary verdict from occupancy and
        /// temperatures that were never measurements at all. An hourly value has to <b>be</b> a number.
        /// </para>
        /// <para>
        /// <c>GetValueKind()</c> is itself guarded, because it throws on the NaN and infinity a
        /// <c>JsonArray</c> will hold - it has to decide how the value would serialize, and those do not.
        /// See <see cref="TryGetHourlyValue"/>.
        /// </para>
        /// <para>
        /// Asked here and not in <see cref="TryGetHourlyValue"/>, which <see cref="Collect"/> shares: on the
        /// ordinary path a value that converts is read exactly as it always was, and this stricter rule
        /// applies only where a caller has asked for a complete year of evidence.
        /// </para>
        /// </summary>
        private static bool IsUsableHour(JsonNode jsonNode)
        {
            if (jsonNode is not JsonValue jsonValue)
            {
                return false;
            }

            try
            {
                if (jsonValue.GetValueKind() != System.Text.Json.JsonValueKind.Number)
                {
                    return false;
                }
            }
            catch (System.ArgumentException)
            {
                return false;
            }

            return TryGetHourlyValue(jsonValue, out double _);
        }

        /// <summary>
        /// One hour of a series as a usable number, or false - absent, not a number, or not finite.
        ///
        /// <para><b>Why the read is guarded rather than direct</b></para>
        /// <para>
        /// A <c>JsonArray</c> WILL hold a NaN or an infinity: <c>JsonValue.Create(double.NaN)</c> succeeds
        /// and so does storing it. Reading it back is what fails - <c>System.Text.Json</c> throws
        /// <see cref="System.ArgumentException"/> ("cannot be written as valid JSON") from the conversion,
        /// because it will not serialize the value it is being asked to hand over.
        /// </para>
        /// <para>
        /// So a single unrepresentable hour anywhere in a building threw out of
        /// <see cref="Calculate_TM59"/> and lost EVERY space's assessment - the same failure the
        /// shared-range walk in <see cref="Collect"/> was written to avoid, arriving by a different door.
        /// Caught here, one bad hour costs at most its own room.
        /// </para>
        /// <para>
        /// The rejection is the same either way: <see cref="Collect"/> already skipped an hour whose
        /// temperature would not convert or was NaN, and this changes none of that - it stops the read
        /// throwing before the skip can happen. Whether a skipped hour is tolerable at all is the separate
        /// question <see cref="HourCount_Expected"/> answers.
        /// </para>
        /// </summary>
        private static bool TryGetHourlyValue(JsonNode jsonNode, out double value)
        {
            value = double.NaN;

            try
            {
                if (!Core.Query.TryConvert(jsonNode, out value))
                {
                    return false;
                }
            }
            catch (System.ArgumentException)
            {
                value = double.NaN;

                return false;
            }

            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        /// Empties the hourly-series record, so one calculation never reports another's refusals. Called at
        /// the top of both calculations, before either can fail.
        /// </summary>
        private void ClearHourlySeriesRefusals()
        {
            hourlySeriesRefusals = [];
            spaceGuids_HourlySeriesRefused = [];
        }

        /// <summary>Records one space's hourly-series refusal - the sentence and the identity together.</summary>
        private void RefuseHourlySeries(Space space, string refusal)
        {
            hourlySeriesRefusals.Add(refusal);

            if (space != null)
            {
                spaceGuids_HourlySeriesRefused.Add(space.Guid);
            }
        }

        /// <summary>
        /// Walks the hourly series once, collecting the operative temperature, the comfort band and which
        /// hours were occupied. An hour counts as occupied when the occupancy gain is above zero.
        /// </summary>
        private static void Collect(
            JsonArray jsonArray_OccupancySensibleGain,
            JsonArray jsonArray_ResultantTemperature,
            IndexedDoubles maxIndoorComfortTemperatures,
            IndexedDoubles minIndoorComfortTemperatures,
            int startHourOfYear,
            int endHourOfYear,
            out HashSet<int> occupiedHourIndices,
            out IndexedDoubles minAcceptableTemperatures,
            out IndexedDoubles maxAcceptableTemperatures,
            out IndexedDoubles operativeTemperatures)
        {
            occupiedHourIndices = [];
            minAcceptableTemperatures = new IndexedDoubles();
            maxAcceptableTemperatures = new IndexedDoubles();
            operativeTemperatures = new IndexedDoubles();

            //The loop is bounded by the SHORTER of the two series, and both callers now REFUSE a space whose
            //series are of different lengths before reaching here - see TryGetHourlyValues and
            //HourlySeriesRefusals. So this bound is no longer what decides such a space's verdict; it is
            //kept as the defence it originally was, because the loop indexes both arrays by one counter and
            //a future caller reaching this private method without that check must still not throw out of
            //the whole TM52/TM59 run and lose every assessment in it.
            //
            //It is NOT a substitute for the refusal, and must not be relied on as one: silently assessing a
            //room over the hours two unequal series happen to share is the defect the refusal removes.
            int count = System.Math.Min(jsonArray_OccupancySensibleGain.Count, jsonArray_ResultantTemperature.Count);

            for (int i = 0; i < count; i++)
            {
                if (i < startHourOfYear || i > endHourOfYear)
                {
                    continue;
                }

                //Guarded, because reading a NaN or infinity node THROWS rather than returning false - see
                //TryGetHourlyValue. An hour that cannot be read is skipped, exactly as an unconvertible or
                //NaN one always was.
                if (!TryGetHourlyValue(jsonArray_ResultantTemperature[i], out double resultantTemperature))
                {
                    continue;
                }

                //Both comfort bounds must EXIST for the hour. IndexedDoubles returns 0 for a missing index,
                //and the comfort series is bounded by the weather year (365 days = 8760 hours) - so a
                //leap-year simulation's extra 24 hours would otherwise be assessed against a 0 degC comfort
                //limit and manufacture exceedances. An hour with no comfort bounds is simply not assessed.
                if (!maxIndoorComfortTemperatures.TryGetValue(i, out double maxIndoorComfortTemperature) || !minIndoorComfortTemperatures.TryGetValue(i, out double minIndoorComfortTemperature))
                {
                    continue;
                }

                maxAcceptableTemperatures.Add(i, maxIndoorComfortTemperature);
                minAcceptableTemperatures.Add(i, minIndoorComfortTemperature);
                operativeTemperatures.Add(i, resultantTemperature);

                if (!TryGetHourlyValue(jsonArray_OccupancySensibleGain[i], out double occupancySensibleGain))
                {
                    continue;
                }

                if (occupancySensibleGain <= 0)
                {
                    continue;
                }

                occupiedHourIndices.Add(i);
            }
        }

        /// <summary>
        /// The ventilation strategy the TM59 criterion selection uses for a space: the scenario's, where
        /// <see cref="VentilationStrategyMap"/> was supplied, and otherwise the old derivation.
        /// <para>
        /// <b>The two paths do not blend.</b> With a map, a refusal is a refusal - it does not fall through to
        /// <see cref="SystemTypeName"/>, because falling through would restore exactly the defect the map
        /// exists to remove and would do it invisibly, at the one input where nothing was said.
        /// </para>
        /// </summary>
        /// <returns>False where the space must not be assessed at all.</returns>
        private bool TryGetVentilationStrategy(AdjacencyCluster adjacencyCluster, Space space, out string ventilationStrategy)
        {
            if (VentilationStrategyMap == null)
            {
                //No scenario stated. The pre-existing derivation, unchanged, for every caller that has none.
                ventilationStrategy = SystemTypeName(adjacencyCluster, space);

                return true;
            }

            VentilationStrategySelection ventilationStrategySelection = VentilationStrategyMap.Selection(space);

            if (!ventilationStrategySelection.IsSelected)
            {
                ventilationStrategyRefusals.Add(ventilationStrategySelection.Reason);
                ventilationStrategy = null;

                return false;
            }

            ventilationStrategy = ventilationStrategySelection.VentilationStrategy;

            return true;
        }

        /// <summary>
        /// The ventilation system type governing a space as it was derived <b>before a scenario could state
        /// one</b>: the space's own internal condition first, then a system type whose name matches one of the
        /// space's zones, and "NV" where nothing says otherwise.
        /// <para>
        /// <b>Superseded, and kept only for callers with no scenario.</b> Every step of it is unsound as a way
        /// of choosing an Approved Document O criterion. The zone-name lookup makes a dwelling's assessment
        /// turn on whether somebody named a zone after a library entry, and the default silently assesses an
        /// MVRE dwelling as naturally ventilated. Supplying a <see cref="VentilationStrategyMap"/> bypasses
        /// this method entirely; it remains reachable because the Grasshopper and user-interface callers have
        /// no scenario to state yet, and removing it would change their behaviour without giving them a way to
        /// state the right one.
        /// </para>
        /// </summary>
        private static string SystemTypeName(AdjacencyCluster adjacencyCluster, Space space)
        {
            string result = space?.InternalCondition?.GetSystemTypeName<VentilationSystemType>()?.ToUpper();
            if (!string.IsNullOrWhiteSpace(result))
            {
                return result;
            }

            //Null wherever the ambient SAM install has no system type library - a clean CI runner, a headless
            //host, or any process whose ActiveSetting was never seeded from %APPDATA%\SAM. Both loops below
            //dereferenced it, so this threw a NullReferenceException out of the middle of an assessment rather
            //than falling through to the documented "NV" default.
            SystemTypeLibrary systemTypeLibrary = Query.DefaultSystemTypeLibrary();

            List<Zone> zones = systemTypeLibrary == null ? null : adjacencyCluster?.GetRelatedObjects<Zone>(space);
            if (zones != null)
            {
                foreach (Zone zone in zones)
                {
                    VentilationSystemType ventilationSystemType = systemTypeLibrary.GetSystemTypes<VentilationSystemType>(zone.Name, TextComparisonType.Equals, true)?.FirstOrDefault();
                    if (ventilationSystemType != null)
                    {
                        return ventilationSystemType.Name.ToUpper().Trim();
                    }
                }

                foreach (Zone zone in zones)
                {
                    VentilationSystemType ventilationSystemType = systemTypeLibrary.GetSystemTypes<VentilationSystemType>(zone.Name, TextComparisonType.StartsWith, false)?.FirstOrDefault();
                    if (ventilationSystemType != null)
                    {
                        return ventilationSystemType.Name.ToUpper().Trim();
                    }
                }
            }

            return "NV";
        }
    }
}
