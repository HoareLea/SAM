// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Assesses the ventilation of NEW dwellings against Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England, in effect from 15 June 2022), for a balanced mechanical
    /// ventilation with heat recovery arrangement.
    ///
    /// <para><b>What it sizes.</b> Whole dwelling rates come from Table 1.3 and paragraph 1.24, including
    /// note 1 for a dwelling with exactly one habitable room; wet room minimum extract rates from
    /// Table 1.2; and supply is distributed in proportion to habitable room volume per paragraph 1.67.</para>
    ///
    /// <para><b>Terminals, not rooms.</b> The unit of the calculation is the terminal. A room can require
    /// more than one: Appendix A (page 36) makes a studio and an open plan living kitchen habitable rooms,
    /// because neither is <i>solely</i> a kitchen, so paragraph 1.67 requires mechanical supply to them,
    /// and both contain the cooking function, so paragraph 1.17a and Table 1.2 require kitchen extract
    /// from them as well. Both terminals are established, sized and reported. Earlier versions of this
    /// calculation assigned one role per space and could only report the missing local kitchen extract as
    /// a warning.</para>
    ///
    /// <para><b>Operating conditions.</b> Every rate exists at its own condition and they are never
    /// combined. The <b>continuous design</b> rate is the Approved Document F sizing condition, taken as
    /// the greater of the bedroom or one-habitable-room rate and the floor area rate, and nothing else -
    /// see below. The <b>high</b> rate is the Table 1.2 condition, for when additional extraction is
    /// required. The <b>setback</b> rate is a SAM reduced-operation convention obtained by scaling the
    /// continuous design rates, and never alters them. <b>Measured</b> rates are commissioning evidence
    /// and never overwrite a design value.</para>
    ///
    /// <para><b>Table 1.2 is two separate requirements.</b> Its continuous column requires the
    /// <i>total</i> of continuous extract to reach the whole dwelling ventilation rate. Its per-room
    /// figures are minimum <i>high</i> rates, each assessed on its own room at the high condition. The
    /// continuous dwelling rate is therefore NOT raised to the sum of the per-room high-rate minimums:
    /// nothing in the Approved Document asks for that, note 1 says only that a room already continuously
    /// at or above its own minimum needs no further increase, and summing them would systematically
    /// oversize normal continuous operation in any dwelling with several wet rooms. Where the sum does
    /// exceed the whole dwelling rate, the continuous total stays at the whole dwelling rate and each
    /// room reaches its own minimum by boosting.</para>
    ///
    /// <para><b>Scope: new dwellings only.</b> Section 3 of the Approved Document, covering work on
    /// existing dwellings, is not implemented, and there is deliberately no new/existing switch. Only
    /// mechanical ventilation with heat recovery (paragraphs 1.67 to 1.73) is sized - natural ventilation
    /// with intermittent extract (Table 1.1) and continuous mechanical extract ventilation (paragraphs
    /// 1.60 to 1.66) are not implemented as system types, although Table 1.1 rates are applied to an
    /// individual intermittent extract device such as a cooker hood.</para>
    ///
    /// <para>
    /// Use <see cref="Calculate(IEnumerable{Space})"/> with no argument to size the whole model as one
    /// dwelling, or <see cref="Calculate(string)"/> with a zone category name to size each zone in that
    /// category independently as its own flat or dwelling.
    /// </para>
    ///
    /// <para>
    /// Rooms are recognised through the shared semantic classification layer
    /// (<see cref="SpaceSemanticsResolver"/>), so Approved Document F, Approved Document O, CIBSE TM59
    /// and the SAM_UI internal condition mapping all agree on what a space is.
    /// </para>
    ///
    /// <para><b>This is an assessment, not a certification.</b> Software cannot certify compliance with
    /// the Building Regulations. What <see cref="PartFDwellingResult.ComplianceResult"/> records is which
    /// requirements were calculated, which were verified from the model geometry, which a person
    /// confirmed, and which remain open.</para>
    /// </summary>
    public class PartFCalculator
    {
        private readonly PartFData partFData;

        public PartFCalculator(PartFData partFData)
        {
            this.partFData = partFData;
        }

        public AdjacencyCluster AdjacencyCluster { get; set; }

        private double? setbackFlowRateFactor = null;

        private PartFExtractAllocationStrategy? extractAllocationStrategy = null;

        /// <summary>
        /// Setback operating factor used by this calculation: setback rate = continuous design rate x
        /// factor. Defaults to the rule set's own factor, which is 30% of continuous design unless the
        /// rule set says otherwise.
        /// <para>
        /// Setting this overrides the rule set for this calculator only, so a caller can offer the factor
        /// as an input without mutating the shared default rule set held by ActiveSetting. Only a factor
        /// greater than 0 and no greater than 1 is accepted; zero, a negative value, a value above 1, NaN
        /// and infinity are replaced by <see cref="PartFData.DefaultSetbackFlowRateFactor"/>.
        /// </para>
        /// <para>
        /// A SAM reduced-operation convention. It is applied only after every Approved Document F minimum
        /// has been established at the continuous design condition, and never changes a design rate.
        /// </para>
        /// </summary>
        public double SetbackFlowRateFactor
        {
            get
            {
                return setbackFlowRateFactor ?? partFData?.SetbackFlowRateFactor ?? PartFData.DefaultSetbackFlowRateFactor;
            }

            set
            {
                setbackFlowRateFactor = PartFData.IsValidSetbackFlowRateFactor(value) ? value : PartFData.DefaultSetbackFlowRateFactor;
            }
        }

        /// <summary>
        /// How continuous extract above the Table 1.2 minimums is shared between the dwelling's extract
        /// terminals. Defaults to the rule set's own strategy.
        /// <para>
        /// Approved Document F prescribes only two things about extract totals: each wet room reaches at
        /// least its Table 1.2 minimum high rate, and the total of all continuous extract is at least the
        /// whole dwelling ventilation rate. The split of the surplus is an engineering strategy, so it is
        /// selectable and is recorded on every result.
        /// </para>
        /// </summary>
        public PartFExtractAllocationStrategy ExtractAllocationStrategy
        {
            get
            {
                return extractAllocationStrategy ?? partFData?.ExtractAllocationStrategy ?? PartFExtractAllocationStrategy.MinimumFirstCookingPriority;
            }

            set
            {
                extractAllocationStrategy = value;
            }
        }

        /// <summary>
        /// Commissioning evidence supplied to the calculation, keyed by dwelling name. Use an empty string
        /// for the single dwelling mode, where the whole model is one dwelling and there is no zone name.
        /// <para>
        /// A zone that carries its own <see cref="ZoneParameter.PartFCommissioningData"/> takes precedence
        /// over an entry here, so evidence stored in the model always wins over evidence passed in for one
        /// run.
        /// </para>
        /// </summary>
        public Dictionary<string, PartFCommissioningData> CommissioningData { get; set; } = [];

        /// <summary>One entry per dwelling sized by the most recent calculation.</summary>
        public List<PartFDwellingResult> DwellingResults { get; private set; } = [];

        /// <summary>
        /// The Part F conformance assessment of each dwelling sized by the most recent calculation, in the
        /// same order as <see cref="DwellingResults"/>.
        /// </summary>
        public List<PartFComplianceResult> ComplianceResults
        {
            get { return [.. DwellingResults.ConvertAll(x => x.ComplianceResult).FindAll(x => x is not null)]; }
        }

        /// <summary>
        /// Conditions from the most recent calculation that need the engineer's attention. Messages
        /// are prefixed with the dwelling zone name where the calculation was grouped by zone.
        /// </summary>
        public List<string> Warnings { get; private set; } = [];

        /// <summary>Informational notes from the most recent calculation.</summary>
        public List<string> Remarks { get; private set; } = [];

        /// <summary>Spaces from the most recent calculation that matched no Part F room category.</summary>
        public List<string> UnclassifiedSpaceNames { get; private set; } = [];

        /// <summary>Classified spaces from the most recent calculation that received no flow rate.</summary>
        public List<string> UnassignedSpaceNames { get; private set; } = [];

        /// <summary>
        /// Zones of the selected category that were not sized because they are not dwellings, from the
        /// most recent calculation.
        /// </summary>
        public List<string> ExcludedZoneNames { get; private set; } = [];

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] at the continuous design condition of the last dwelling
        /// sized. In single dwelling mode this is the rate for the whole model; in zoned mode use
        /// <see cref="DwellingResults"/>.
        /// </summary>
        public double? FinalSystemRate_Lps { get; private set; }

        /// <summary>
        /// Whole dwelling ventilation rate [l/s] at the setback operating condition of the last dwelling
        /// sized.
        /// </summary>
        public double? SetbackSystemRate_Lps { get; private set; }

        /// <summary>
        /// Sizes the supplied spaces, or the whole model when <paramref name="spaces"/> is null, as
        /// a single dwelling.
        /// </summary>
        public bool Calculate(IEnumerable<Space> spaces = null)
        {
            Reset();

            if (partFData is null || AdjacencyCluster is null)
            {
                return false;
            }

            AdjacencyCluster adjacencyCluster = new(AdjacencyCluster, deepClone: true);

            Dictionary<Guid, PartFPurgeVentilationData> dictionary_Purge = ClearStalePartFSpaceData(adjacencyCluster, out Dictionary<(Guid, PartFTerminalRole), (double?, double?)> dictionary_Measured);
            Dictionary<Guid, PartFDoorTransferData> dictionary_DoorTransfer = ReadDoorTransferInputs(adjacencyCluster);

            List<Space> spaces_Selected = Resolve(adjacencyCluster, spaces is null ? adjacencyCluster.GetSpaces() : [.. spaces]);

            CalculateDwelling(adjacencyCluster, null, spaces_Selected, dictionary_Purge, dictionary_Measured, dictionary_DoorTransfer, null);

            AdjacencyCluster = adjacencyCluster;

            return true;
        }

        /// <summary>
        /// Sizes each dwelling zone of the named zone category independently as its own dwelling. When
        /// <paramref name="zoneCategoryName"/> is null or blank the whole model is sized as one
        /// dwelling, which is the correct behaviour for a single house and is not itself a problem.
        /// <para>
        /// A zone category on its own does not identify a dwelling: a shared corridor, a landlord area
        /// or a commercial unit can legitimately sit in the same category as the flats it serves. Zones
        /// are therefore filtered on <see cref="ZoneParameter.IsDwelling"/>:
        /// </para>
        /// <list type="bullet">
        /// <item>where any zone in the category carries the parameter, only zones explicitly set to true
        /// are sized, and zones carrying no value at all are reported;</item>
        /// <item>a zone explicitly set to false is never sized, whatever else the category contains;</item>
        /// <item>where no zone in the category carries the parameter, every zone is sized as before, so
        /// existing models keep working, and a warning recommends marking the dwellings explicitly.</item>
        /// </list>
        /// </summary>
        public bool Calculate(string zoneCategoryName)
        {
            if (string.IsNullOrWhiteSpace(zoneCategoryName))
            {
                return Calculate((IEnumerable<Space>)null);
            }

            Reset();

            if (partFData is null || AdjacencyCluster is null)
            {
                return false;
            }

            AdjacencyCluster adjacencyCluster = new(AdjacencyCluster, deepClone: true);

            Dictionary<Guid, PartFPurgeVentilationData> dictionary_Purge = ClearStalePartFSpaceData(adjacencyCluster, out Dictionary<(Guid, PartFTerminalRole), (double?, double?)> dictionary_Measured);
            Dictionary<Guid, PartFDoorTransferData> dictionary_DoorTransfer = ReadDoorTransferInputs(adjacencyCluster);

            List<Zone> zones = adjacencyCluster.GetZones();
            if (zones is null || zones.Count == 0)
            {
                Warnings.Add(string.Format("The model contains no zones, so zone category '{0}' could not be used. Add zones for the individual dwellings, or leave zoneCategoryName_ empty to size the whole model as one dwelling.", zoneCategoryName));
                AdjacencyCluster = adjacencyCluster;
                return true;
            }

            List<Zone> zones_Category = zones.FindAll(x => x.GetValue<string>(ZoneParameter.ZoneCategory) == zoneCategoryName);
            if (zones_Category.Count == 0)
            {
                Warnings.Add(NoMatchingZoneMessage(zones, zoneCategoryName));
                AdjacencyCluster = adjacencyCluster;
                return true;
            }

            List<Zone> zones_Selected = SelectDwellingZones(zones_Category, zoneCategoryName, out List<Zone> zones_ExplicitlyExcluded);
            if (zones_Selected.Count == 0)
            {
                AdjacencyCluster = adjacencyCluster;
                return true;
            }

            Dictionary<Guid, List<string>> dictionary_Zones = [];
            List<Tuple<Zone, List<Space>>> tuples_Zone = [];

            foreach (Zone zone in zones_Selected)
            {
                List<Space> spaces_Zone = Resolve(adjacencyCluster, adjacencyCluster.GetRelatedObjects<Space>(zone));

                tuples_Zone.Add(new Tuple<Zone, List<Space>>(zone, spaces_Zone));

                foreach (Space space in spaces_Zone)
                {
                    if (!dictionary_Zones.TryGetValue(space.Guid, out List<string> names))
                    {
                        names = [];
                        dictionary_Zones[space.Guid] = names;
                    }

                    names.Add(zone.Name);
                }
            }

            //A space in two selected dwelling zones is sized twice, and the second dwelling silently
            //overwrites the first.
            foreach (KeyValuePair<Guid, List<string>> keyValuePair in dictionary_Zones)
            {
                if (keyValuePair.Value.Count < 2)
                {
                    continue;
                }

                Space space = adjacencyCluster.GetObject<Space>(keyValuePair.Key);
                Warnings.Add(string.Format("Space '{0}' belongs to more than one dwelling zone ({1}). It has been sized once for each, and only the last result is kept. Each space should belong to exactly one dwelling zone.", space?.Name, string.Join(", ", keyValuePair.Value)));
            }

            List<Space> spaces_Unzoned = adjacencyCluster.GetSpaces().FindAll(x => !dictionary_Zones.ContainsKey(x.Guid));

            //A space in a zone explicitly marked Is Dwelling = false (e.g. a shared corridor) is
            //excluded on purpose: SelectDwellingZones already reported the zone-level exclusion as a
            //Remark, so folding its spaces into the generic "unzoned" Warning below would report the
            //same, expected exclusion twice - once correctly as a Remark, once misleadingly as a
            //Warning. Only genuinely unzoned or ambiguous (unmarked-zone) spaces remain a Warning.
            HashSet<Guid> guids_ExplicitlyExcludedSpace = [];
            foreach (Zone zone in zones_ExplicitlyExcluded)
            {
                List<Space> spaces_Zone_Unzoned = Resolve(adjacencyCluster, adjacencyCluster.GetRelatedObjects<Space>(zone)).FindAll(x => !dictionary_Zones.ContainsKey(x.Guid));
                if (spaces_Zone_Unzoned.Count == 0)
                {
                    continue;
                }

                foreach (Space space in spaces_Zone_Unzoned)
                {
                    guids_ExplicitlyExcludedSpace.Add(space.Guid);
                }

                Remarks.Add(string.Format("'{0}' excluded from the Part F calculation because Is Dwelling is set to No: {1} space(s) were given no ventilation properties ({2}).", zone.Name, spaces_Zone_Unzoned.Count, string.Join(", ", spaces_Zone_Unzoned.ConvertAll(x => x.Name))));
            }

            List<string> names_Unzoned = [.. spaces_Unzoned.FindAll(x => !guids_ExplicitlyExcludedSpace.Contains(x.Guid)).ConvertAll(x => x.Name)];
            if (names_Unzoned.Count != 0)
            {
                Warnings.Add(string.Format("{0} space(s) do not belong to any dwelling zone of category '{1}' and were given no ventilation properties: {2}. Shared and landlord areas are expected here; any dwelling space in this list is missing from its flat.", names_Unzoned.Count, zoneCategoryName, string.Join(", ", names_Unzoned)));
            }

            foreach (Tuple<Zone, List<Space>> tuple in tuples_Zone)
            {
                if (tuple.Item2.Count == 0)
                {
                    Warnings.Add(string.Format("Dwelling zone '{0}' contains no spaces and was skipped.", tuple.Item1.Name));
                    continue;
                }

                CalculateDwelling(adjacencyCluster, tuple.Item1.Name, tuple.Item2, dictionary_Purge, dictionary_Measured, dictionary_DoorTransfer, tuple.Item1);
            }

            AdjacencyCluster = adjacencyCluster;

            return true;
        }

        /// <summary>
        /// Applies the explicit dwelling filter to the zones of the selected category. See
        /// <see cref="Calculate(string)"/> for the rules; this reports what it excluded and why.
        /// <paramref name="zones_ExplicitlyExcluded"/> carries the zones marked Is Dwelling = false back
        /// to the caller, so their spaces can be reported as an expected exclusion (a Remark) rather than
        /// folded into the generic "space does not belong to any dwelling zone" Warning.
        /// </summary>
        private List<Zone> SelectDwellingZones(List<Zone> zones_Category, string zoneCategoryName, out List<Zone> zones_ExplicitlyExcluded)
        {
            //The DECISION comes from the shared rule, so that anything needing to know what this calculator
            //would do - the Part F view preset asks exactly that - gets the same answer without a second copy
            //of the policy. What stays here is the REPORTING, which is this class's own business: the
            //classification below only supplies the lists to report on.
            List<Zone> zones_Selected = zones_Category.PartFDwellingZones();

            zones_Category.PartFClassifyDwellingZones(out List<Zone> zones_True, out List<Zone> zones_False, out List<Zone> zones_Unmarked);

            zones_ExplicitlyExcluded = zones_False;

            //No zone carries the parameter: the shared rule preserves the previous category-based behaviour so
            //existing models keep working, but recommend marking the dwellings explicitly.
            if (zones_True.Count == 0 && zones_False.Count == 0)
            {
                Warnings.Add(string.Format("No zone in category '{0}' has an Is Dwelling parameter, so every zone in the category has been sized as a dwelling. Set Is Dwelling on each zone - true for a flat or house, false for a shared corridor, landlord area or commercial unit - so that non-dwelling zones are not sized as dwellings.", zoneCategoryName));
                return zones_Selected;
            }

            if (zones_False.Count != 0)
            {
                ExcludedZoneNames.AddRange(zones_False.ConvertAll(x => x.Name));
                Remarks.Add(string.Format("{0} zone(s) in category '{1}' are marked Is Dwelling = false and were not sized as dwellings: {2}. Size shared and landlord areas to Approved Document F, Volume 2: Buildings other than dwellings.", zones_False.Count, zoneCategoryName, string.Join(", ", zones_False.ConvertAll(x => x.Name))));
            }

            //Mixed: some zones marked, some not. Only an explicit true is treated as a dwelling, and the
            //unmarked zones are reported rather than silently included or silently dropped.
            if (zones_Unmarked.Count != 0)
            {
                ExcludedZoneNames.AddRange(zones_Unmarked.ConvertAll(x => x.Name));
                Warnings.Add(string.Format("{0} zone(s) in category '{1}' have no Is Dwelling parameter while others do, so they were NOT sized as dwellings: {2}. Set Is Dwelling explicitly on every zone in the category.", zones_Unmarked.Count, zoneCategoryName, string.Join(", ", zones_Unmarked.ConvertAll(x => x.Name))));
            }

            if (zones_True.Count == 0)
            {
                Warnings.Add(string.Format("No zone in category '{0}' is marked Is Dwelling = true, so no dwelling was sized. Set Is Dwelling = true on each zone that represents a flat or house.", zoneCategoryName));
            }

            return zones_Selected;
        }

        private static string NoMatchingZoneMessage(List<Zone> zones, string zoneCategoryName)
        {
            List<string> categories = [.. zones.ConvertAll(x => x.GetValue<string>(ZoneParameter.ZoneCategory)).FindAll(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x)];

            string result = string.Format("No zone belongs to zone category '{0}', so no ventilation properties were assigned. ", zoneCategoryName);

            List<string> categories_Case = categories.FindAll(x => string.Equals(x, zoneCategoryName, StringComparison.OrdinalIgnoreCase));
            if (categories_Case.Count != 0)
            {
                return result + string.Format("Zone category names are case sensitive - did you mean '{0}'?", categories_Case[0]);
            }

            if (categories.Count == 0)
            {
                return result + "None of the zones in the model has a Zone Category set.";
            }

            return result + string.Format("Zone categories present in the model: {0}.", string.Join(", ", categories));
        }

        private void Reset()
        {
            DwellingResults = [];
            Warnings = [];
            Remarks = [];
            UnclassifiedSpaceNames = [];
            UnassignedSpaceNames = [];
            ExcludedZoneNames = [];
            FinalSystemRate_Lps = null;
            SetbackSystemRate_Lps = null;
        }

        /// <summary>
        /// Clears any <see cref="SpaceParameter.PartFSpaceData"/> already sitting on the cloned
        /// cluster's spaces, so this calculation starts from a clean slate, and returns the purge
        /// ventilation inputs it found on the way so they can be carried forward.
        /// <para>
        /// A space only has its PartFSpaceData written to during a run that actually sizes it. A space
        /// sized by an earlier run - e.g. under the legacy category-only fallback, before its zone was
        /// marked Is Dwelling = false - but excluded from THIS run keeps its old, now-meaningless
        /// flow rate forever unless something clears it: nothing downstream overwrites a space it never
        /// visits. Clearing every space up front means "not sized this run" and "carries no Part F data"
        /// are the same state, which is what every caller (SAM_UI, Grasshopper) actually reads.
        /// </para>
        /// <para>
        /// The purge record is a different matter. Its openable area, opening type and mechanical purge
        /// capacity are things only a person can supply, and clearing them would silently discard the
        /// engineer's work every time the model was recalculated. They are harvested here and reapplied.
        /// </para>
        /// <para>
        /// <b>So are a terminal's MEASURED rates, for the same reason.</b> <c>BuildTerminals</c> builds a
        /// brand new <see cref="PartFVentilationTerminalRequirement"/> for every terminal on every run - it
        /// has no memory of the previous run's object - so a measured rate recorded on last run's terminal
        /// is gone the moment this method clears the space's old <c>PartFSpaceData</c>, unless it is
        /// harvested here exactly as the purge record is. Losing it silently is worse than losing the purge
        /// record: <c>PartFCheckBuilder</c>'s Appendix C comparison reads these values to decide whether the
        /// installed system was actually verified, so a recalculation that drops them would make a
        /// previously-evidenced dwelling report as unmeasured again.
        /// </para>
        /// </summary>
        private static Dictionary<Guid, PartFPurgeVentilationData> ClearStalePartFSpaceData(AdjacencyCluster adjacencyCluster, out Dictionary<(Guid, PartFTerminalRole), (double? ContinuousDesignFlowRate_Lps, double? HighFlowRate_Lps)> dictionary_Measured)
        {
            Dictionary<Guid, PartFPurgeVentilationData> result = [];
            dictionary_Measured = [];

            foreach (Space space in adjacencyCluster?.GetSpaces() ?? [])
            {
                if (space is null || space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData) is not PartFSpaceData partFSpaceData)
                {
                    continue;
                }

                if (partFSpaceData.Purge is not null)
                {
                    result[space.Guid] = partFSpaceData.Purge;
                }

                foreach (PartFVentilationTerminalRequirement terminal in partFSpaceData.Terminals ?? [])
                {
                    if (terminal is null || (terminal.MeasuredContinuousFlowRate_Lps is null && terminal.MeasuredHighFlowRate_Lps is null))
                    {
                        continue;
                    }

                    //Keyed by role rather than the terminal's name text: the name is derived from the space
                    //name plus a role suffix, so a space rename between runs would otherwise orphan a
                    //measurement that still belongs to the same physical terminal.
                    dictionary_Measured[(space.Guid, terminal.TerminalRole)] = (terminal.MeasuredContinuousFlowRate_Lps, terminal.MeasuredHighFlowRate_Lps);
                }

                space.RemoveValue(SpaceParameter.PartFSpaceData);
                adjacencyCluster.AddObject(space);
            }

            return result;
        }

        /// <summary>
        /// Reads the transfer records already on the model's door apertures, so the engineering inputs a
        /// person supplied - the provided undercut, the provided free area, the transfer device type, any
        /// transfer flow override - survive this recalculation. The records themselves are rewritten;
        /// only the inputs are carried forward.
        /// </summary>
        private static Dictionary<Guid, PartFDoorTransferData> ReadDoorTransferInputs(AdjacencyCluster adjacencyCluster)
        {
            return adjacencyCluster.GetPartFDoorTransferData();
        }

        private static List<Space> Resolve(AdjacencyCluster adjacencyCluster, List<Space> spaces)
        {
            List<Space> result = [];
            if (spaces is null)
            {
                return result;
            }

            foreach (Space space in spaces)
            {
                if (space is null)
                {
                    continue;
                }

                Space space_Cluster = adjacencyCluster.GetObject<Space>(space.Guid);
                if (space_Cluster is not null)
                {
                    result.Add(space_Cluster);
                }
            }

            return result;
        }

        /// <summary>
        /// One space of the dwelling with everything the terminal calculation needs to size it, so the
        /// category, the space and the terminals stay together instead of being carried in parallel lists
        /// that can drift out of step.
        /// </summary>
        private sealed class DwellingSpace
        {
            public PartFCategory PartFCategory;
            public Space Space;
            public List<PartFVentilationTerminalRequirement> Terminals = [];

            public double Volume_M3
            {
                get { return Space?.GetValue<double>(SpaceParameter.Volume) ?? 0; }
            }

            public double Area_M2
            {
                get { return Space?.GetValue<double>(SpaceParameter.Area) ?? 0; }
            }
        }

        private void CalculateDwelling(
            AdjacencyCluster adjacencyCluster,
            string dwellingName,
            List<Space> spaces,
            Dictionary<Guid, PartFPurgeVentilationData> dictionary_Purge,
            Dictionary<(Guid, PartFTerminalRole), (double? ContinuousDesignFlowRate_Lps, double? HighFlowRate_Lps)> dictionary_Measured,
            Dictionary<Guid, PartFDoorTransferData> dictionary_DoorTransfer,
            Zone zone)
        {
            double setbackFlowRateFactor = SetbackFlowRateFactor;

            PartFDwellingResult dwellingResult = new(dwellingName)
            {
                SpaceNames = [.. spaces.ConvertAll(x => x.Name)],
                SetbackFlowRateFactor = setbackFlowRateFactor,
            };

            PartFComplianceResult complianceResult = new(dwellingName)
            {
                ExtractAllocationStrategy = ExtractAllocationStrategy,
            };

            dwellingResult.ComplianceResult = complianceResult;

            DwellingResults.Add(dwellingResult);

            if (spaces.Count == 0)
            {
                Publish(dwellingResult);
                return;
            }

            List<DwellingSpace> dwellingSpaces = Classify(adjacencyCluster, dwellingResult, spaces);

            CalculateWholeDwellingRates(dwellingResult, dwellingSpaces);

            BuildTerminals(dwellingResult, dwellingSpaces, dictionary_Measured);

            AllocateContinuousRates(dwellingResult, dwellingSpaces);

            AllocateHighRates(dwellingResult, dwellingSpaces);

            ApplySetbackRates(dwellingSpaces, setbackFlowRateFactor);

            WriteSpaceData(adjacencyCluster, dwellingSpaces);

            AssessPurgeVentilation(adjacencyCluster, dwellingSpaces, dictionary_Purge, complianceResult);

            //Written after the purge assessment so each space's PartFSpaceData carries its purge record.
            WriteSpaceData(adjacencyCluster, dwellingSpaces);

            CalculateTransferAir(adjacencyCluster, dwellingResult, dwellingSpaces, dictionary_DoorTransfer, setbackFlowRateFactor);

            PopulateComplianceResult(dwellingResult, dwellingSpaces);

            complianceResult.Commissioning = ResolveCommissioningData(zone, dwellingName);

            PartFCheckBuilder.Build(dwellingResult, partFData);

            complianceResult.Resolve();

            FinalSystemRate_Lps = dwellingResult.ContinuousDesignSystemRate_Lps;
            SetbackSystemRate_Lps = dwellingResult.SetbackSystemRate_Lps;

            Publish(dwellingResult);
        }

        // ------------------------------------------------------------------
        // Classification
        // ------------------------------------------------------------------

        private List<DwellingSpace> Classify(AdjacencyCluster adjacencyCluster, PartFDwellingResult dwellingResult, List<Space> spaces)
        {
            List<DwellingSpace> result = [];

            foreach (Space space in spaces)
            {
                PartFCategory partFCategory = partFData.GetPartFCategory(space, out SpaceSemantics spaceSemantics);

                //Record the shared classification on the space whether or not Part F has a category for
                //it, so the mapping is visible in SAM_UI and reusable by Part O and TM59.
                if (spaceSemantics is not null)
                {
                    space.SetValue(SpaceParameter.SpaceSemantics, spaceSemantics);
                    adjacencyCluster.AddObject(space);
                }

                //A space positively identified as belonging to no dwelling is excluded from this
                //dwelling entirely rather than sized as part of it. Being unclassified is NOT such an
                //identification - an unrecognised name is a reporting problem, not evidence the space is
                //communal - so this only catches communal circulation and explicitly non-dwelling uses.
                if (spaceSemantics is not null && spaceSemantics.SpaceUse != SpaceUse.Undefined && !spaceSemantics.IsDwellingSpace)
                {
                    dwellingResult.NonDwellingSpaceNames.Add(space.Name);
                    continue;
                }

                if (partFCategory is null)
                {
                    dwellingResult.UnclassifiedSpaceNames.Add(space.Name);
                    continue;
                }

                result.Add(new DwellingSpace { PartFCategory = partFCategory, Space = space });
            }

            if (dwellingResult.NonDwellingSpaceNames.Count != 0)
            {
                dwellingResult.Warnings.Add(string.Format("{0} space(s) are not part of any dwelling and were excluded from this dwelling entirely - they do not count towards the bedroom count, the internal floor area or the flow rates, and no transfer air is routed through them: {1}. Move shared circulation and landlord areas out of the dwelling zone and size them to Approved Document F, Volume 2: Buildings other than dwellings.", dwellingResult.NonDwellingSpaceNames.Count, string.Join(", ", dwellingResult.NonDwellingSpaceNames)));
            }

            if (dwellingResult.UnclassifiedSpaceNames.Count != 0)
            {
                dwellingResult.Warnings.Add(string.Format("{0} space(s) could not be matched to a Part F room category and were left out of the dwelling entirely - they do not count towards the bedroom count, the internal floor area or the flow rates, and no transfer air is routed through them: {1}. Rename them, set a Space Use Override on them, or add a matching synonym to the space use text map.", dwellingResult.UnclassifiedSpaceNames.Count, string.Join(", ", dwellingResult.UnclassifiedSpaceNames)));
            }

            return result;
        }

        // ------------------------------------------------------------------
        // Whole dwelling rates
        // ------------------------------------------------------------------

        private void CalculateWholeDwellingRates(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces)
        {
            //Habitable room count. Appendix A (page 36): a habitable room is used for dwelling purposes
            //but is not SOLELY a kitchen, utility room, bathroom, cellar or sanitary accommodation. So a
            //studio and an open plan living kitchen are habitable, a room that is solely a kitchen is not,
            //and a bathroom, ensuite, utility room, sanitary accommodation, circulation space, store,
            //plant room or void never increases this count.
            List<DwellingSpace> dwellingSpaces_Habitable = dwellingSpaces.FindAll(x => x.PartFCategory.PartFType == PartFType.Habitable);

            dwellingResult.HabitableRoomCount = dwellingSpaces_Habitable.Count;
            dwellingResult.HabitableRoomNames = [.. dwellingSpaces_Habitable.ConvertAll(x => x.Space.Name)];

            //Table 1.3 (page 10): minimum whole dwelling rate by number of bedrooms. A studio counts as
            //one bedroom because it combines sleeping with living and cooking in a single room.
            dwellingResult.BedroomCount = dwellingSpaces.FindAll(x => x.PartFCategory.IsBedroom).Count;

            //Table 1.3 note 1 (page 10): "If the dwelling only has one habitable room, a minimum
            //ventilation rate of 13l/s should be used." This keys off the habitable ROOM count, not the
            //bedroom count, and where it applies it REPLACES the bedroom based rate - so a studio with a
            //separate bathroom is sized at 13 l/s, not the one bedroom figure of 19 l/s. Adding any second
            //habitable room (a separate living room, a study) takes the dwelling back onto Table 1.3.
            dwellingResult.OneHabitableRoomRuleApplied = dwellingResult.HabitableRoomCount == 1;

            //BedroomBasedRate_Lps and BedroomOrHabitableRate_Lps are deliberately two different numbers:
            //the former is always the plain Table 1.3 bedroom-count rate, the latter is whichever of
            //note 1 or Table 1.3 was actually selected. Assigning the selected rate to both would report
            //a one-habitable-room studio's bedroom-table rate as 13 l/s (the note 1 rate) instead of the
            //real Table 1.3 one-bedroom figure of 19 l/s, corrupting any comparison between the two.
            double bedroomBasedRate = partFData.GetWholeDwellingRates_Lps(dwellingResult.BedroomCount);
            double bedroomBaseRate = partFData.GetBedroomOrHabitableRate_Lps(dwellingResult.HabitableRoomCount, dwellingResult.BedroomCount);
            if (double.IsNaN(bedroomBaseRate) || double.IsNaN(bedroomBasedRate))
            {
                dwellingResult.Warnings.Add("No whole dwelling ventilation rate table (Approved Document F Table 1.3) is loaded. The bedroom based minimum rate has been ignored and only the floor area based rate applied.");
                bedroomBaseRate = 0;
                bedroomBasedRate = 0;
            }

            dwellingResult.BedroomBasedRate_Lps = bedroomBasedRate;
            dwellingResult.BedroomOrHabitableRate_Lps = bedroomBaseRate;

            if (dwellingResult.OneHabitableRoomRuleApplied)
            {
                dwellingResult.Remarks.Add(string.Format("The dwelling has exactly one habitable room ({0}), so Approved Document F Table 1.3 note 1 applies and the whole dwelling rate set by the rooms is {1:0.##} l/s rather than the one bedroom figure of Table 1.3. The paragraph 1.24a floor area rate still applies alongside it, so the continuous design rate may be higher.", string.Join(", ", dwellingResult.HabitableRoomNames), bedroomBaseRate));
            }

            if (dwellingResult.HabitableRoomCount == 0)
            {
                dwellingResult.Warnings.Add("No space was classified as a habitable room. Approved Document F paragraph 1.67 requires mechanical supply to each habitable room, so this dwelling has no supply provision, and Table 1.3 note 1 could not be assessed.");
            }

            if (dwellingResult.BedroomCount == 0 && dwellingResult.HabitableRoomCount != 1)
            {
                dwellingResult.Warnings.Add("No space was classified as a bedroom, so the dwelling has been sized at the one bedroom rate of Approved Document F Table 1.3, which gives no value below one bedroom. Check the room names if the dwelling does contain a bedroom.");
            }

            //Paragraph 1.24a (page 10): 0.3 l/s per m2 of internal floor area, all floors.
            List<DwellingSpace> dwellingSpaces_Area = dwellingSpaces.FindAll(x => x.PartFCategory.IncludeInFloorAreaCheck);

            dwellingResult.FloorAreaExcludedSpaceNames = [.. dwellingSpaces.FindAll(x => !x.PartFCategory.IncludeInFloorAreaCheck).ConvertAll(x => x.Space.Name)];
            if (dwellingResult.FloorAreaExcludedSpaceNames.Count != 0)
            {
                dwellingResult.Remarks.Add(string.Format("{0} space(s) were excluded from the Approved Document F paragraph 1.24a internal floor area because their category is not counted, normally voids and open-to-below areas: {1}.", dwellingResult.FloorAreaExcludedSpaceNames.Count, string.Join(", ", dwellingResult.FloorAreaExcludedSpaceNames)));
            }

            dwellingResult.InternalFloorArea_M2 = dwellingSpaces_Area.Sum(x => x.Area_M2);
            if (dwellingResult.InternalFloorArea_M2 <= 0)
            {
                dwellingResult.Warnings.Add("The internal floor area of the dwelling is zero, so the floor area based minimum rate of Approved Document F paragraph 1.24a could not be applied. Check that the spaces carry an Area parameter in m2.");
            }

            dwellingResult.AreaBasedRate_Lps = partFData.AreaRate_LpsPerM2 * dwellingResult.InternalFloorArea_M2;

            //Paragraph 1.24 (page 10): the rate must meet both conditions, so the greater applies.
            dwellingResult.WholeDwellingRate_Lps = System.Math.Max(bedroomBaseRate, dwellingResult.AreaBasedRate_Lps);
        }

        // ------------------------------------------------------------------
        // Terminals
        // ------------------------------------------------------------------

        private void BuildTerminals(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces, Dictionary<(Guid, PartFTerminalRole), (double? ContinuousDesignFlowRate_Lps, double? HighFlowRate_Lps)> dictionary_Measured)
        {
            double kitchenRate_Lps = partFData.GetKitchenExtractHighRate_Lps();

            //Paragraph 1.17a (page 8): extract ventilation is required from the kitchen. A dwelling with no
            //cooking space recognised at all is almost always a naming problem rather than a dwelling
            //without a kitchen, so it is reported before anything is sized.
            if (!dwellingSpaces.Exists(x => x.PartFCategory.IsCookingSpace))
            {
                dwellingResult.Warnings.Add("The dwelling contains no kitchen, open plan living kitchen or studio. Approved Document F paragraph 1.17a requires extract ventilation from the kitchen, so check that the cooking space has been named in a way the space use text map recognises.");
            }

            foreach (DwellingSpace dwellingSpace in dwellingSpaces)
            {
                PartFCategory partFCategory = dwellingSpace.PartFCategory;

                //Paragraph 1.67 (page 16): each habitable room has mechanical supply. The condition is
                //habitability, not the absence of a cooking function, so a studio and an open plan living
                //kitchen take a supply terminal exactly as a bedroom does.
                if (partFCategory.PartFType == PartFType.Habitable && partFCategory.IsTerminalSpace && partFCategory.ScaleSupplyWithVolume)
                {
                    PartFVentilationTerminalRequirement terminal_Supply = new(string.Format("{0} - supply", dwellingSpace.Space.Name), dwellingSpace.Space.Guid, PartFTerminalRole.Supply)
                    {
                        SpaceName = dwellingSpace.Space.Name,
                        ExtractMethod = PartFExtractMethod.NotRepresented,
                        OperatingMode = PartFOperatingMode.ContinuousDesign,
                        IsInBalancedFlow = true,
                        IsRequired = true,
                        SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.67 (page 16)",
                    };

                    ApplyMeasuredRates(terminal_Supply, dictionary_Measured);
                    dwellingSpace.Terminals.Add(terminal_Supply);
                }

                //Paragraph 1.17a (page 8): extract ventilation to the outside from the room containing the
                //cooking function. Held as a LOCAL kitchen extract terminal, distinct from general wet
                //room extract, because extract from a bathroom or ensuite may balance the dwelling airflow
                //without being local kitchen extract at all.
                if (partFCategory.IsCookingSpace)
                {
                    PartFVentilationTerminalRequirement terminal_Kitchen = BuildLocalKitchenExtractTerminal(dwellingSpace, kitchenRate_Lps, dwellingResult);
                    ApplyMeasuredRates(terminal_Kitchen, dictionary_Measured);
                    dwellingSpace.Terminals.Add(terminal_Kitchen);
                    continue;
                }

                //Paragraphs 1.17b to 1.17d and 1.70: continuous mechanical extract from each wet room, at
                //least the Table 1.2 minimum high rate.
                if (partFCategory.PartFVentilationType == Enums.PartFVentilationType.extract && partFCategory.IsTerminalSpace)
                {
                    PartFVentilationTerminalRequirement terminal_Extract = new(string.Format("{0} - general extract", dwellingSpace.Space.Name), dwellingSpace.Space.Guid, PartFTerminalRole.GeneralExtract)
                    {
                        SpaceName = dwellingSpace.Space.Name,

                        //A continuous terminal on the balanced system is what SAM PROPOSES for a wet room
                        //here, not something read from the model. Nothing in the analytical model says a
                        //terminal was installed, so the provision stays unrecorded and the assessment
                        //reports it as such rather than crediting the design with it.
                        ExtractMethod = PartFExtractMethod.MVHRContinuousTerminal,
                        ProposedExtractMethod = PartFExtractMethod.MVHRContinuousTerminal,
                        ProvidedExtractMethod = PartFExtractMethod.NotSpecified,
                        ProvisionStatus = PartFComplianceStatus.CannotBeDetermined,

                        OperatingMode = PartFOperatingMode.ContinuousDesign,
                        MinimumRequiredFlowRate_Lps = partFCategory.MinFlowRate_Lps,
                        IsInBalancedFlow = true,
                        IsLocalExtract = false,
                        IsRequired = true,
                        SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17 (page 8), Table 1.2 (page 10) and paragraph 1.70 (page 17)",
                    };

                    ApplyMeasuredRates(terminal_Extract, dictionary_Measured);
                    dwellingSpace.Terminals.Add(terminal_Extract);
                }
            }
        }

        /// <summary>
        /// Carries a previous run's measured rate onto the freshly built terminal that replaces it, keyed by
        /// the space it belongs to and its role - never by object identity, since <c>BuildTerminals</c>
        /// constructs a new <see cref="PartFVentilationTerminalRequirement"/> every run and has nothing else
        /// stable to match against.
        /// </summary>
        private static void ApplyMeasuredRates(PartFVentilationTerminalRequirement terminal, Dictionary<(Guid, PartFTerminalRole), (double? ContinuousDesignFlowRate_Lps, double? HighFlowRate_Lps)> dictionary_Measured)
        {
            if (terminal is null || dictionary_Measured is null || !dictionary_Measured.TryGetValue((terminal.SpaceGuid, terminal.TerminalRole), out (double? ContinuousDesignFlowRate_Lps, double? HighFlowRate_Lps) measured))
            {
                return;
            }

            terminal.MeasuredContinuousFlowRate_Lps = measured.ContinuousDesignFlowRate_Lps;
            terminal.MeasuredHighFlowRate_Lps = measured.HighFlowRate_Lps;
        }

        /// <summary>
        /// Builds the local kitchen extract terminal of one cooking space. The extract METHOD decides
        /// both which table the requirement comes from and whether the terminal counts towards the
        /// balanced continuous flow, and the method is an engineering input because the analytical model
        /// looks the same whether the hob has a mechanical ventilation with heat recovery extract terminal
        /// over it, a cooker hood ducted outside, or a recirculating hood.
        /// </summary>
        private PartFVentilationTerminalRequirement BuildLocalKitchenExtractTerminal(DwellingSpace dwellingSpace, double kitchenRate_Lps, PartFDwellingResult dwellingResult)
        {
            //What the Approved Document REQUIRES, what SAM PROPOSES and what the design actually PROVIDES
            //are three different statements and are held as three. A cooking space discovered from the
            //room semantics always carries a required terminal at the Table 1.2 kitchen high rate; SAM
            //proposes a continuous terminal on the balanced system to carry it, because that is what the
            //rest of the system implies; but until the model or a person records the arrangement, nothing
            //is known to be provided, and a proposal is never evidence of provision. This matters
            //particularly for a kitchen, where paragraph 1.17 requires extract TO THE OUTSIDE and a
            //recirculating cooker hood on its own is expressly not acceptable (Diagram 1.2 note 1).
            PartFExtractMethod partFExtractMethod_Provided = ReadLocalExtractMethod(dwellingSpace.Space);
            PartFExtractMethod partFExtractMethod_Proposed = PartFExtractMethod.MVHRContinuousTerminal;

            bool isDefault = partFExtractMethod_Provided == PartFExtractMethod.NotSpecified;

            //A rate has to be sized from something, so the proposal stands in where nothing is recorded.
            PartFExtractMethod partFExtractMethod = isDefault ? partFExtractMethod_Proposed : partFExtractMethod_Provided;

            PartFVentilationTerminalRequirement result = new(string.Format("{0} - local kitchen extract", dwellingSpace.Space.Name), dwellingSpace.Space.Guid, PartFTerminalRole.LocalKitchenExtract)
            {
                SpaceName = dwellingSpace.Space.Name,
                ExtractMethod = partFExtractMethod,
                ProposedExtractMethod = partFExtractMethod_Proposed,
                ProvidedExtractMethod = partFExtractMethod_Provided,
                ProvisionStatus = ProvisionStatus(partFExtractMethod_Provided),
                IsLocalExtract = true,
                IsRequired = true,
            };

            switch (partFExtractMethod)
            {
                case PartFExtractMethod.MVHRContinuousTerminal:
                    result.OperatingMode = PartFOperatingMode.ContinuousDesign;
                    result.IsInBalancedFlow = true;
                    result.MinimumRequiredFlowRate_Lps = kitchenRate_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17a (page 8), Table 1.2 (page 10) and paragraph 1.70 (page 17)";
                    result.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                    result.Diagnostic = isDefault
                        ? string.Format("REQUIRED but NOT CONFIRMED AS PROVIDED. Paragraph 1.17a requires extract ventilation to the outside from this cooking space, of at least the Table 1.2 kitchen high rate of {0:0.##} l/s. No local kitchen extract method is recorded for it, so SAM has PROPOSED a continuous mechanical ventilation with heat recovery extract terminal local to the cooking area - the arrangement the rest of the system implies - and sized the schedule from that proposal. A proposal is not a provision: nothing in the model states that this terminal exists, and paragraph 1.17 requires extract TO THE OUTSIDE, which a recirculating cooker hood on its own does not give (Diagram 1.2 note 1). Set the space's PartF Local Extract Method parameter to record the actual arrangement.", kitchenRate_Lps)
                        : string.Format("A continuous mechanical ventilation with heat recovery extract terminal local to the cooking area is recorded as provided, sized to at least the Table 1.2 kitchen high rate of {0:0.##} l/s. Paragraph 1.17 requires extract ventilation TO THE OUTSIDE, and whether the duct reaches outside air is a construction fact the model cannot show, so confirm it at commissioning.", kitchenRate_Lps);
                    break;

                case PartFExtractMethod.CookerHoodExtractingOutside:
                    //Table 1.1 (page 8) and Diagram 1.1 (page 9): 30 l/s where a cooker hood extracts to
                    //the outside. Intermittent, so it is not part of the balanced continuous flow.
                    result.OperatingMode = PartFOperatingMode.HighBoost;
                    result.IsInBalancedFlow = false;
                    result.MinimumRequiredFlowRate_Lps = partFData.IntermittentKitchenRateWithCookerHood_Lps;
                    result.HighFlowRate_Lps = partFData.IntermittentKitchenRateWithCookerHood_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17a (page 8), Table 1.1 (page 8), Diagram 1.1 (page 9) and paragraph 1.21 (page 8)";
                    result.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                    result.Diagnostic = string.Format("A cooker hood extracting to the outside, assessed against Table 1.1 at {0:0.##} l/s intermittent. It runs intermittently, so it is deliberately excluded from the balanced continuous supply and extract totals - counting it would credit the continuous design condition with air that only moves during cooking. Paragraph 1.21 also requires the hood to sit between 650mm and 750mm above the hob surface unless the manufacturer specifies otherwise, which the model cannot show.", partFData.IntermittentKitchenRateWithCookerHood_Lps);
                    break;

                case PartFExtractMethod.SeparateIntermittentExtract:
                    //Table 1.1 (page 8) and Diagram 1.2 (page 9): 60 l/s where there is no cooker hood
                    //extracting to the outside.
                    result.OperatingMode = PartFOperatingMode.HighBoost;
                    result.IsInBalancedFlow = false;
                    result.MinimumRequiredFlowRate_Lps = partFData.IntermittentKitchenRateWithoutCookerHood_Lps;
                    result.HighFlowRate_Lps = partFData.IntermittentKitchenRateWithoutCookerHood_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17a (page 8), Table 1.1 (page 8) and Diagram 1.2 (page 9)";
                    result.ComplianceStatus = PartFComplianceStatus.CannotBeDetermined;
                    result.Diagnostic = string.Format("A separate intermittent extract fan discharging outside, with no cooker hood extracting to the outside, assessed against Table 1.1 at {0:0.##} l/s. It runs intermittently, so it is deliberately excluded from the balanced continuous supply and extract totals.", partFData.IntermittentKitchenRateWithoutCookerHood_Lps);
                    break;

                case PartFExtractMethod.OtherExplicitExternalExtract:
                    result.OperatingMode = PartFOperatingMode.HighBoost;
                    result.IsInBalancedFlow = false;
                    result.MinimumRequiredFlowRate_Lps = kitchenRate_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17a (page 8), Table 1.1 and Table 1.2";
                    result.ComplianceStatus = PartFComplianceStatus.EngineeringReviewRequired;
                    result.Diagnostic = "Another arrangement extracting to the outside is recorded. Which of Table 1.1 and Table 1.2 applies, and therefore what rate is required, depends on whether the device runs intermittently or continuously, so this cannot be established automatically. Record the rate and the applicable table, and have the arrangement reviewed.";
                    break;

                case PartFExtractMethod.RecirculatingCookerHood:
                    result.OperatingMode = PartFOperatingMode.HighBoost;
                    result.IsInBalancedFlow = false;
                    result.MinimumRequiredFlowRate_Lps = partFData.IntermittentKitchenRateWithoutCookerHood_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), Diagram 1.2 note 1 (page 9) and Table 1.1 (page 8)";
                    result.ComplianceStatus = PartFComplianceStatus.Fail;
                    result.Diagnostic = string.Format("A recirculating cooker hood does not extract to the outside. Diagram 1.2 note 1 states that a recirculating cooker hood on its own does not provide a means of ventilation that complies with Part F, so it satisfies nothing here and contributes to no design flow. The cooking space still needs extract ventilation to the outside: {0:0.##} l/s intermittent under Table 1.1 where there is no cooker hood extracting outside, or a continuous terminal at the Table 1.2 kitchen high rate of {1:0.##} l/s.", partFData.IntermittentKitchenRateWithoutCookerHood_Lps, kitchenRate_Lps);
                    break;

                default:
                    result.OperatingMode = PartFOperatingMode.ContinuousDesign;
                    result.IsInBalancedFlow = false;
                    result.MinimumRequiredFlowRate_Lps = kitchenRate_Lps;
                    result.SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17a (page 8) and Table 1.2 (page 10)";
                    result.ComplianceStatus = PartFComplianceStatus.Fail;
                    result.Diagnostic = string.Format("No local kitchen or cooker extract is represented for this cooking space. Paragraph 1.17a requires extract ventilation to the outside from the room containing the cooking function, at least the Table 1.2 kitchen high rate of {0:0.##} l/s on a continuous system, or the Table 1.1 rate on an intermittent one. Extract from a bathroom, ensuite or other wet room may balance the dwelling airflow but does not satisfy this requirement.", kitchenRate_Lps);

                    dwellingResult.Warnings.Add(string.Format("ENGINEERING CHECK REQUIRED: '{0}' contains the cooking function but its PartF Local Extract Method is set to Not Represented, so no local kitchen or cooker extract is modelled for it. The missing provision is extract ventilation to the outside from the cooking space itself, at least {1:0.##} l/s under Approved Document F Table 1.2 on a continuous system. Extract from a bathroom, ensuite or other wet room may balance the dwelling airflow but does not demonstrate compliance with the local kitchen-extract requirement.", dwellingSpace.Space.Name, kitchenRate_Lps));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Reads the engineer's record of how a cooking space's local extract is <b>provided</b>, or
        /// <see cref="PartFExtractMethod.NotSpecified"/> where nothing has been recorded.
        /// <para>
        /// It deliberately never substitutes a default. Filling an unanswered question in with the most
        /// likely answer and then sizing from it is how a proposal turns into an assumed installation.
        /// The substitution the sizing needs happens once, visibly, in
        /// <see cref="BuildLocalKitchenExtractTerminal"/>, where the proposed and the provided methods are
        /// both kept.
        /// </para>
        /// </summary>
        private static PartFExtractMethod ReadLocalExtractMethod(Space space)
        {
            string text = space?.GetValue<string>(SpaceParameter.PartFLocalExtractMethod);

            return string.IsNullOrWhiteSpace(text)
                ? PartFExtractMethod.NotSpecified
                : Core.Query.Enum<PartFExtractMethod>(text);
        }

        /// <summary>
        /// Whether a recorded extract method establishes that a provision exists. It says nothing about
        /// whether that provision is adequate - the rate assessment on the terminal does that.
        /// </summary>
        private static PartFComplianceStatus ProvisionStatus(PartFExtractMethod partFExtractMethod)
        {
            switch (partFExtractMethod)
            {
                //Nobody has said what is installed. The terminal is still required and still sized, but
                //nothing is known to be provided, and absence of evidence is never compliance.
                case PartFExtractMethod.NotSpecified:
                    return PartFComplianceStatus.CannotBeDetermined;

                //A positive statement that there is no provision, and a recirculating hood is a positive
                //statement of a provision that Diagram 1.2 note 1 says does not comply. Both are decided.
                case PartFExtractMethod.NotRepresented:
                case PartFExtractMethod.RecirculatingCookerHood:
                    return PartFComplianceStatus.Fail;

                //The arrangement cannot be classified against a rate table without a decision.
                case PartFExtractMethod.OtherExplicitExternalExtract:
                    return PartFComplianceStatus.EngineeringReviewRequired;

                //A person or the model has recorded an arrangement that extracts to the outside.
                default:
                    return PartFComplianceStatus.UserConfirmed;
            }
        }

        // ------------------------------------------------------------------
        // Continuous design rates
        // ------------------------------------------------------------------

        private void AllocateContinuousRates(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces)
        {
            List<DwellingSpace> dwellingSpaces_Supply = dwellingSpaces.FindAll(x => x.Terminals.Exists(y => y.TerminalRole == PartFTerminalRole.Supply));

            List<PartFVentilationTerminalRequirement> terminals_Extract_Balanced = [.. dwellingSpaces
                .SelectMany(x => x.Terminals)
                .Where(x => x.IsExtract && x.IsInBalancedFlow)];

            //Paragraph 1.70 / Table 1.2: only terminals that are actually part of the continuous system
            //carry a Table 1.2 minimum high rate. A cooker hood is assessed against Table 1.1 and never
            //runs at this condition, so its 30 l/s has no place in this total either.
            //
            //This total is REPORTED, not applied to the whole dwelling rate. See below.
            dwellingResult.WetRoomMinimumTotal_Lps = terminals_Extract_Balanced.Sum(x => x.MinimumRequiredFlowRate_Lps ?? 0);

            //Paragraph 1.24 (page 10) and paragraph 1.69 (page 16): the whole dwelling continuous design
            //rate is the whole dwelling ventilation rate and nothing else.
            //
            //  ContinuousDesignRate = max(BedroomOrHabitableRate, 0.3 l/s/m2 x InternalFloorArea)
            //
            //Table 1.2 (page 10) defines two distinct things and they must not be conflated. Its
            //continuous column requires that the TOTAL of continuous extract reaches the whole dwelling
            //rate. Its per-room figures are minimum HIGH rates, each of which is assessed separately, on
            //the high rate, by AllocateHighRates and by the per-room Table 1.2 check. Nothing in the
            //Approved Document requires the continuous dwelling rate to reach the SUM of the per-room
            //high-rate minimums; note 1 says only that a room already continuously at or above its own
            //high-rate minimum needs no further increase. Summing them into the continuous rate would
            //systematically oversize normal continuous operation in any dwelling with several wet rooms.
            dwellingResult.ContinuousDesignSystemRate_Lps = dwellingResult.WholeDwellingRate_Lps;

            dwellingResult.SetbackSystemRate_Lps = dwellingResult.ContinuousDesignSystemRate_Lps * dwellingResult.SetbackFlowRateFactor;

            AllocateContinuousExtract(dwellingResult, dwellingSpaces, terminals_Extract_Balanced);

            AllocateContinuousSupply(dwellingResult, dwellingSpaces_Supply);

            dwellingResult.TotalExtract_Lps = terminals_Extract_Balanced.Sum(x => x.ContinuousDesignFlowRate_Lps ?? 0);
            dwellingResult.TotalSupply_Lps = dwellingSpaces
                .SelectMany(x => x.Terminals)
                .Where(x => x.TerminalRole == PartFTerminalRole.Supply)
                .Sum(x => x.ContinuousDesignFlowRate_Lps ?? 0);
        }

        private void AllocateContinuousExtract(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces, List<PartFVentilationTerminalRequirement> terminals_Extract_Balanced)
        {
            if (terminals_Extract_Balanced.Count == 0)
            {
                dwellingResult.Warnings.Add("The dwelling has no extract terminal that forms part of the balanced continuous system, so the total of all extract on its continuous rate is zero and cannot reach the whole dwelling ventilation rate required by Approved Document F Table 1.2 and paragraph 1.69.");
                return;
            }

            //The whole dwelling continuous rate is no longer raised to the sum of the Table 1.2 per-room
            //high-rate minimums, so that sum can now exceed it - typically in a small dwelling with
            //several wet rooms. When it does, the continuous condition cannot hold every room at its own
            //high-rate minimum, and the Approved Document does not ask it to: Table 1.2 requires the
            //TOTAL of continuous extract to reach the whole dwelling rate, and each room to reach its own
            //minimum at the HIGH rate, which AllocateHighRates then does by boosting.
            if (dwellingResult.WetRoomMinimumTotal_Lps > dwellingResult.ContinuousDesignSystemRate_Lps + PartFAirflowNetwork.Tolerance_Lps)
            {
                AllocateContinuousExtractBelowMinimumTotal(dwellingResult, dwellingSpaces, terminals_Extract_Balanced);
                return;
            }

            //Step 1: every extract terminal takes its Table 1.2 minimum.
            foreach (PartFVentilationTerminalRequirement terminal in terminals_Extract_Balanced)
            {
                terminal.ContinuousDesignFlowRate_Lps = terminal.MinimumRequiredFlowRate_Lps ?? 0;
            }

            double surplus = System.Math.Max(0, dwellingResult.ContinuousDesignSystemRate_Lps - dwellingResult.WetRoomMinimumTotal_Lps);
            if (surplus <= PartFAirflowNetwork.Tolerance_Lps)
            {
                return;
            }

            //Step 2: the surplus is shared out. Approved Document F fixes only the two constraints already
            //satisfied - each room at its Table 1.2 minimum, and the total at the whole dwelling rate - so
            //this split is an engineering strategy and is recorded as one on the result.
            List<PartFVentilationTerminalRequirement> terminals_Target = ExtractAllocationStrategy == PartFExtractAllocationStrategy.MinimumFirstCookingPriority
                ? [.. terminals_Extract_Balanced.Where(x => x.TerminalRole == PartFTerminalRole.LocalKitchenExtract)]
                : [];

            if (terminals_Target.Count != 0)
            {
                //Cooking priority: the surplus goes to the local kitchen extract, in proportion to each
                //terminal's own minimum where a dwelling has more than one cooking space. The cooking
                //function is the dwelling's largest single source of moisture and cooking pollutants, and
                //removing them closest to source is the stated aim of extract ventilation in requirement
                //F1(1)(a).
                double weight_Total = terminals_Target.Sum(x => x.MinimumRequiredFlowRate_Lps ?? 0);

                foreach (PartFVentilationTerminalRequirement terminal in terminals_Target)
                {
                    double share = weight_Total > 0
                        ? surplus * ((terminal.MinimumRequiredFlowRate_Lps ?? 0) / weight_Total)
                        : surplus / terminals_Target.Count;

                    terminal.ContinuousDesignFlowRate_Lps += share;
                }

                dwellingResult.Remarks.Add(string.Format("The {0:0.##} l/s surplus above the combined {1:0.##} l/s Table 1.2 high-rate minima was allocated to the local kitchen extract by the minimum-first, cooking-priority strategy. Approved Document F requires only that each room reaches its own Table 1.2 minimum and that the total reaches the whole dwelling rate; the split above those minima is an engineering strategy, not a regulatory value, and may be changed or overridden.", surplus, dwellingResult.WetRoomMinimumTotal_Lps));

                return;
            }

            //Volume weighting, either because that strategy was selected or because the dwelling has no
            //local kitchen extract terminal in the balanced flow for the surplus to go to.
            List<PartFVentilationTerminalRequirement> terminals_Scaled = [.. terminals_Extract_Balanced.Where(x => Scalable(dwellingSpaces, x))];
            if (terminals_Scaled.Count == 0)
            {
                terminals_Scaled = terminals_Extract_Balanced;
            }

            double volume_Total = terminals_Scaled.Sum(x => Volume_M3(dwellingSpaces, x));
            if (volume_Total <= 0)
            {
                dwellingResult.Warnings.Add("The balance of continuous extract above the Approved Document F Table 1.2 minimums could not be distributed because the extract spaces have no volume. Each extract terminal holds its Table 1.2 minimum only, so the total of all extract on its continuous rate is below the whole dwelling ventilation rate.");
                return;
            }

            foreach (PartFVentilationTerminalRequirement terminal in terminals_Scaled)
            {
                terminal.ContinuousDesignFlowRate_Lps += surplus * (Volume_M3(dwellingSpaces, terminal) / volume_Total);
            }

            dwellingResult.Remarks.Add(string.Format("The {0:0.##} l/s surplus above the combined {1:0.##} l/s Table 1.2 high-rate minima was allocated between the extract terminals in proportion to room volume. Approved Document F requires only that each room reaches its own Table 1.2 minimum and that the total reaches the whole dwelling rate; the split above those minima is an engineering strategy, not a regulatory value.", surplus, dwellingResult.WetRoomMinimumTotal_Lps));
        }

        /// <summary>
        /// Distributes the whole dwelling continuous rate where it is BELOW the total of the Table 1.2
        /// per-room minimum high rates, so no room can be held at its own minimum on the continuous
        /// condition.
        /// <para>
        /// This is a normal design outcome, not a failure. Table 1.2's per-room figures are minimum
        /// <b>high</b> rates: the continuous requirement is on the total, and each room reaches its own
        /// figure by boosting. Every terminal here is left with
        /// <see cref="PartFVentilationTerminalRequirement.HighRateIncreaseRequired"/> set by
        /// <see cref="AllocateHighRates"/>, and the dwelling total still reaches the whole dwelling rate.
        /// </para>
        /// </summary>
        private void AllocateContinuousExtractBelowMinimumTotal(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces, List<PartFVentilationTerminalRequirement> terminals_Extract_Balanced)
        {
            //The strategy still chooses the basis of the split, exactly as it does for the surplus above
            //the minimums. Cooking priority has no surplus left to prioritise here, so it falls back to
            //the Table 1.2 minimums themselves, which keeps each room's share in proportion to the rate
            //the Approved Document asks of it.
            bool byVolume = ExtractAllocationStrategy == PartFExtractAllocationStrategy.VolumeWeighted
                && terminals_Extract_Balanced.Sum(x => Volume_M3(dwellingSpaces, x)) > 0;

            double Weight(PartFVentilationTerminalRequirement terminal)
            {
                return byVolume ? Volume_M3(dwellingSpaces, terminal) : terminal.MinimumRequiredFlowRate_Lps ?? 0;
            }

            double weight_Total = terminals_Extract_Balanced.Sum(Weight);

            foreach (PartFVentilationTerminalRequirement terminal in terminals_Extract_Balanced)
            {
                terminal.ContinuousDesignFlowRate_Lps = weight_Total > 0
                    ? dwellingResult.ContinuousDesignSystemRate_Lps * (Weight(terminal) / weight_Total)
                    : dwellingResult.ContinuousDesignSystemRate_Lps / terminals_Extract_Balanced.Count;
            }

            dwellingResult.Remarks.Add(string.Format("The total of the Approved Document F Table 1.2 minimum high rates ({0:0.##} l/s) is above the whole dwelling ventilation rate ({1:0.##} l/s). Those figures are minimum HIGH rates, and Table 1.2's continuous requirement is that the TOTAL of continuous extract reaches the whole dwelling rate, so the continuous rate is not raised to their sum. The whole dwelling rate was distributed between the {2} continuous extract terminal(s) in proportion to {3}, and each room reaches its own Table 1.2 minimum by boosting to its high rate.", dwellingResult.WetRoomMinimumTotal_Lps, dwellingResult.ContinuousDesignSystemRate_Lps, terminals_Extract_Balanced.Count, byVolume ? "room volume" : "its Table 1.2 minimum high rate"));
        }

        private static bool Scalable(List<DwellingSpace> dwellingSpaces, PartFVentilationTerminalRequirement terminal)
        {
            //A local kitchen extract terminal in a habitable room carries no wet room category of its own,
            //so it has no ScaleExtractAboveMinimum flag; it is always scalable, being the terminal the
            //default strategy targets anyway.
            if (terminal.TerminalRole == PartFTerminalRole.LocalKitchenExtract)
            {
                return true;
            }

            return dwellingSpaces.Find(x => x.Space.Guid == terminal.SpaceGuid)?.PartFCategory?.ScaleExtractAboveMinimum ?? false;
        }

        private static double Volume_M3(List<DwellingSpace> dwellingSpaces, PartFVentilationTerminalRequirement terminal)
        {
            return dwellingSpaces.Find(x => x.Space.Guid == terminal.SpaceGuid)?.Volume_M3 ?? 0;
        }

        private static void AllocateContinuousSupply(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces_Supply)
        {
            if (dwellingSpaces_Supply.Count == 0)
            {
                dwellingResult.Warnings.Add("The dwelling has no space that takes a supply terminal, so no supply air was distributed and total supply does not balance total extract (Approved Document F paragraphs 1.67 and 1.69).");
                return;
            }

            //Paragraph 1.67 (page 16): "The total supply air flow should be distributed proportionately to
            //the volume of each habitable room."
            double volume_Total = dwellingSpaces_Supply.Sum(x => x.Volume_M3);
            if (volume_Total <= 0)
            {
                dwellingResult.Warnings.Add("The supply spaces have no volume, so the supply air could not be distributed in proportion to room volume (Approved Document F paragraph 1.67). Check that the spaces carry a Volume parameter in m3.");
                return;
            }

            foreach (DwellingSpace dwellingSpace in dwellingSpaces_Supply)
            {
                PartFVentilationTerminalRequirement terminal = dwellingSpace.Terminals.Find(x => x.TerminalRole == PartFTerminalRole.Supply);
                if (terminal is null)
                {
                    continue;
                }

                terminal.ContinuousDesignFlowRate_Lps = dwellingResult.ContinuousDesignSystemRate_Lps * (dwellingSpace.Volume_M3 / volume_Total);
                terminal.ComplianceStatus = PartFComplianceStatus.Pass;
                terminal.Diagnostic = string.Format("Mechanical supply to a habitable room under paragraph 1.67, distributed in proportion to habitable room volume: {0:0.##} m3 of {1:0.##} m3 across the dwelling.", dwellingSpace.Volume_M3, volume_Total);
            }
        }

        // ------------------------------------------------------------------
        // High rates
        // ------------------------------------------------------------------

        private static void AllocateHighRates(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces)
        {
            List<PartFVentilationTerminalRequirement> terminals_Extract_Balanced = [.. dwellingSpaces
                .SelectMany(x => x.Terminals)
                .Where(x => x.IsExtract && x.IsInBalancedFlow)];

            foreach (PartFVentilationTerminalRequirement terminal in terminals_Extract_Balanced)
            {
                double continuous = terminal.ContinuousDesignFlowRate_Lps ?? 0;
                double minimum = terminal.MinimumRequiredFlowRate_Lps ?? 0;

                //Table 1.2 note 1 (page 10): "If the continuous rate of ventilation provided in a room is
                //equal to or higher than the minimum high rate specified in the table, no extra
                //ventilation is needed." So the high rate is the greater of the two, and a room already
                //running at or above its high rate needs no boost at all.
                terminal.HighFlowRate_Lps = System.Math.Max(continuous, minimum);
                terminal.HighRateIncreaseRequired = continuous + PartFAirflowNetwork.Tolerance_Lps < minimum;
            }

            dwellingResult.TotalHighExtract_Lps = terminals_Extract_Balanced.Sum(x => x.HighFlowRate_Lps ?? 0);

            dwellingResult.TotalIntermittentExtract_Lps = dwellingSpaces
                .SelectMany(x => x.Terminals)
                .Where(x => x.IsExtract && !x.IsInBalancedFlow)
                .Sum(x => x.HighFlowRate_Lps ?? 0);

            //Balanced mechanical ventilation with heat recovery supplies as much as it extracts at every
            //condition, so the high supply total matches the high extract total and is distributed by the
            //same paragraph 1.67 rule as the continuous supply.
            List<DwellingSpace> dwellingSpaces_Supply = dwellingSpaces.FindAll(x => x.Terminals.Exists(y => y.TerminalRole == PartFTerminalRole.Supply));

            double volume_Total = dwellingSpaces_Supply.Sum(x => x.Volume_M3);
            if (dwellingSpaces_Supply.Count == 0 || volume_Total <= 0)
            {
                return;
            }

            foreach (DwellingSpace dwellingSpace in dwellingSpaces_Supply)
            {
                PartFVentilationTerminalRequirement terminal = dwellingSpace.Terminals.Find(x => x.TerminalRole == PartFTerminalRole.Supply);
                if (terminal is not null)
                {
                    terminal.HighFlowRate_Lps = dwellingResult.TotalHighExtract_Lps * (dwellingSpace.Volume_M3 / volume_Total);
                }
            }

            dwellingResult.TotalHighSupply_Lps = dwellingSpaces_Supply
                .ConvertAll(x => x.Terminals.Find(y => y.TerminalRole == PartFTerminalRole.Supply)?.HighFlowRate_Lps ?? 0)
                .Sum();
        }

        // ------------------------------------------------------------------
        // Setback rates
        // ------------------------------------------------------------------

        /// <summary>
        /// Writes the setback rate of every terminal that runs continuously. The setback rate is only ever
        /// a scaling of an already-established continuous design rate, so no regulatory minimum can be
        /// bypassed by the reduced-operation factor, and a terminal that does not run continuously - a
        /// cooker hood, a separate intermittent extract fan - has no setback rate at all rather than a
        /// misleading fraction of one.
        /// </summary>
        private static void ApplySetbackRates(List<DwellingSpace> dwellingSpaces, double setbackFlowRateFactor)
        {
            foreach (PartFVentilationTerminalRequirement terminal in dwellingSpaces.SelectMany(x => x.Terminals))
            {
                terminal.SetbackFlowRate_Lps = terminal.ContinuousDesignFlowRate_Lps is null
                    ? null
                    : terminal.ContinuousDesignFlowRate_Lps.Value * setbackFlowRateFactor;
            }
        }

        // ------------------------------------------------------------------
        // Writing the space data
        // ------------------------------------------------------------------

        private static void WriteSpaceData(AdjacencyCluster adjacencyCluster, List<DwellingSpace> dwellingSpaces)
        {
            foreach (DwellingSpace dwellingSpace in dwellingSpaces)
            {
                PartFSpaceData partFSpaceData = new(dwellingSpace.PartFCategory)
                {
                    Terminals = [.. dwellingSpace.Terminals],
                };

                //The scalar rate keeps its original meaning: the primary terminal's continuous design
                //rate, which is supply for a habitable room and extract for a wet room. A consumer written
                //before terminal-level sizing therefore reads exactly the number it always read.
                PartFVentilationTerminalRequirement terminal_Primary = partFSpaceData.PrimaryTerminal();

                partFSpaceData.ContinuousDesignFlowRate_Lps = terminal_Primary?.ContinuousDesignFlowRate_Lps ?? 0;
                partFSpaceData.SetbackFlowRate_Lps = terminal_Primary?.SetbackFlowRate_Lps ?? 0;

                //Carried over from the previous write of this run, so the purge assessment survives the
                //second pass that records it.
                partFSpaceData.Purge = dwellingSpace.Space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.Purge;

                dwellingSpace.Space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);
                adjacencyCluster.AddObject(dwellingSpace.Space);
            }
        }

        // ------------------------------------------------------------------
        // Purge ventilation
        // ------------------------------------------------------------------

        private static void AssessPurgeVentilation(
            AdjacencyCluster adjacencyCluster,
            List<DwellingSpace> dwellingSpaces,
            Dictionary<Guid, PartFPurgeVentilationData> dictionary_Purge,
            PartFComplianceResult complianceResult)
        {
            foreach (DwellingSpace dwellingSpace in dwellingSpaces)
            {
                bool isHabitable = dwellingSpace.PartFCategory.PartFType == PartFType.Habitable;
                if (!isHabitable)
                {
                    //Paragraph 1.26 requires purge ventilation in habitable rooms only, so a bathroom or a
                    //hall carries no purge record at all rather than one saying "not applicable" for every
                    //wet room in the dwelling.
                    continue;
                }

                dictionary_Purge.TryGetValue(dwellingSpace.Space.Guid, out PartFPurgeVentilationData partFPurgeVentilationData_Existing);

                PartFPurgeVentilationData partFPurgeVentilationData = PartFPurgeAssessor.Assess(dwellingSpace.Space, adjacencyCluster, true, partFPurgeVentilationData_Existing);
                if (partFPurgeVentilationData is null)
                {
                    continue;
                }

                complianceResult.PurgeVentilation.Add(partFPurgeVentilationData);

                if (dwellingSpace.Space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData) is PartFSpaceData partFSpaceData)
                {
                    partFSpaceData.Purge = partFPurgeVentilationData;
                }
            }
        }

        // ------------------------------------------------------------------
        // Transfer air
        // ------------------------------------------------------------------

        private static void CalculateTransferAir(
            AdjacencyCluster adjacencyCluster,
            PartFDwellingResult dwellingResult,
            List<DwellingSpace> dwellingSpaces,
            Dictionary<Guid, PartFDoorTransferData> dictionary_DoorTransfer,
            double setbackFlowRateFactor)
        {
            PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;

            //Every classified space of this dwelling is a node, including halls, landings and stores that
            //take no terminal: paragraph 1.25 is about air flowing THROUGH the dwelling, and internal
            //circulation is exactly what it flows through. Spaces excluded as communal or non-dwelling are
            //not here, so no route can leave the dwelling.
            PartFAirflowNetwork partFAirflowNetwork = new(adjacencyCluster, dwellingSpaces.ConvertAll(x => x.Space));

            complianceResult.DwellingSpaceCount = dwellingSpaces.Count;
            complianceResult.InternalConnectionCount = partFAirflowNetwork.Connections.Count;

            Dictionary<Guid, double> dictionary_Net_Continuous = [];
            Dictionary<Guid, double> dictionary_Net_High = [];

            foreach (DwellingSpace dwellingSpace in dwellingSpaces)
            {
                double net_Continuous = 0;
                double net_High = 0;

                foreach (PartFVentilationTerminalRequirement terminal in dwellingSpace.Terminals)
                {
                    if (!terminal.IsInBalancedFlow)
                    {
                        continue;
                    }

                    double sign = terminal.IsExtract ? -1 : 1;

                    net_Continuous += sign * (terminal.ContinuousDesignFlowRate_Lps ?? 0);
                    net_High += sign * (terminal.HighFlowRate_Lps ?? 0);
                }

                dictionary_Net_Continuous[dwellingSpace.Space.Guid] = net_Continuous;
                dictionary_Net_High[dwellingSpace.Space.Guid] = net_High;
            }

            Dictionary<(Guid, Guid), double> dictionary_Flow_Continuous = partFAirflowNetwork.Solve(
                x => dictionary_Net_Continuous.TryGetValue(x, out double result) ? result : 0,
                out List<Guid> guids_Unreachable);

            Dictionary<(Guid, Guid), double> dictionary_Flow_High = partFAirflowNetwork.Solve(
                x => dictionary_Net_High.TryGetValue(x, out double result) ? result : 0,
                out List<Guid> _);

            complianceResult.TransferPaths = PartFTransferPathBuilder.Build(
                partFAirflowNetwork,
                dictionary_Flow_Continuous,
                dictionary_Flow_High,
                setbackFlowRateFactor,
                dwellingResult.Name,
                dictionary_DoorTransfer);

            //Written back onto the door apertures so SAM_UI and Grasshopper can show and edit them, and so
            //the engineering inputs are read again by the next calculation.
            foreach (PartFDoorTransferData partFDoorTransferData in complianceResult.TransferPaths)
            {
                if (partFDoorTransferData.ApertureGuid == Guid.Empty)
                {
                    continue;
                }

                adjacencyCluster.SetPartFDoorTransferData(partFDoorTransferData.ApertureGuid, partFDoorTransferData);
            }

            //A dwelling with no internal separating element between any two of its spaces is a gap in the
            //model, not a dwelling whose rooms do not adjoin. Naming every space individually would bury
            //that one fact under a list, so it is reported once, and the per-space diagnostic below is
            //suppressed: every space is unreachable for the same reason and saying so room by room adds
            //nothing.
            if (complianceResult.HasNoInternalAdjacency)
            {
                dwellingResult.Remarks.Add(string.Format("No internal separating element was found between any two of this dwelling's {0} spaces, so the internal transfer air network is empty and Approved Document F paragraph 1.25 could not be assessed. Where the model carries no internal partitions this is expected; where it does, check that the partitions are related to the spaces on both sides.", complianceResult.DwellingSpaceCount));
            }
            else if (guids_Unreachable.Count != 0)
            {
                List<string> names = [.. guids_Unreachable.Distinct().Select(partFAirflowNetwork.Name).OrderBy(x => x, StringComparer.Ordinal)];

                dwellingResult.Warnings.Add(string.Format("{0} space(s) have a net airflow that cannot reach anywhere it could go, because no space of the opposite sign is connected to them within this dwelling: {1}. Approved Document F paragraph 1.25 requires internal doors to allow air to flow THROUGH the dwelling, so supply air must be able to reach an extract location and every extract location must be reachable from the supply spaces. Check the internal partitions between these rooms and the rest of the dwelling.", names.Count, string.Join(", ", names)));
            }
        }

        // ------------------------------------------------------------------
        // Compliance result
        // ------------------------------------------------------------------

        private void PopulateComplianceResult(PartFDwellingResult dwellingResult, List<DwellingSpace> dwellingSpaces)
        {
            PartFComplianceResult complianceResult = dwellingResult.ComplianceResult;

            complianceResult.Terminals = [.. dwellingSpaces
                .SelectMany(x => x.Terminals)
                .OrderBy(x => x.TerminalRole)
                .ThenBy(x => x.SpaceName, StringComparer.Ordinal)];

            complianceResult.ContinuousDesignSystemRate_Lps = dwellingResult.ContinuousDesignSystemRate_Lps;
            complianceResult.TotalContinuousSupply_Lps = dwellingResult.TotalSupply_Lps;
            complianceResult.TotalContinuousExtract_Lps = dwellingResult.TotalExtract_Lps;
            complianceResult.TotalHighSupply_Lps = dwellingResult.TotalHighSupply_Lps;
            complianceResult.TotalHighExtract_Lps = dwellingResult.TotalHighExtract_Lps;

            dwellingResult.TotalSetbackSupply_Lps = dwellingResult.TotalSupply_Lps * dwellingResult.SetbackFlowRateFactor;
            dwellingResult.TotalSetbackExtract_Lps = dwellingResult.TotalExtract_Lps * dwellingResult.SetbackFlowRateFactor;

            complianceResult.TotalSetbackSupply_Lps = dwellingResult.TotalSetbackSupply_Lps;
            complianceResult.TotalSetbackExtract_Lps = dwellingResult.TotalSetbackExtract_Lps;

            foreach (DwellingSpace dwellingSpace in dwellingSpaces.FindAll(x => x.Terminals.Count == 0))
            {
                dwellingResult.UnassignedSpaceNames.Add(dwellingSpace.Space.Name);
            }

            if (dwellingResult.UnassignedSpaceNames.Count != 0)
            {
                dwellingResult.Remarks.Add(string.Format("{0} classified space(s) received no ventilation terminal because their category is neither a supply nor an extract location: {1}. They still take part in the internal transfer air network, which is what circulation spaces are for.", dwellingResult.UnassignedSpaceNames.Count, string.Join(", ", dwellingResult.UnassignedSpaceNames)));
            }
        }

        /// <summary>
        /// Finds the commissioning evidence for one dwelling. Evidence stored on the zone always wins over
        /// evidence passed in for a single run, so a model carries its own record.
        /// </summary>
        private PartFCommissioningData ResolveCommissioningData(Zone zone, string dwellingName)
        {
            if (zone?.GetValue<PartFCommissioningData>(ZoneParameter.PartFCommissioningData) is PartFCommissioningData result)
            {
                return result;
            }

            if (CommissioningData is null)
            {
                return null;
            }

            return CommissioningData.TryGetValue(dwellingName ?? string.Empty, out PartFCommissioningData result_Supplied) ? result_Supplied : null;
        }

        private void Publish(PartFDwellingResult dwellingResult)
        {
            string prefix = string.IsNullOrWhiteSpace(dwellingResult.Name) ? string.Empty : dwellingResult.Name + ": ";

            //The result's own lists are the record of what happened in that dwelling; the calculator's
            //lists are the flattened view across the whole model, prefixed so a message can be traced back
            //to the flat it came from.
            List<string> warnings = [.. dwellingResult.Warnings];
            warnings.AddRange(dwellingResult.ComplianceResult?.Warnings ?? []);

            List<string> remarks = [.. dwellingResult.Remarks];
            remarks.AddRange(dwellingResult.ComplianceResult?.Notes ?? []);

            Warnings.AddRange(warnings.ConvertAll(x => prefix + x));
            Remarks.AddRange(remarks.ConvertAll(x => prefix + x));
            UnclassifiedSpaceNames.AddRange(dwellingResult.UnclassifiedSpaceNames);
            UnassignedSpaceNames.AddRange(dwellingResult.UnassignedSpaceNames);
        }
    }
}
