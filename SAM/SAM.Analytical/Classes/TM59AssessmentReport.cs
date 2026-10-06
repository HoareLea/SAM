// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// A <see cref="TM59AssessmentResult"/> arranged for a person to check: one row per criterion per space,
    /// carrying what was counted, what the criterion allowed, the margin between them, and the verdict.
    /// <para>
    /// <b>A view over the assessment, not a second assessment.</b> Nothing here counts an hour, derives a
    /// threshold or decides a pass. Every number is read off the <c>TMResult</c> objects the calculation
    /// produced, and every verdict is that result's own - which is why building a report cannot change what
    /// the assessment said.
    /// </para>
    /// <para>
    /// <b>Compliance and risk are kept apart on purpose.</b> The occupied spaces are judged Pass/Fail against
    /// the criterion that applies to them; <see cref="TM59AssessmentResult.CorridorResults"/> is reported as
    /// <see cref="TM59RiskStatus"/> instead, and exceeding its threshold never turns
    /// <see cref="OccupiedSpaceComplianceStatus"/> into a failure. That threshold is a reporting reference for
    /// design review, and stating it as a regulatory failure would claim an outcome TM59 does not reach.
    /// </para>
    /// <para>
    /// <b>A communal corridor is positively identified, not merely bucketed.</b>
    /// <c>TMOverheatingCalculator.Calculate_TM59</c> routes a space to the &gt;28 °C calculation when it
    /// carries no TM59 space application at all, or when its ventilation strategy is the closed vocabulary's
    /// <c>"UV"</c> - which also catches a bathroom, hall, ensuite or plant room, not only a real corridor. This
    /// report tells the two apart by the space's restored/resolved InternalCondition: only
    /// <see cref="TM59InternalConditionResolver.CommunalCorridorInternalConditionName"/> lands in
    /// <see cref="CorridorChecks"/> and counts towards <see cref="CorridorRiskStatus"/>; everything else the
    /// same calculation produced is real engineering information, kept in <see cref="SupplementaryChecks"/>
    /// instead.
    /// </para>
    /// <para>
    /// <b>What this report does not say.</b> It is an assessment of simulated temperatures. It cannot show
    /// that the Approved Document O modelling assumptions behind the simulation were applied, so it never
    /// states Part O compliance - only the TM59 outcome.
    /// </para>
    /// <para>
    /// Structured rather than pre-rendered so a CSV, JSON or PDF view can be added beside
    /// <see cref="TM59AssessmentReportFormatter"/> without any of them restating the assessment.
    /// </para>
    /// </summary>
    public class TM59AssessmentReport
    {
        /// <summary>The naturally ventilated day criterion - TM52 adaptive comfort, as TM59 applies it.</summary>
        public const string Check_Criterion1 = "Criterion 1";

        /// <summary>The bedroom night-time criterion.</summary>
        public const string Check_Criterion2 = "Criterion 2";

        /// <summary>The mechanically ventilated fixed-temperature criterion.</summary>
        public const string Check_HoursExceeding26 = ">26 C hours";

        /// <summary>The communal-corridor reference threshold.</summary>
        public const string Check_HoursExceeding28 = ">28 C hours";

        private const string reason_NoResult = "produced no TM59 result - either no criterion was settled for it, or its hourly series could not be read";

        /// <param name="tM59AssessmentResult">The assessment to report on. Its numbers and verdicts are read, never recomputed.</param>
        /// <param name="source">Where the results came from, for the report header - typically the TSD path.</param>
        public TM59AssessmentReport(TM59AssessmentResult tM59AssessmentResult, string source = null)
            : this(tM59AssessmentResult?.Spaces, tM59AssessmentResult?.MechanicalVentilationResults, tM59AssessmentResult?.NaturalVentilationResults, tM59AssessmentResult?.CorridorResults, tM59AssessmentResult?.VentilationStrategyRefusals, source)
        {
        }

        /// <summary>
        /// The same report from the four lists directly, which is the shape a caller holding Grasshopper
        /// outputs already has - and the shape that can be exercised without running a simulation.
        /// </summary>
        public TM59AssessmentReport(
            IEnumerable<Space> spaces,
            IEnumerable<TMResult> tMResults_MechanicalVentilation,
            IEnumerable<TMResult> tMResults_NaturalVentilation,
            IEnumerable<TMResult> tMResults_Corridor,
            IEnumerable<string> refusals = null,
            string source = null)
        {
            Source = source;

            List<TMResult> tMResults_Mechanical = Clean(tMResults_MechanicalVentilation);
            List<TMResult> tMResults_Natural = Clean(tMResults_NaturalVentilation);
            List<TMResult> tMResults_Corridors = Clean(tMResults_Corridor);

            TM52BuildingCategory = tMResults_Natural.Concat(tMResults_Mechanical).Concat(tMResults_Corridors).FirstOrDefault()?.TM52BuildingCategory ?? TM52BuildingCategory.Undefined;

            //The real annual series length this run used, read off whichever result carries it - never
            //assumed to be 8760. An extended result of any of the three kinds carries its own operative-
            //temperature series; a plain result only carries it on TM59CorridorResult, which is why the
            //corridor/supplementary bucket is checked too, not only natural and mechanical.
            AnnualHours = tMResults_Natural.Concat(tMResults_Mechanical).Concat(tMResults_Corridors)
                .Select(AnnualHoursFor)
                .FirstOrDefault(x => x.HasValue);

            //Keyed by the simulated space's Guid, as text - the same identity TMResult.Reference already
            //carries - never by Space.Name, which two dwellings can share. This is how a check row recovers
            //the restored/design InternalCondition the assessment actually classified it from.
            Dictionary<string, Space> spacesByReference = [];
            foreach (Space space in spaces ?? [])
            {
                if (space == null)
                {
                    continue;
                }

                spacesByReference[space.Guid.ToString()] = space;
            }

            NaturalVentilationChecks = [.. tMResults_Natural.SelectMany(x => NaturalVentilationChecksFor(x, spacesByReference))];
            MechanicalVentilationChecks = [.. tMResults_Mechanical.Select(x => MechanicalVentilationCheckFor(x, spacesByReference))];

            List<TM59AssessmentReportCheck> corridorBucketChecks = [.. tMResults_Corridors.Select(x => CorridorCheckFor(x, spacesByReference))];

            //Only a space whose restored/resolved InternalCondition is positively the TM59 communal-corridor
            //condition is a communal corridor. Everything else that landed in this bucket (a bathroom, a
            //hall, an ensuite, or a space this run simply could not resolve an InternalCondition for) is real
            //engineering information, but it is not a proven corridor, so it is kept apart rather than
            //silently counted towards CorridorRiskStatus below.
            CorridorChecks = [.. corridorBucketChecks.Where(x => x.InternalCondition == TM59InternalConditionResolver.CommunalCorridorInternalConditionName)];
            SupplementaryChecks = [.. corridorBucketChecks.Where(x => x.InternalCondition != TM59InternalConditionResolver.CommunalCorridorInternalConditionName)];

            //One entry per occupied space, grouped by identity in report order - natural ventilation first,
            //then mechanical, the order the text report lays them out in. A space is assessed under one
            //ventilation strategy, so its checks all come from one of the two lists.
            OccupiedSpaces = [.. NaturalVentilationChecks.Concat(MechanicalVentilationChecks)
                .GroupBy(x => x.Reference)
                .Select(x => new TM59AssessmentReportSpace(x.Key, x.First().SpaceName, [.. x], Combine([.. x])))];

            NaturalVentilationComplianceStatus = Combine(NaturalVentilationChecks);
            MechanicalVentilationComplianceStatus = Combine(MechanicalVentilationChecks);
            OccupiedSpaceComplianceStatus = Combine([.. NaturalVentilationChecks, .. MechanicalVentilationChecks]);

            //Any communal corridor over its threshold makes the whole building's corridor risk significant -
            //and that is as far as it travels. It is deliberately not folded into OccupiedSpaceComplianceStatus
            //above, and a supplementary (non-corridor) row over the same threshold never reaches this either.
            CorridorRiskStatus = CorridorChecks.Count == 0
                ? TM59RiskStatus.Undefined
                : CorridorChecks.Exists(x => x.RiskStatus == TM59RiskStatus.SignificantRisk) ? TM59RiskStatus.SignificantRisk : TM59RiskStatus.Acceptable;

            //A space the assessment covered but produced no result for is named, never quietly absent. The
            //refusals the assessment itself recorded say why for the spaces a scenario left unsettled; a space
            //whose hourly series were unreadable is silent at source, so it gets the combined sentence.
            HashSet<string> references = [.. tMResults_Natural.Concat(tMResults_Mechanical).Concat(tMResults_Corridors).Select(x => x.Reference).Where(x => !string.IsNullOrWhiteSpace(x))];

            UnassessedSpaces =
            [
                .. (spaces ?? []).Where(x => x != null && !references.Contains(x.Guid.ToString())).Select(x => string.Format("Space '{0}' {1}.", x.Name, reason_NoResult)),
                .. (refusals ?? []).Where(x => !string.IsNullOrWhiteSpace(x)),
            ];
        }

        /// <summary>Where the results came from, for the header. Null where the caller stated none.</summary>
        public string Source { get; }

        /// <summary>
        /// What thermal model produced these results - the whole building, or an isolated derived model of
        /// named dwellings. Null where the caller stated nothing, which reads as the ordinary whole-building
        /// run.
        /// <para>
        /// <b>Stated because it changes what the numbers mean.</b> An isolated run simulates interfaces to
        /// the dwellings it left out as adiabatic, so its results are not the whole-building results for the
        /// same flat. A reader who cannot see that from the report could compare two runs that were never
        /// comparable. The assessment itself is identical either way - the same criteria, over whichever
        /// spaces were simulated - which is why this is a statement of scope and not an assessment input.
        /// </para>
        /// </summary>
        public string ThermalModelScope { get; set; }

        /// <summary>The category the comfort limits were derived for, as the results themselves report it.</summary>
        public TM52BuildingCategory TM52BuildingCategory { get; }

        /// <summary>
        /// The number of hours in the full annual series this assessment actually used (typically 8760),
        /// read directly off a result that carries it. Null where nothing in the results states it - never
        /// defaulted to 8760, since a partial-year series would make that a fabricated number.
        /// </summary>
        public int? AnnualHours { get; }

        /// <summary>
        /// Criterion 1 for every naturally ventilated space, and Criterion 2 alongside it - satisfied,
        /// exceeded, or not applicable where the space is not a bedroom.
        /// </summary>
        public List<TM59AssessmentReportCheck> NaturalVentilationChecks { get; }

        /// <summary>The fixed-temperature criterion for every mechanically ventilated space.</summary>
        public List<TM59AssessmentReportCheck> MechanicalVentilationChecks { get; }

        /// <summary>
        /// The &gt;28 °C communal-corridor overheating-risk threshold, for every space whose restored/resolved
        /// InternalCondition is positively <see cref="TM59InternalConditionResolver.CommunalCorridorInternalConditionName"/>.
        /// Every row carries <c>ComplianceStatus.NotApplicable</c> and a real <c>RiskStatus</c>. See
        /// <see cref="SupplementaryChecks"/> for everything else the same &gt;28 °C calculation also produced.
        /// </summary>
        public List<TM59AssessmentReportCheck> CorridorChecks { get; }

        /// <summary>
        /// The same &gt;28 °C calculation, for every space the assessment put in that bucket WITHOUT a
        /// positively identified communal-corridor InternalCondition - a bathroom, hall, ensuite, or any
        /// other space with no assessed occupied-space use. Real engineering information, kept apart because
        /// it is not a proven corridor: it never contributes to <see cref="CorridorRiskStatus"/> and is never
        /// presented as a mandatory communal-corridor criterion.
        /// </summary>
        public List<TM59AssessmentReportCheck> SupplementaryChecks { get; }

        /// <summary>
        /// Spaces the assessment covered without producing a result, one sentence each, followed by the
        /// assessment's own refusals. Empty where every space produced a result.
        /// </summary>
        public List<string> UnassessedSpaces { get; }

        /// <summary>
        /// Every occupied space the report holds a natural- or mechanical-ventilation check for, with its
        /// overall outcome - the per-space verdict the text report's "Overall" column prints. Corridor and
        /// supplementary &gt;28 °C rows are not occupied-space verdicts and are not here.
        /// </summary>
        public List<TM59AssessmentReportSpace> OccupiedSpaces { get; }

        /// <summary>Every applicable natural-ventilation criterion, combined.</summary>
        public TM59ComplianceStatus NaturalVentilationComplianceStatus { get; }

        /// <summary>Every applicable mechanical-ventilation criterion, combined.</summary>
        public TM59ComplianceStatus MechanicalVentilationComplianceStatus { get; }

        /// <summary>
        /// The TM59 outcome for the occupied spaces: every applicable natural- and mechanical-ventilation
        /// criterion combined. <b>The corridor takes no part in this</b> - see
        /// <see cref="CorridorRiskStatus"/>.
        /// </summary>
        public TM59ComplianceStatus OccupiedSpaceComplianceStatus { get; }

        /// <summary>
        /// <c>SignificantRisk</c> where any corridor exceeds its threshold, and <c>Undefined</c> where no
        /// corridor was assessed.
        /// </summary>
        public TM59RiskStatus CorridorRiskStatus { get; }

        /// <summary>The multiline text report, for a Grasshopper panel.</summary>
        public override string ToString()
        {
            return TM59AssessmentReportFormatter.Text(this);
        }

        private static List<TMResult> Clean(IEnumerable<TMResult> tMResults)
        {
            return [.. (tMResults ?? []).Where(x => x != null)];
        }

        /// <summary>
        /// The real annual series length a single result carries, or null where this particular result
        /// does not state one. An extended result of any kind carries its own operative-temperature series
        /// (<see cref="TMExtendedResult.GetAnnualHours"/>); among plain results, only
        /// <see cref="TM59CorridorResult"/> does.
        /// </summary>
        private static int? AnnualHoursFor(TMResult tMResult)
        {
            int value = tMResult is TMExtendedResult extended
                ? extended.GetAnnualHours()
                : (tMResult as TM59CorridorResult)?.AnnualHours ?? int.MinValue;

            return value > 0 ? value : (int?)null;
        }

        /// <summary>
        /// Criterion 1, and Criterion 2 beside it on every row - a bedroom's verdict where it is one, and
        /// <c>NotApplicable</c> where it is not. A non-bedroom is stated rather than left off the report, so
        /// a reader can see the criterion was considered and not required.
        /// </summary>
        private static IEnumerable<TM59AssessmentReportCheck> NaturalVentilationChecksFor(TMResult tMResult, Dictionary<string, Space> spacesByReference)
        {
            string use = Use(tMResult);
            string reference = tMResult.Reference;
            string internalCondition = InternalConditionName(reference, spacesByReference);

            //The extended and plain results are sibling branches, not parent and child, so both are read
            //explicitly - and the bedroom variant is tested before its non-bedroom parent on each branch,
            //since a bedroom result also matches the parent type.
            //
            //Actual and Limit are both deliberately the SUMMER basis (GetSummerOccupiedHoursExceedingComfortRange /
            //MaxExceedableSummerHours), not the base type's annual GetOccupiedHoursExceedingComfortRange /
            //MaxExceedableHours - TAS's own "Max. Exceedable Hours" column is paired with "Occupied Summer
            //Hours", not the whole-year count, and the two differ by roughly the ratio of summer to annual
            //occupied hours (found comparing this report's Studio 1_0 row against the real Flat1 BasePassive
            //TAS report: annual gives 262, TAS's actual figure is 110). This is also the same authority
            //Criterion1 itself now decides Pass/Fail from, so this row can no longer disagree with its own
            //ComplianceStatus.
            int? actual_Criterion1 = tMResult is TM59NaturalVentilationExtendedResult extended
                ? Count(extended.GetSummerOccupiedHoursExceedingComfortRange())
                : Count((tMResult as TM59NaturalVentilationResult)?.HoursExceedingComfortRange);

            int? limit_Criterion1 = tMResult is TM59NaturalVentilationExtendedResult extended_ForLimit
                ? Count(extended_ForLimit.GetSummerMaxExceedableHours())
                : Count((tMResult as TM59NaturalVentilationResult)?.MaxExceedableSummerHours);

            //The basis Criterion 1 was actually assessed against - Occupied Summer Hours - read directly off
            //the result, the same fields Limit above already reads, never reconstructed from Limit.
            int? basisHours_Criterion1 = tMResult is TM59NaturalVentilationExtendedResult extended_ForBasis1
                ? Count(extended_ForBasis1.GetSummerOccupiedHours())
                : Count((tMResult as TM59NaturalVentilationResult)?.SummerOccupiedHours);

            yield return new TM59AssessmentReportCheck(tMResult.Name, use, Check_Criterion1, actual_Criterion1, limit_Criterion1, tMResult.Pass ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, reference: reference, internalCondition: internalCondition, basisHours: basisHours_Criterion1);

            if (tMResult is TM59NaturalVentilationBedroomExtendedResult bedroom_Extended)
            {
                yield return new TM59AssessmentReportCheck(tMResult.Name, use, Check_Criterion2, Count(bedroom_Extended.GetNightHoursNumberExceeding26()), Count(bedroom_Extended.GetAnnualMaxExceedableNightHours()), bedroom_Extended.Criterion2 ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, reference: reference, internalCondition: internalCondition, basisHours: Count(bedroom_Extended.GetAnnualNightOccupiedHours()));
            }
            else if (tMResult is TM59NaturalVentilationBedroomResult bedroom)
            {
                yield return new TM59AssessmentReportCheck(tMResult.Name, use, Check_Criterion2, Count(bedroom.NightHoursNumberExceeding26), Count(bedroom.MaxExceedableNightHours), bedroom.Criterion2 ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, reference: reference, internalCondition: internalCondition, basisHours: Count(bedroom.AnnualNightOccupiedHours));
            }
            else
            {
                yield return new TM59AssessmentReportCheck(tMResult.Name, use, Check_Criterion2, null, null, TM59ComplianceStatus.NotApplicable, reference: reference, internalCondition: internalCondition);
            }
        }

        private static TM59AssessmentReportCheck MechanicalVentilationCheckFor(TMResult tMResult, Dictionary<string, Space> spacesByReference)
        {
            int? actual = tMResult is TM59MechanicalVentilationExtendedResult extended
                ? Count(extended.GetHoursNumberExceeding26())
                : Count((tMResult as TM59MechanicalVentilationResult)?.HoursExceeding26);

            //Occupied Hours is the one basis this criterion has, and it is already on the base TMResult -
            //no branching between plain and extended is needed the way the natural-ventilation bases require.
            int? basisHours = Count(tMResult.OccupiedHours);

            return new TM59AssessmentReportCheck(tMResult.Name, Use(tMResult), Check_HoursExceeding26, actual, Count(tMResult.MaxExceedableHours), tMResult.Pass ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, reference: tMResult.Reference, internalCondition: InternalConditionName(tMResult.Reference, spacesByReference), basisHours: basisHours);
        }

        /// <summary>
        /// The &gt;28 °C row: <c>NotApplicable</c> as a compliance status - both because the threshold is not a
        /// mandatory occupied-space test, and because this method alone cannot tell a communal corridor apart
        /// from any other space with no assessed occupied-space use. The caller partitions the result by
        /// <see cref="TM59AssessmentReportCheck.InternalCondition"/> into <see cref="CorridorChecks"/> and
        /// <see cref="SupplementaryChecks"/> - this method only ever builds the row itself.
        /// </summary>
        private static TM59AssessmentReportCheck CorridorCheckFor(TMResult tMResult, Dictionary<string, Space> spacesByReference)
        {
            int? actual = tMResult is TM59CorridorExtendedResult extended
                ? Count(extended.GetHoursNumberExceeding28())
                : Count((tMResult as TM59CorridorResult)?.HoursExceeding28);

            int? basisHours = tMResult is TM59CorridorExtendedResult extended_ForBasis
                ? Count(extended_ForBasis.GetAnnualHours())
                : Count((tMResult as TM59CorridorResult)?.AnnualHours);

            return new TM59AssessmentReportCheck(tMResult.Name, Use(tMResult), Check_HoursExceeding28, actual, Count(tMResult.MaxExceedableHours), TM59ComplianceStatus.NotApplicable, tMResult.Pass ? TM59RiskStatus.Acceptable : TM59RiskStatus.SignificantRisk, reference: tMResult.Reference, internalCondition: InternalConditionName(tMResult.Reference, spacesByReference), basisHours: basisHours);
        }

        /// <summary>
        /// The restored/design InternalCondition's name for the space this reference identifies - null where
        /// the reference is absent, unrecognised, or the space carries no InternalCondition. Matched by Guid,
        /// never by Space.Name.
        /// </summary>
        private static string InternalConditionName(string reference, Dictionary<string, Space> spacesByReference)
        {
            if (string.IsNullOrWhiteSpace(reference) || spacesByReference == null)
            {
                return null;
            }

            return spacesByReference.TryGetValue(reference, out Space space) ? space?.InternalCondition?.Name : null;
        }

        /// <summary>
        /// A fixed order rather than the set's own, so two runs of the same model read identically.
        /// </summary>
        private static string Use(TMResult tMResult)
        {
            HashSet<TM59SpaceApplication> tM59SpaceApplications = (tMResult as TM59Result)?.TM59SpaceApplications ?? (tMResult as TM59ExtendedResult)?.TM59SpaceApplications;
            if (tM59SpaceApplications == null)
            {
                return null;
            }

            List<string> result = [];
            foreach (TM59SpaceApplication tM59SpaceApplication in new[] { TM59SpaceApplication.Sleeping, TM59SpaceApplication.Living, TM59SpaceApplication.Cooking })
            {
                if (tM59SpaceApplications.Contains(tM59SpaceApplication))
                {
                    result.Add(tM59SpaceApplication.ToString());
                }
            }

            return result.Count == 0 ? null : string.Join(", ", result);
        }

        /// <summary>
        /// -1 is the extended results' sentinel for "no readable series to count from" (see
        /// <c>TM59MechanicalVentilationExtendedResult.GetHoursNumberExceeding26</c>), and <c>int.MinValue</c>
        /// is the plain results' unset marker. Both are reported as absent rather than as a count, so a
        /// fabricated margin cannot be computed from them.
        /// </summary>
        private static int? Count(int? value)
        {
            return value.HasValue && value.Value >= 0 && value.Value != int.MinValue ? value : null;
        }

        /// <summary>
        /// Any failure fails the group; a group with nothing applicable to it is <c>NotApplicable</c> rather
        /// than a vacuous pass. The one combining rule: a section, the occupied spaces as a whole, and each
        /// space in <see cref="OccupiedSpaces"/>.
        /// </summary>
        private static TM59ComplianceStatus Combine(List<TM59AssessmentReportCheck> tM59AssessmentReportChecks)
        {
            if (tM59AssessmentReportChecks.Exists(x => x.ComplianceStatus == TM59ComplianceStatus.Fail))
            {
                return TM59ComplianceStatus.Fail;
            }

            return tM59AssessmentReportChecks.Exists(x => x.ComplianceStatus == TM59ComplianceStatus.Pass) ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.NotApplicable;
        }
    }
}
