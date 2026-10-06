// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <c>TM59AssessmentReport</c> - the verification view over a TM59 assessment.
    /// <para>
    /// <b>What these tests are guarding.</b> The report exists so an engineer can check a TM59 run without
    /// re-deriving it, which only works if the report cannot disagree with the assessment. So the assertions
    /// are mostly about what the report is <i>not</i> allowed to do: invent a verdict from arithmetic, turn a
    /// corridor's overheating risk into an occupied-space compliance failure, quietly upgrade a criterion
    /// that was never required of a space into a pass, or identify a communal corridor by anything other
    /// than its InternalCondition.
    /// </para>
    /// <para>
    /// The results are built directly rather than simulated. Every value below is a fixed number chosen to
    /// make one distinction visible, which is what lets a margin or a status be asserted exactly.
    /// </para>
    /// </summary>
    public class TM59AssessmentReportTests
    {
        private const string source = "SAM.Tests";

        // ---------------------------------------------------------------------------------------------
        // Natural ventilation - Criterion 1 and Criterion 2 stay separate in the data, one row in the text
        // ---------------------------------------------------------------------------------------------

        [Fact]
        public void NaturalVentilation_Passing_ReportsCriterion1AsPassWithTheRemainingAllowance()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.NaturalVentilationChecks, "Living 1_0", TM59AssessmentReport.Check_Criterion1);

            Assert.Equal(TM59ComplianceStatus.Pass, check.ComplianceStatus);
            Assert.Equal(37, check.Actual);
            Assert.Equal(110, check.Limit);
            Assert.Equal(73, check.Margin);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.NaturalVentilationComplianceStatus);
        }

        [Fact]
        public void NaturalVentilation_Failing_ReportsCriterion1AsFailWithANegativeMargin()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 150, maxExceedableSummerHours: 110, pass: false)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.NaturalVentilationChecks, "Living 1_0", TM59AssessmentReport.Check_Criterion1);

            Assert.Equal(TM59ComplianceStatus.Fail, check.ComplianceStatus);
            Assert.Equal(-40, check.Margin);
            Assert.Equal(TM59ComplianceStatus.Fail, tM59AssessmentReport.NaturalVentilationComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Fail, tM59AssessmentReport.OccupiedSpaceComplianceStatus);
        }

        /// <summary>
        /// Criterion 2 is a separate row in the data with its own numbers, and it is the bedroom night-time
        /// figures that appear on it - not Criterion 1's, which is the confusion a single merged Pass/Fail
        /// would create.
        /// </summary>
        [Theory]
        [InlineData(11, 32, true)]
        [InlineData(45, 32, false)]
        public void Bedroom_ReportsCriterion2SeparatelyFromCriterion1(int nightHoursExceeding26, int maxExceedableNightHours, bool expected_Pass)
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [Bedroom("Bedroom 2_3", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: nightHoursExceeding26, maxExceedableNightHours: maxExceedableNightHours, pass: true)]);

            TM59AssessmentReportCheck check_Criterion1 = Check(tM59AssessmentReport.NaturalVentilationChecks, "Bedroom 2_3", TM59AssessmentReport.Check_Criterion1);
            TM59AssessmentReportCheck check_Criterion2 = Check(tM59AssessmentReport.NaturalVentilationChecks, "Bedroom 2_3", TM59AssessmentReport.Check_Criterion2);

            Assert.Equal(37, check_Criterion1.Actual);
            Assert.Equal(110, check_Criterion1.Limit);

            Assert.Equal(nightHoursExceeding26, check_Criterion2.Actual);
            Assert.Equal(maxExceedableNightHours, check_Criterion2.Limit);
            Assert.Equal(maxExceedableNightHours - nightHoursExceeding26, check_Criterion2.Margin);
            Assert.Equal(expected_Pass ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, check_Criterion2.ComplianceStatus);

            //And a failed Criterion 2 fails the assessment even though Criterion 1 passed - Pass on a TM59
            //result is Criterion 1 alone, so a report reading only Pass would have called this a clean pass.
            Assert.Equal(expected_Pass ? TM59ComplianceStatus.Pass : TM59ComplianceStatus.Fail, tM59AssessmentReport.OccupiedSpaceComplianceStatus);
        }

        /// <summary>
        /// Criterion 2 is not required of a room that is not a bedroom. It is stated as N/A rather than left
        /// off the report or folded into a pass, so a reader can see it was considered - and that N/A is
        /// what the rendered text shows on that space's row, not a blank cell.
        /// </summary>
        [Fact]
        public void NonBedroom_ReportsCriterion2AsNotApplicableRatherThanPass()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.NaturalVentilationChecks, "Living 1_0", TM59AssessmentReport.Check_Criterion2);

            Assert.Equal(TM59ComplianceStatus.NotApplicable, check.ComplianceStatus);
            Assert.Null(check.Actual);
            Assert.Null(check.Limit);
            Assert.Null(check.Margin);

            //A group of nothing but non-applicable criteria is not a pass either.
            Assert.Equal(TM59ComplianceStatus.NotApplicable, Report(naturalVentilation: []).NaturalVentilationComplianceStatus);

            Assert.Contains("N/A", tM59AssessmentReport.ToString());
        }

        /// <summary>
        /// <b>Real numbers, from the Iteration 1 BasePassive validation pass.</b> Flat1's <c>Studio 1_0</c>
        /// against TAS's own "Domestic Overheating (CIBSE TM59)" report for the same run: Occupied Summer
        /// Hours 3672, Max. Exceedable Hours 110, Criterion 1 exceedance 37, Pass. The annual figures this
        /// fixture also carries (8760 occupied, 999 as a deliberately wrong-looking annual limit) exist only
        /// to prove the row does NOT read them - it did, once: the annual <c>MaxExceedableHours</c> (262 for
        /// this space) was shown as the Limit until this test caught it against the real TAS figures.
        /// </summary>
        [Fact]
        public void NaturalVentilation_Criterion1Limit_IsTheSummerBasisTasActuallyReports()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [NaturalVentilation("Studio 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.NaturalVentilationChecks, "Studio 1_0", TM59AssessmentReport.Check_Criterion1);

            Assert.Equal(37, check.Actual);
            Assert.Equal(110, check.Limit);
            Assert.Equal(73, check.Margin);
            Assert.NotEqual(262, check.Limit);
        }

        // ---------------------------------------------------------------------------------------------
        // Natural ventilation - one displayed row per space, and its basis hours
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Whether a natural-ventilation row states one criterion as Actual | Limit | Margin | Status - the
        /// convention the mechanical-ventilation section already uses, with Margin = Limit - Actual.
        /// </summary>
        private static bool HasCriterion(string line, int actual, int limit, int margin)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(
                line,
                string.Format(System.Globalization.CultureInfo.InvariantCulture, @"(^|\s){0}\s+{1}\s+{2}{3}\s+(PASS|FAIL)(\s|$)", actual, limit, margin > 0 ? "\\+" : string.Empty, margin));
        }

        /// <summary>
        /// The text presents Criterion 1 and Criterion 2 on the SAME line for a bedroom - not as two
        /// separate rows - and states the Overall verdict beside them.
        /// </summary>
        [Fact]
        public void NaturalVentilation_RendersOneRowPerSpace_WithBothCriteriaOnIt()
        {
            string text = Report(naturalVentilation: [Bedroom("Studio 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true)]).ToString();

            string naturalVentilationSection = Section(text, TM59AssessmentReportFormatter.Heading_NaturalVentilation, TM59AssessmentReportFormatter.Heading_AssessmentHours);

            //Exactly one line carries the space name in this section - the header row does not repeat it.
            Assert.Equal(1, naturalVentilationSection.Split('\n').Count(x => x.Contains("Studio 1_0")));

            string line = naturalVentilationSection.Split('\n').Single(x => x.Contains("Studio 1_0"));
            Assert.True(HasCriterion(line, 37, 110, 73), line);
            Assert.True(HasCriterion(line, 11, 32, 21), line);
            Assert.Contains("PASS", line);
        }

        /// <summary>A non-bedroom natural-ventilation row shows Criterion 2 as N/A on its single line, not FAIL or blank.</summary>
        [Fact]
        public void NaturalVentilation_NonBedroomRow_ShowsCriterion2AsNotApplicable()
        {
            string text = Report(naturalVentilation: [NaturalVentilation("Living_1", hoursExceedingComfortRange: 21, maxExceedableSummerHours: 105, pass: true)]).ToString();

            string line = Section(text, TM59AssessmentReportFormatter.Heading_NaturalVentilation, TM59AssessmentReportFormatter.Heading_AssessmentHours)
                .Split('\n').Single(x => x.Contains("Living_1"));

            Assert.True(HasCriterion(line, 21, 105, 84), line);
            Assert.Contains("N/A", line);
            Assert.Contains("PASS", line);
        }

        /// <summary>
        /// The exact Occupied Summer Hours and Annual Night Occupied Hours basis figures are exposed - read
        /// off the result, not reconstructed from a rounded Limit.
        /// </summary>
        [Fact]
        public void AssessmentHours_ExposesExactOccupiedSummerAndAnnualNightHours()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(naturalVentilation: [Bedroom("Studio 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true)]);

            TM59AssessmentReportCheck check_Criterion1 = Check(tM59AssessmentReport.NaturalVentilationChecks, "Studio 1_0", TM59AssessmentReport.Check_Criterion1);
            TM59AssessmentReportCheck check_Criterion2 = Check(tM59AssessmentReport.NaturalVentilationChecks, "Studio 1_0", TM59AssessmentReport.Check_Criterion2);

            Assert.Equal(3672, check_Criterion1.BasisHours);
            Assert.Equal(3285, check_Criterion2.BasisHours);

            string assessmentHoursSection = Section(tM59AssessmentReport.ToString(), TM59AssessmentReportFormatter.Heading_AssessmentHours, TM59AssessmentReportFormatter.Heading_MechanicalVentilation);
            Assert.Contains("3672", assessmentHoursSection);
            Assert.Contains("3285", assessmentHoursSection);
        }

        // ---------------------------------------------------------------------------------------------
        // Mechanical ventilation
        // ---------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(135, 142, true, TM59ComplianceStatus.Pass, 7)]
        [InlineData(200, 142, false, TM59ComplianceStatus.Fail, -58)]
        public void MechanicalVentilation_ReportsTheFixedTemperatureCheck(int hoursExceeding26, int maxExceedableHours, bool pass, TM59ComplianceStatus expected, int expected_Margin)
        {
            TM59AssessmentReport tM59AssessmentReport = Report(mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26, maxExceedableHours, pass)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.MechanicalVentilationChecks, "Kitchen_4", TM59AssessmentReport.Check_HoursExceeding26);

            Assert.Equal(hoursExceeding26, check.Actual);
            Assert.Equal(maxExceedableHours, check.Limit);
            Assert.Equal(expected_Margin, check.Margin);
            Assert.Equal(expected, check.ComplianceStatus);
            Assert.Equal(expected, tM59AssessmentReport.MechanicalVentilationComplianceStatus);
        }

        /// <summary>The exact Occupied Hours basis is exposed directly on the mechanical row - there is only one denominator, so no separate table is needed.</summary>
        [Fact]
        public void MechanicalVentilation_ExposesExactOccupiedHours()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.MechanicalVentilationChecks, "Kitchen_4", TM59AssessmentReport.Check_HoursExceeding26);

            Assert.Equal(4740, check.BasisHours);
            Assert.Contains("4740", tM59AssessmentReport.ToString());
        }

        /// <summary>The mechanical basis column is headed "Annual Occupied Hours", not the ambiguous "Occupied Hours".</summary>
        [Fact]
        public void MechanicalVentilation_BasisColumnIsHeadedAnnualOccupiedHours()
        {
            string text = Report(mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)]).ToString();

            Assert.Contains("Annual Occupied Hours", text);
        }

        // ---------------------------------------------------------------------------------------------
        // Assessment basis - descriptive, and never a hardcoded 8760
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// States the fixed TM59:2017 assessment periods, and the one genuinely data-derived figure - the
        /// real annual series length - read off whichever result carries it, here the corridor check.
        /// </summary>
        [Fact]
        public void AssessmentBasis_StatesTheFixedPeriodsAndTheRealAnnualSeriesLength()
        {
            Space space = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString(), annualHours: 8760)]);

            Assert.Equal(8760, tM59AssessmentReport.AnnualHours);

            string text = tM59AssessmentReport.ToString();

            Assert.Contains(TM59AssessmentReportFormatter.Heading_AssessmentBasis, text);
            Assert.Contains("CIBSE TM59:2017", text);
            Assert.Contains("1 Jan - 31 Dec (8760 hours)", text);
            Assert.Contains("1 May - 30 Sep (3672 clock hours)", text);
            Assert.Contains("22:00 - 07:00, full year (3285 clock hours)", text);
            Assert.Contains("annual occupied hours are space-specific", text);
            Assert.Contains("Communal corridor check:        Full year (8760 hours)", text);
        }

        /// <summary>
        /// The whole point of reading this from data: an assessment whose real series is NOT a standard
        /// 8760-hour year states the real number and the real (leap-year) night-window figure, not a
        /// hardcoded 8760/3285.
        /// </summary>
        [Fact]
        public void AssessmentBasis_DoesNotHardcode8760_StatesTheActualSeriesLength()
        {
            Space space = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString(), annualHours: 8784)]);

            Assert.Equal(8784, tM59AssessmentReport.AnnualHours);

            string text = tM59AssessmentReport.ToString();
            Assert.Contains("1 Jan - 31 Dec (8784 hours)", text);
            Assert.Contains("Communal corridor check:        Full year (8784 hours)", text);

            //The 366-day leap-year night-window figure, not the standard-year 3285 - and the summer study
            //period is unaffected (1 May - 30 Sep never includes February).
            Assert.Contains("22:00 - 07:00, full year (3294 clock hours)", text);
            Assert.Contains("1 May - 30 Sep (3672 clock hours)", text);
            Assert.DoesNotContain("8760", text);
        }

        /// <summary>
        /// Where nothing in the results states the real series length, the report says so honestly rather
        /// than assuming a standard year - no fabricated hour count and no fabricated clock-hour figure.
        /// </summary>
        [Fact]
        public void AssessmentBasis_OmitsTheHourCount_WhenNoResultStatesIt()
        {
            //Plain (non-extended) natural/mechanical results carry no annual-series field of their own -
            //only TM59CorridorResult and any extended result do - so with neither present here, nothing can
            //state the real figure.
            string text = Report(naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)]).ToString();

            string assessmentBasisSection = Section(text, TM59AssessmentReportFormatter.Heading_AssessmentBasis, TM59AssessmentReportFormatter.Heading_NaturalVentilation);

            Assert.Contains("Full year", assessmentBasisSection);
            Assert.Contains("1 May - 30 Sep", assessmentBasisSection);
            Assert.DoesNotContain("hours)", assessmentBasisSection);
            Assert.DoesNotContain("clock hours", assessmentBasisSection);
        }

        /// <summary>A non-standard annual series (neither 365 nor 366 days) is not forced into a misleading clock-hour total either.</summary>
        [Fact]
        public void AssessmentBasis_OmitsClockHours_WhenTheSeriesIsNotAStandardYear()
        {
            Space space = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            //A partial-year run - a real annual figure, but not one that supports the 365/366-day clock-hour derivation.
            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString(), annualHours: 4380)]);

            string text = tM59AssessmentReport.ToString();

            Assert.Contains("1 Jan - 31 Dec (4380 hours)", text);
            Assert.Contains("Communal corridor check:        Full year (4380 hours)", text);

            string assessmentBasisSection = Section(text, TM59AssessmentReportFormatter.Heading_AssessmentBasis, TM59AssessmentReportFormatter.Heading_NaturalVentilation);
            Assert.DoesNotContain("clock hours", assessmentBasisSection);
        }

        /// <summary>Descriptive only - the section states the same periods regardless of the results' own numbers, and never changes any check's Actual/Limit/ComplianceStatus.</summary>
        [Fact]
        public void AssessmentBasis_IsDescriptiveOnly_NeverRecalculatesCompliance()
        {
            TM59AssessmentReportCheck check = Check(
                Report(naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)]).NaturalVentilationChecks,
                "Living 1_0", TM59AssessmentReport.Check_Criterion1);

            Assert.Equal(37, check.Actual);
            Assert.Equal(110, check.Limit);
            Assert.Equal(TM59ComplianceStatus.Pass, check.ComplianceStatus);
        }

        // ---------------------------------------------------------------------------------------------
        // Internal Condition and TM59 Application - two distinct, auditable columns
        // ---------------------------------------------------------------------------------------------
        //
        // The Kitchen classification fix itself (TM59Manager.RoleMatchName) is regressed against the real
        // TM59Manager/TextMap pipeline in TM59SpaceApplicationClassificationTests, not here - this report's
        // own fixtures build TMResult objects directly with a fixed TM59SpaceApplication, deliberately
        // bypassing classification (see the class-level summary), so a fixture-level test could not
        // exercise the fix at all.

        [Fact]
        public void InternalCondition_AppearsInTheReport_SeparateFromTM59Application()
        {
            Space space = new("Bedroom 2_3") { InternalCondition = new InternalCondition("Double Bedroom") };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                naturalVentilation: [Bedroom("Bedroom 2_3", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true, reference: space.Guid.ToString())]);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.NaturalVentilationChecks, "Bedroom 2_3", TM59AssessmentReport.Check_Criterion1);

            Assert.Equal("Double Bedroom", check.InternalCondition);
            Assert.Equal("Sleeping", check.Use);

            string text = tM59AssessmentReport.ToString();
            Assert.Contains("Internal Condition", text);
            Assert.Contains("TM59 Application", text);
            Assert.Contains("Double Bedroom", text);

            string line = Section(text, TM59AssessmentReportFormatter.Heading_NaturalVentilation, TM59AssessmentReportFormatter.Heading_AssessmentHours)
                .Split('\n').Single(x => x.Contains("Bedroom 2_3"));
            Assert.Contains("Double Bedroom", line);
            Assert.Contains("Sleeping", line);
        }

        // ---------------------------------------------------------------------------------------------
        // Identity - grouping and lookup use Reference (the space Guid), never SpaceName
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Two dwellings can each contain a room named "Bedroom 2". They must not be merged into one
        /// displayed row - each keeps its own Criterion 1/2 numbers, distinguished by Reference.
        /// </summary>
        [Fact]
        public void DuplicateRoomNamesInDifferentDwellings_DoNotMergeGroupedByReference()
        {
            Space space_FlatA = new("Bedroom 2");
            Space space_FlatB = new("Bedroom 2");

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space_FlatA, space_FlatB],
                naturalVentilation:
                [
                    Bedroom("Bedroom 2", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true, reference: space_FlatA.Guid.ToString()),
                    Bedroom("Bedroom 2", hoursExceedingComfortRange: 60, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 20, maxExceedableNightHours: 32, pass: true, reference: space_FlatB.Guid.ToString()),
                ]);

            //Both rows exist in the data, each keyed by its own Reference.
            Assert.Equal(2, tM59AssessmentReport.NaturalVentilationChecks.Count(x => x.Check == TM59AssessmentReport.Check_Criterion1 && x.SpaceName == "Bedroom 2"));
            Assert.Equal(2, tM59AssessmentReport.NaturalVentilationChecks.Select(x => x.Reference).Distinct().Count());

            //And the text shows two distinct lines, each with its own numbers - not one merged row.
            string naturalVentilationSection = Section(tM59AssessmentReport.ToString(), TM59AssessmentReportFormatter.Heading_NaturalVentilation, TM59AssessmentReportFormatter.Heading_AssessmentHours);
            List<string> lines = [.. naturalVentilationSection.Split('\n').Where(x => x.Contains("Bedroom 2"))];

            Assert.Equal(2, lines.Count);
            Assert.Contains(lines, x => HasCriterion(x, 37, 110, 73) && HasCriterion(x, 11, 32, 21));
            Assert.Contains(lines, x => HasCriterion(x, 60, 110, 50) && HasCriterion(x, 20, 32, 12));
        }

        // ---------------------------------------------------------------------------------------------
        // Per-space overall status - one structured value, the one the text prints
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Every occupied space is one <c>OccupiedSpaces</c> entry with its own overall status: a bedroom that
        /// passes Criterion 1 and fails Criterion 2 fails; a non-bedroom whose Criterion 2 is N/A passes on
        /// Criterion 1; a mechanical space is its &gt;26 °C check. A corridor or supplementary row is not an
        /// occupied-space verdict and is not there.
        /// </summary>
        [Fact]
        public void OccupiedSpaces_CarryEachSpacesOverallStatus_AndNoCorridor()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(
                naturalVentilation:
                [
                    Bedroom("Bedroom 1", hoursExceedingComfortRange: 10, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 40, maxExceedableNightHours: 32, pass: true, reference: "bedroom"),
                    NaturalVentilation("Living 1", hoursExceedingComfortRange: 10, maxExceedableSummerHours: 110, pass: true, reference: "living"),
                ],
                mechanicalVentilation: [Mechanical("Kitchen 1", hoursExceeding26: 300, maxExceedableHours: 142, pass: false, reference: "kitchen")],
                corridor: [Corridor("Corridor 1", hoursExceeding28: 400, maxExceedableHours: 87, pass: false, reference: "corridor")]);

            Assert.Equal(["bedroom", "living", "kitchen"], tM59AssessmentReport.OccupiedSpaces.Select(x => x.Reference));
            Assert.Equal(TM59ComplianceStatus.Fail, Space(tM59AssessmentReport, "bedroom").ComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, Space(tM59AssessmentReport, "living").ComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Fail, Space(tM59AssessmentReport, "kitchen").ComplianceStatus);
            Assert.Equal(2, Space(tM59AssessmentReport, "bedroom").Checks.Count);
        }

        /// <summary>
        /// The text report's per-space Overall column and the structured status agree, space by space: the
        /// formatter prints the structured value rather than working out its own.
        /// </summary>
        [Theory]
        [InlineData(10, 110, 11, 32, true)]
        [InlineData(10, 110, 40, 32, true)]
        [InlineData(200, 110, 11, 32, false)]
        [InlineData(200, 110, 40, 32, false)]
        public void TheOverallColumn_IsTheStructuredSpaceStatus(int hoursExceedingComfortRange, int maxExceedableSummerHours, int nightHoursNumberExceeding26, int maxExceedableNightHours, bool pass)
        {
            TM59AssessmentReport tM59AssessmentReport = Report(
                naturalVentilation:
                [
                    Bedroom("Bedroom 1", hoursExceedingComfortRange, maxExceedableSummerHours, nightHoursNumberExceeding26, maxExceedableNightHours, pass, reference: "bedroom"),
                    NaturalVentilation("Living 1", hoursExceedingComfortRange, maxExceedableSummerHours, pass, reference: "living"),
                ]);

            string naturalVentilationSection = Section(tM59AssessmentReport.ToString(), TM59AssessmentReportFormatter.Heading_NaturalVentilation, TM59AssessmentReportFormatter.Heading_AssessmentHours);

            foreach (TM59AssessmentReportSpace space in tM59AssessmentReport.OccupiedSpaces)
            {
                string line = naturalVentilationSection.Split('\n').Single(x => x.StartsWith(space.SpaceName));

                string expected = space.ComplianceStatus switch
                {
                    TM59ComplianceStatus.Pass => "PASS",
                    TM59ComplianceStatus.Fail => "FAIL",
                    _ => "N/A",
                };

                Assert.EndsWith(expected, line.TrimEnd());
            }

            //And the whole-report verdict is those spaces combined.
            Assert.Equal(
                tM59AssessmentReport.OccupiedSpaces.Exists(x => x.ComplianceStatus == TM59ComplianceStatus.Fail) ? TM59ComplianceStatus.Fail : TM59ComplianceStatus.Pass,
                tM59AssessmentReport.OccupiedSpaceComplianceStatus);
        }

        /// <summary>Two dwellings' "Bedroom 2" are two spaces with two statuses, not one merged entry.</summary>
        [Fact]
        public void OccupiedSpaces_AreGroupedByReference_NotByName()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(
                mechanicalVentilation:
                [
                    Mechanical("Bedroom 2", hoursExceeding26: 100, maxExceedableHours: 262, pass: true, reference: "flat-a"),
                    Mechanical("Bedroom 2", hoursExceeding26: 300, maxExceedableHours: 262, pass: false, reference: "flat-b"),
                ]);

            Assert.Equal(2, tM59AssessmentReport.OccupiedSpaces.Count);
            Assert.Equal(TM59ComplianceStatus.Pass, Space(tM59AssessmentReport, "flat-a").ComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Fail, Space(tM59AssessmentReport, "flat-b").ComplianceStatus);
        }

        [Fact]
        public void OccupiedSpaces_IsEmpty_WhereNoOccupiedSpaceWasAssessed()
        {
            TM59AssessmentReport tM59AssessmentReport = Report(corridor: [Corridor("Corridor 1", hoursExceeding28: 10, maxExceedableHours: 87, pass: true, reference: "corridor")]);

            Assert.Empty(tM59AssessmentReport.OccupiedSpaces);
            Assert.Equal(TM59ComplianceStatus.NotApplicable, tM59AssessmentReport.OccupiedSpaceComplianceStatus);
        }

        private static TM59AssessmentReportSpace Space(TM59AssessmentReport tM59AssessmentReport, string reference)
        {
            return tM59AssessmentReport.OccupiedSpaces.Single(x => x.Reference == reference);
        }

        // ---------------------------------------------------------------------------------------------
        // Communal corridor - positively identified by InternalCondition, never by Space name
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Only a space whose restored InternalCondition is exactly the TM59 communal-corridor condition
        /// lands in <c>CorridorChecks</c> / <c>COMMUNAL CORRIDOR RISK</c>, and it does so however the Space
        /// happens to be named.
        /// </summary>
        [Fact]
        public void CommunalCorridor_IsPositivelyIdentifiedByInternalCondition_NotBySpaceName()
        {
            //Named nothing like a corridor - the InternalCondition alone must still identify it.
            Space space = new("Room_99") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                corridor: [Corridor("Room_99", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString())]);

            Assert.Single(tM59AssessmentReport.CorridorChecks);
            Assert.Empty(tM59AssessmentReport.SupplementaryChecks);
            Assert.Equal(TM59RiskStatus.SignificantRisk, tM59AssessmentReport.CorridorRiskStatus);

            string text = tM59AssessmentReport.ToString();
            Assert.Contains(TM59AssessmentReportFormatter.Heading_CommunalCorridorRisk, text);
            Assert.Contains(TM59InternalConditionResolver.CommunalCorridorInternalConditionName, text);
        }

        /// <summary>The converse: a Space literally named "Corridor" is NOT treated as a communal corridor without the matching InternalCondition.</summary>
        [Fact]
        public void ASpaceNamedCorridor_WithoutTheCommunalCorridorInternalCondition_IsNotClassifiedAsOne()
        {
            Space space = new("Corridor_5") { InternalCondition = new InternalCondition("TM59_Internal Corridor") };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                corridor: [Corridor("Corridor_5", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString())]);

            Assert.Empty(tM59AssessmentReport.CorridorChecks);
            Assert.Single(tM59AssessmentReport.SupplementaryChecks);
            Assert.Equal(TM59RiskStatus.Undefined, tM59AssessmentReport.CorridorRiskStatus);
        }

        // ---------------------------------------------------------------------------------------------
        // The corridor-style bucket also holds ancillary rooms - reported, never claimed as corridors
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>An ancillary room is never presented as a communal corridor.</b> A bathroom with no TM59 space
        /// application reaches the same &gt;28 °C calculation a communal corridor does, but its InternalCondition
        /// is not the communal-corridor condition, so it is reported under
        /// <see cref="TM59AssessmentReportFormatter.Heading_Supplementary"/> instead.
        /// </summary>
        [Fact]
        public void AnAncillaryRoom_IsNotPresentedAsACommunalCorridor()
        {
            Space space = new("Bathroom_2") { InternalCondition = new InternalCondition("TM59_Bathroom") };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)],
                corridor: [Corridor("Bathroom_2", hoursExceeding28: 2, maxExceedableHours: 262, pass: true, reference: space.Guid.ToString())]);

            Assert.Empty(tM59AssessmentReport.CorridorChecks);

            TM59AssessmentReportCheck check = Check(tM59AssessmentReport.SupplementaryChecks, "Bathroom_2", TM59AssessmentReport.Check_HoursExceeding28);

            //Not a compliance verdict, and not counted towards the occupied-space assessment either.
            Assert.Equal(TM59ComplianceStatus.NotApplicable, check.ComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.OccupiedSpaceComplianceStatus);

            string text = tM59AssessmentReport.ToString();
            Assert.Contains(TM59AssessmentReportFormatter.Heading_Supplementary, text);
            Assert.Contains("Bathroom_2", text);
        }

        /// <summary>
        /// The same refusal to identify holds when the room really is over the threshold, and a supplementary
        /// row's own risk never contributes to the headline communal-corridor risk.
        /// </summary>
        [Fact]
        public void SupplementaryChecks_DoNotAffectCorridorRiskStatus()
        {
            Space space = new("Ensuite_5") { InternalCondition = new InternalCondition("TM59_Bathroom") };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space],
                naturalVentilation: [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true)],
                corridor: [Corridor("Ensuite_5", hoursExceeding28: 400, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString())]);

            Assert.Equal(TM59RiskStatus.SignificantRisk, Check(tM59AssessmentReport.SupplementaryChecks, "Ensuite_5", TM59AssessmentReport.Check_HoursExceeding28).RiskStatus);

            //No true communal corridor was assessed, so the headline stays Undefined - a supplementary row's
            //own risk, however severe, does not promote it.
            Assert.Equal(TM59RiskStatus.Undefined, tM59AssessmentReport.CorridorRiskStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.OccupiedSpaceComplianceStatus);

            string text = tM59AssessmentReport.ToString();
            Assert.Contains(TM59AssessmentReportFormatter.Heading_Supplementary, text);
        }

        /// <summary>
        /// <b>The regulatory point of the whole separation.</b> A communal corridor over its threshold is
        /// highly visible in the summary and changes nothing about whether the occupied spaces passed.
        /// </summary>
        [Fact]
        public void ASignificantRiskCorridor_DoesNotFailTheOccupiedSpaceAssessment()
        {
            Space space_Corridor = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space_Corridor],
                naturalVentilation: [Bedroom("Bedroom 2_3", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true)],
                mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)],
                corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space_Corridor.Guid.ToString())]);

            Assert.Equal(TM59RiskStatus.SignificantRisk, tM59AssessmentReport.CorridorRiskStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.OccupiedSpaceComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.NaturalVentilationComplianceStatus);
            Assert.Equal(TM59ComplianceStatus.Pass, tM59AssessmentReport.MechanicalVentilationComplianceStatus);

            string text = tM59AssessmentReport.ToString();

            //Visible at the top and in the summary, and the two verdicts are stated as different things.
            Assert.Contains("TM59 OCCUPIED-SPACE ASSESSMENT: PASS", text);
            Assert.Contains("TM59 COMMUNAL-CORRIDOR RISK: SIGNIFICANT RISK", text);
            Assert.Contains("TM59 occupied-space assessment:   PASS", text);
            Assert.Contains("TM59 communal-corridor risk:      SIGNIFICANT RISK", text);
        }

        // ---------------------------------------------------------------------------------------------
        // Margins
        // ---------------------------------------------------------------------------------------------

        /// <summary>Margin is Limit - Actual, and it is signed in the text so the two directions are distinct.</summary>
        [Fact]
        public void Margin_IsLimitMinusActualAndIsRenderedSigned()
        {
            Space space_Corridor = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space_Corridor],
                mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)],
                corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space_Corridor.Guid.ToString())]);

            Assert.Equal(7, Check(tM59AssessmentReport.MechanicalVentilationChecks, "Kitchen_4", TM59AssessmentReport.Check_HoursExceeding26).Margin);
            Assert.Equal(-75, Check(tM59AssessmentReport.CorridorChecks, "Corridor_1", TM59AssessmentReport.Check_HoursExceeding28).Margin);

            string text = tM59AssessmentReport.ToString();

            Assert.Contains("+7", text);
            Assert.Contains("-75", text);
        }

        // ---------------------------------------------------------------------------------------------
        // Spaces that produced nothing
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// A space the assessment covered but produced no result for is named, with the assessment's own
        /// refusals beside it. Silently dropping it would let a short count read as a clean run.
        /// </summary>
        [Fact]
        public void ASpaceWithNoResult_IsNamedRatherThanOmitted()
        {
            Space space_Assessed = new("Living 1_0");
            Space space_Unassessed = new("Bathroom_2");

            TM59AssessmentReport tM59AssessmentReport = new(
                [space_Assessed, space_Unassessed],
                null,
                [NaturalVentilation("Living 1_0", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, pass: true, reference: space_Assessed.Guid.ToString())],
                null,
                ["No scenario covers space 'Hall_6'."]);

            Assert.Equal(2, tM59AssessmentReport.UnassessedSpaces.Count);
            Assert.Contains(tM59AssessmentReport.UnassessedSpaces, x => x.Contains("Bathroom_2"));
            Assert.Contains(tM59AssessmentReport.UnassessedSpaces, x => x.Contains("Hall_6"));

            //And the assessed one is not listed as missing.
            Assert.DoesNotContain(tM59AssessmentReport.UnassessedSpaces, x => x.Contains("Living 1_0"));

            string text = tM59AssessmentReport.ToString();
            Assert.Contains("Bathroom_2", text);
            Assert.Contains("Hall_6", text);
        }

        // ---------------------------------------------------------------------------------------------
        // The text
        // ---------------------------------------------------------------------------------------------

        [Fact]
        public void TheText_CarriesEverySectionAndNamesItsSource()
        {
            Space space_Corridor = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };
            Space space_Bathroom = new("Bathroom_2") { InternalCondition = new InternalCondition("TM59_Bathroom") };

            TM59AssessmentReport tM59AssessmentReport = Report(
                spaces: [space_Corridor, space_Bathroom],
                naturalVentilation: [Bedroom("Bedroom 2_3", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true)],
                mechanicalVentilation: [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)],
                corridor:
                [
                    Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space_Corridor.Guid.ToString()),
                    Corridor("Bathroom_2", hoursExceeding28: 2, maxExceedableHours: 262, pass: true, reference: space_Bathroom.Guid.ToString()),
                ]);

            string text = tM59AssessmentReport.ToString();

            foreach (string heading in new[]
            {
                TM59AssessmentReportFormatter.Heading,
                TM59AssessmentReportFormatter.Heading_AssessmentBasis,
                TM59AssessmentReportFormatter.Heading_NaturalVentilation,
                TM59AssessmentReportFormatter.Heading_AssessmentHours,
                TM59AssessmentReportFormatter.Heading_MechanicalVentilation,
                TM59AssessmentReportFormatter.Heading_CommunalCorridorRisk,
                TM59AssessmentReportFormatter.Heading_Supplementary,
                TM59AssessmentReportFormatter.Heading_Unassessed,
                TM59AssessmentReportFormatter.Heading_Summary,
                TM59AssessmentReportFormatter.Heading_Legend,
            })
            {
                Assert.Contains(heading, text);
            }

            Assert.Contains(source, text);
            Assert.Contains(TM52BuildingCategory.CategoryII.Description(), text);
            Assert.Contains(TM59AssessmentReportFormatter.PartOModellingAssumptionsNotice, text);

            //Every criterion appears under its own name rather than as one merged verdict.
            Assert.Contains(TM59AssessmentReport.Check_Criterion1, text);
            Assert.Contains(TM59AssessmentReport.Check_Criterion2, text);
            Assert.Contains(TM59AssessmentReport.Check_HoursExceeding26, text);
            Assert.Contains(TM59AssessmentReport.Check_HoursExceeding28, text);
        }

        [Fact]
        public void TheHeading_StatesTheTM59YearAndDoesNotClaimPartOCompliance()
        {
            string text = Report().ToString();

            Assert.Contains("CIBSE TM59:2017 OVERHEATING ASSESSMENT", text);
            Assert.DoesNotContain("PART O OVERHEATING VERIFICATION", text);
            Assert.DoesNotContain("Part O compliant", text);
        }

        /// <summary>
        /// <b>An isolated run says so, above the verdicts.</b> The scope decides how every line below it may
        /// be read: the same criteria over two rooms of a building is not the same statement as the same
        /// criteria over the building, and a reader handed the second when the first was run has been
        /// misled. It is printed before the category and the verdicts for that reason.
        /// </summary>
        [Fact]
        public void TheScopeLine_StatesAnIsolatedScope_AboveTheVerdicts()
        {
            TM59AssessmentReport tM59AssessmentReport = Report();

            tM59AssessmentReport.ThermalModelScope = "ISOLATED. Selected dwellings: Flat 1";

            string text = tM59AssessmentReport.ToString();

            Assert.Contains("Thermal model scope: ISOLATED. Selected dwellings: Flat 1", text);
            Assert.DoesNotContain("WHOLE BUILDING", text);

            //Above the verdicts, which is what makes it a qualifier on them rather than a footnote.
            Assert.True(text.IndexOf("Thermal model scope:") < text.IndexOf("TM59 OCCUPIED-SPACE ASSESSMENT:"));
        }

        /// <summary>
        /// <b>And a report that states no scope prints WHOLE BUILDING rather than nothing.</b> Silence would
        /// be read as a whole-building run by a reader who had no reason to think otherwise - which is the
        /// one reading an isolated run must never get by default. Every report states its scope, so the line
        /// being absent is never how a whole-building run is said.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TheScopeLine_FallsBackToWholeBuilding_WhenNoScopeIsStated(string thermalModelScope)
        {
            TM59AssessmentReport tM59AssessmentReport = Report();

            tM59AssessmentReport.ThermalModelScope = thermalModelScope;

            Assert.Contains("Thermal model scope: WHOLE BUILDING", tM59AssessmentReport.ToString());
        }

        /// <summary>
        /// The legend has to explain what a reader is looking at, and it has to include the caveat: passing
        /// temperatures are not a statement of Approved Document O compliance.
        /// </summary>
        [Fact]
        public void TheLegend_ExplainsEveryTermTheTableUses()
        {
            Space space = new("Corridor_1") { InternalCondition = new InternalCondition(TM59InternalConditionResolver.CommunalCorridorInternalConditionName) };
            string text = Report(spaces: [space], corridor: [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false, reference: space.Guid.ToString())]).ToString();

            string legend = text[text.IndexOf(TM59AssessmentReportFormatter.Heading_Legend)..];

            foreach (string term in new[] { "Actual", "Limit", "Margin", "Criterion 1", "Criterion 2", ">26 C hours", ">28 C hours", "TM59 Application", "ComplianceStatus", "RiskStatus", "N/A" })
            {
                Assert.Contains(term, legend);
            }

            //Margin's direction, and the two criteria's differing strictness, are both stated - a reader who
            //re-derives a verdict from a zero margin gets Criterion 2 wrong otherwise.
            Assert.Contains("Limit - Actual", legend);
            Assert.Contains("inclusive", legend);

            //Risk is explicitly not a compliance failure.
            Assert.Contains("NOT", legend);
            Assert.Contains("occupied-space compliance failure", legend);

            //And the report never claims Part O compliance.
            Assert.Contains(TM59AssessmentReportFormatter.Caveat, text);
            Assert.DoesNotContain("Part O compliant", text);
        }

        /// <summary>
        /// The TM59 Application legend lists the real <see cref="TM59SpaceApplication"/> values - Sleeping,
        /// Living, Cooking - each with a short explanation, and an example of a space carrying more than
        /// one. <c>Undefined</c> is excluded: it can never appear in a rendered TM59 Application cell (see
        /// <see cref="InternalCondition_AppearsInTheReport_SeparateFromTM59Application"/> and
        /// <see cref="NaturalVentilation_NonBedroomRow_ShowsCriterion2AsNotApplicable"/> for what a cell with
        /// no application at all shows instead: "-", never the word "Undefined"), so explaining it in the
        /// legend would describe a value a reader cannot actually see.
        /// </summary>
        [Fact]
        public void TheLegend_ListsTheRealTM59ApplicationValues()
        {
            string text = Report().ToString();
            string legend = text[text.IndexOf(TM59AssessmentReportFormatter.Heading_Legend)..];

            foreach (TM59SpaceApplication tM59SpaceApplication in System.Enum.GetValues(typeof(TM59SpaceApplication)))
            {
                if (tM59SpaceApplication == TM59SpaceApplication.Undefined)
                {
                    Assert.DoesNotContain("Undefined", legend);
                    continue;
                }

                Assert.Contains(tM59SpaceApplication.ToString(), legend);
            }

            Assert.Contains("Living, Cooking", legend);
        }

        // ---------------------------------------------------------------------------------------------
        // Simplify carries the TM59 space applications
        //
        // The extended results are classified from the internal condition at calculation time; the
        // simplified results are what the report renders. Dropping the classification at the seam is what
        // left every mechanical row's TM59 Application column "-".
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The classification survives simplification.</b> A mechanical result simplified for the report
        /// keeps the applications its extended form was classified with - the mechanical criterion does not
        /// vary by application, but the column still says what the space was assessed as.
        /// </summary>
        [Fact]
        public void SimplifiedMechanicalResult_KeepsItsTM59SpaceApplications()
        {
            TM59MechanicalVentilationExtendedResult extended = new(
                "Kitchen_4",
                source,
                null,
                TM52BuildingCategory.CategoryII,
                [0, 1, 2],
                new IndexedDoubles(),
                new IndexedDoubles(),
                new IndexedDoubles(),
                TM59SpaceApplication.Cooking);

            TM59MechanicalVentilationResult simplified = (TM59MechanicalVentilationResult)((TMExtendedResult)extended).Simplify();

            Assert.NotNull(simplified.TM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Cooking, simplified.TM59SpaceApplications);
            Assert.Single(simplified.TM59SpaceApplications);

            //And the report's mechanical row shows it, rather than "-".
            string text = new TM59AssessmentReport(null, [simplified], null, null, null, source).ToString();

            Assert.Contains("Cooking", Section(text, TM59AssessmentReportFormatter.Heading_MechanicalVentilation, TM59AssessmentReportFormatter.Heading_CommunalCorridorRisk));
        }

        /// <summary>The same for a non-bedroom natural-ventilation result - the branch with the same omission.</summary>
        [Fact]
        public void SimplifiedNaturalVentilationResult_KeepsItsTM59SpaceApplications()
        {
            TM59NaturalVentilationExtendedResult extended = new(
                "Living 1_0",
                source,
                null,
                TM52BuildingCategory.CategoryII,
                [0, 1, 2],
                new IndexedDoubles(),
                new IndexedDoubles(),
                new IndexedDoubles(),
                TM59SpaceApplication.Living,
                TM59SpaceApplication.Cooking);

            TM59NaturalVentilationResult simplified = (TM59NaturalVentilationResult)((TMExtendedResult)extended).Simplify();

            Assert.NotNull(simplified.TM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Living, simplified.TM59SpaceApplications);
            Assert.Contains(TM59SpaceApplication.Cooking, simplified.TM59SpaceApplications);
        }

        // ---------------------------------------------------------------------------------------------
        // The report changes nothing
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Building a report is a read. The results it was built from carry the same numbers and the same
        /// verdicts afterwards, and building it twice produces the same text.
        /// </summary>
        [Fact]
        public void BuildingAReport_LeavesTheAssessmentUntouched()
        {
            List<TMResult> tMResults_Natural = [Bedroom("Bedroom 2_3", hoursExceedingComfortRange: 37, maxExceedableSummerHours: 110, nightHoursNumberExceeding26: 11, maxExceedableNightHours: 32, pass: true)];
            List<TMResult> tMResults_Mechanical = [Mechanical("Kitchen_4", hoursExceeding26: 135, maxExceedableHours: 142, pass: true)];
            List<TMResult> tMResults_Corridor = [Corridor("Corridor_1", hoursExceeding28: 337, maxExceedableHours: 262, pass: false)];

            List<string> before = Transcript(tMResults_Natural, tMResults_Mechanical, tMResults_Corridor);

            string text = new TM59AssessmentReport(null, tMResults_Mechanical, tMResults_Natural, tMResults_Corridor, null, source).ToString();

            Assert.Equal(before, Transcript(tMResults_Natural, tMResults_Mechanical, tMResults_Corridor));
            Assert.Equal(text, new TM59AssessmentReport(null, tMResults_Mechanical, tMResults_Natural, tMResults_Corridor, null, source).ToString());
        }

        [Fact]
        public void AnAbsentAssessment_ProducesAReportRatherThanThrowing()
        {
            TM59AssessmentReport tM59AssessmentReport = new((TM59AssessmentResult)null);

            Assert.Equal(TM59ComplianceStatus.NotApplicable, tM59AssessmentReport.OccupiedSpaceComplianceStatus);
            Assert.Equal(TM59RiskStatus.Undefined, tM59AssessmentReport.CorridorRiskStatus);
            Assert.NotNull(tM59AssessmentReport.ToString());

            Assert.Null(TM59AssessmentReportFormatter.Text(null));
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture
        // ---------------------------------------------------------------------------------------------

        private static List<string> Transcript(params List<TMResult>[] tMResults)
        {
            return [.. tMResults.SelectMany(x => x).Select(x => string.Format("{0}|{1}|{2}|{3}|{4}", x.Name, x.OccupiedHours, x.MaxExceedableHours, x.Pass, x.ToJsonObject()?.ToJsonString()))];
        }

        private static TM59AssessmentReportCheck Check(List<TM59AssessmentReportCheck> tM59AssessmentReportChecks, string spaceName, string check)
        {
            return tM59AssessmentReportChecks.Single(x => x.SpaceName == spaceName && x.Check == check);
        }

        /// <summary>The text strictly between two headings, so an assertion about one section cannot accidentally match another.</summary>
        private static string Section(string text, string heading, string nextHeading)
        {
            int start = text.IndexOf(heading);
            int end = text.IndexOf(nextHeading, start);
            return end == -1 ? text[start..] : text[start..end];
        }

        private static TM59AssessmentReport Report(List<Space> spaces = null, List<TMResult> naturalVentilation = null, List<TMResult> mechanicalVentilation = null, List<TMResult> corridor = null)
        {
            return new TM59AssessmentReport(spaces, mechanicalVentilation, naturalVentilation, corridor, null, source);
        }

        /// <summary>
        /// <c>maxExceedableSummerHours</c> is deliberately the only limit a caller controls: the report reads
        /// Criterion 1's limit off the summer basis, never the annual <c>maxExceedableHours</c> field this
        /// fixture fixes at an unrelated 999 - so a regression back to reading the annual field would show up
        /// here as a wrong Limit, not pass by coincidence the way an equal-looking pair of test values would.
        /// </summary>
        private static TMResult NaturalVentilation(string name, int hoursExceedingComfortRange, int maxExceedableSummerHours, bool pass, string reference = null)
        {
            return new TM59NaturalVentilationResult(name, source, reference, TM52BuildingCategory.CategoryII, 8760, 999, 3672, maxExceedableSummerHours, hoursExceedingComfortRange, pass, TM59SpaceApplication.Living);
        }

        private static TMResult Bedroom(string name, int hoursExceedingComfortRange, int maxExceedableSummerHours, int nightHoursNumberExceeding26, int maxExceedableNightHours, bool pass, string reference = null)
        {
            return new TM59NaturalVentilationBedroomResult(name, source, reference, TM52BuildingCategory.CategoryII, 8760, 999, hoursExceedingComfortRange, 3285, 3672, maxExceedableSummerHours, maxExceedableNightHours, nightHoursNumberExceeding26, pass);
        }

        private static TMResult Mechanical(string name, int hoursExceeding26, int maxExceedableHours, bool pass, string reference = null)
        {
            return new TM59MechanicalVentilationResult(name, source, reference, TM52BuildingCategory.CategoryII, 4740, maxExceedableHours, hoursExceeding26, pass, TM59SpaceApplication.Cooking);
        }

        private static TMResult Corridor(string name, int hoursExceeding28, int maxExceedableHours, bool pass, string reference = null, int annualHours = 8760)
        {
            return new TM59CorridorResult(name, source, reference, TM52BuildingCategory.CategoryII, 8760, maxExceedableHours, hoursExceeding28, pass, annualHours);
        }
    }
}
