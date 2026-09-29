// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SAM.Analytical
{
    /// <summary>
    /// Renders a <see cref="TM59AssessmentReport"/> as the multiline text an engineer reads in a Grasshopper
    /// panel.
    /// <para>
    /// <b>Presentation only.</b> It reads the report's rows and statuses and lays them out. It counts
    /// nothing, decides nothing and cannot disagree with the assessment - which is what lets a CSV or JSON
    /// view be added beside it later without any of them restating TM59.
    /// </para>
    /// </summary>
    public static class TM59AssessmentReportFormatter
    {
        /// <summary>Section headings, exposed so a caller checking for a section does not restate the string.</summary>
        public const string Heading = "CIBSE TM59:2017 OVERHEATING ASSESSMENT";

        /// <summary>
        /// Descriptive only - the fixed assessment periods this TM59:2017 implementation uses, plus the
        /// one figure that is genuinely read off the data rather than assumed: the real annual series
        /// length (see <see cref="TM59AssessmentReport.AnnualHours"/>). Never recomputes compliance.
        /// </summary>
        public const string Heading_AssessmentBasis = "ASSESSMENT BASIS";

        public const string Heading_NaturalVentilation = "NATURAL VENTILATION";
        public const string Heading_AssessmentHours = "ASSESSMENT HOURS";
        public const string Heading_MechanicalVentilation = "MECHANICAL VENTILATION";

        /// <summary>Only a positively identified communal corridor - see <see cref="TM59AssessmentReport.CorridorChecks"/> - is presented here.</summary>
        public const string Heading_CommunalCorridorRisk = "COMMUNAL CORRIDOR RISK";

        /// <summary>
        /// The same &gt;28 °C calculation for every space that reached it WITHOUT a positively identified
        /// communal-corridor InternalCondition - see <see cref="TM59AssessmentReport.SupplementaryChecks"/>.
        /// Real engineering information, never a mandatory communal-corridor criterion - the heading says so
        /// explicitly rather than relying on a reader to infer it from the section name alone.
        /// </summary>
        public const string Heading_Supplementary = "SUPPLEMENTARY >28 C CHECKS - INFORMATION ONLY";

        public const string Heading_Unassessed = "SPACES NOT ASSESSED";
        public const string Heading_Summary = "SUMMARY";
        public const string Heading_Legend = "LEGEND";

        /// <summary>
        /// Printed near the header, beside the TM52 building category. This component assesses simulated
        /// temperatures against TM59:2017 - it cannot itself prove that every Approved Document O modelling
        /// assumption was applied to the simulation the results came from, so it states plainly, up front,
        /// that it never verifies that.
        /// </summary>
        public const string PartOModellingAssumptionsNotice = "Part O modelling assumptions: NOT VERIFIED BY THIS RESULT REPORT";

        /// <summary>
        /// The one sentence the whole separation exists to allow. Passing temperatures do not establish that
        /// the Approved Document O modelling assumptions behind the simulation were applied, so no Part O
        /// compliance is claimed anywhere in this report.
        /// </summary>
        public const string Caveat =
            "This report assesses simulated temperatures against CIBSE TM59:2017. It does not by itself\r\n" +
            "verify that every Approved Document O modelling assumption was applied to the simulation.";

        public static string Text(TM59AssessmentReport tM59AssessmentReport)
        {
            if (tM59AssessmentReport == null)
            {
                return null;
            }

            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine(Heading);
            stringBuilder.AppendLine(new string('=', Heading.Length));

            if (!string.IsNullOrWhiteSpace(tM59AssessmentReport.Source))
            {
                stringBuilder.AppendLine(string.Format("Source: {0}", tM59AssessmentReport.Source));
            }

            //Before the category and the verdicts: the scope decides how every line below it may be read.
            stringBuilder.AppendLine(string.Format(
                "Thermal model scope: {0}",
                string.IsNullOrWhiteSpace(tM59AssessmentReport.ThermalModelScope) ? "WHOLE BUILDING" : tM59AssessmentReport.ThermalModelScope));

            stringBuilder.AppendLine(string.Format("TM52 building category: {0}", tM59AssessmentReport.TM52BuildingCategory.Description()));
            stringBuilder.AppendLine(PartOModellingAssumptionsNotice);
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(string.Format("TM59 OCCUPIED-SPACE ASSESSMENT: {0}", Display(tM59AssessmentReport.OccupiedSpaceComplianceStatus)));
            stringBuilder.AppendLine(string.Format("TM59 COMMUNAL-CORRIDOR RISK: {0}", Display(tM59AssessmentReport.CorridorRiskStatus)));

            AppendAssessmentBasisSection(stringBuilder, tM59AssessmentReport.AnnualHours);
            AppendNaturalVentilationSection(stringBuilder, tM59AssessmentReport);
            AppendAssessmentHoursSection(stringBuilder, tM59AssessmentReport);
            AppendMechanicalVentilationSection(stringBuilder, tM59AssessmentReport.MechanicalVentilationChecks);
            AppendRiskSection(stringBuilder, Heading_CommunalCorridorRisk, tM59AssessmentReport.CorridorChecks);
            AppendRiskSection(stringBuilder, Heading_Supplementary, tM59AssessmentReport.SupplementaryChecks);

            AppendHeading(stringBuilder, Heading_Unassessed);
            if (tM59AssessmentReport.UnassessedSpaces.Count == 0)
            {
                stringBuilder.AppendLine("Every space the assessment covered produced a result.");
            }
            else
            {
                foreach (string unassessedSpace in tM59AssessmentReport.UnassessedSpaces)
                {
                    stringBuilder.AppendLine(unassessedSpace);
                }
            }

            AppendHeading(stringBuilder, Heading_Summary);
            stringBuilder.AppendLine(string.Format("Natural ventilation:             {0}", Display(tM59AssessmentReport.NaturalVentilationComplianceStatus)));
            stringBuilder.AppendLine(string.Format("Mechanical ventilation:           {0}", Display(tM59AssessmentReport.MechanicalVentilationComplianceStatus)));
            stringBuilder.AppendLine(string.Format("TM59 occupied-space assessment:   {0}", Display(tM59AssessmentReport.OccupiedSpaceComplianceStatus)));
            stringBuilder.AppendLine(string.Format("TM59 communal-corridor risk:      {0}", Display(tM59AssessmentReport.CorridorRiskStatus)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Caveat);

            AppendHeading(stringBuilder, Heading_Legend);
            stringBuilder.Append(Legend());

            return stringBuilder.ToString();
        }

        /// <summary>
        /// One row per space - Criterion 1 and Criterion 2 side by side - for each
        /// <see cref="TM59AssessmentReport.OccupiedSpaces"/> entry that carries natural-ventilation checks. Those
        /// are grouped by <see cref="TM59AssessmentReportCheck.Reference"/> (the simulated space's Guid), never by
        /// <see cref="TM59AssessmentReportCheck.SpaceName"/>: two dwellings can share a room name, not a Guid. The
        /// Overall column is that entry's own <see cref="TM59AssessmentReportSpace.ComplianceStatus"/>, not
        /// worked out here.
        /// </summary>
        private static void AppendNaturalVentilationSection(StringBuilder stringBuilder, TM59AssessmentReport tM59AssessmentReport)
        {
            AppendHeading(stringBuilder, Heading_NaturalVentilation);

            List<TM59AssessmentReportSpace> spaces = NaturalVentilationSpaces(tM59AssessmentReport);

            if (spaces.Count == 0)
            {
                stringBuilder.AppendLine("No space was assessed against this criterion.");
                return;
            }

            List<string[]> rows =
            [
                ["Space", "Internal Condition", "TM59 Application", "C1 Actual", "C1 Limit", "C1 Margin", "C1 Status", "C2 Actual", "C2 Limit", "C2 Margin", "C2 Status", "Overall"],
            ];

            foreach (TM59AssessmentReportSpace space in spaces)
            {
                TM59AssessmentReportCheck check_Criterion1 = space.Checks.FirstOrDefault(x => x.Check == TM59AssessmentReport.Check_Criterion1);
                TM59AssessmentReportCheck check_Criterion2 = space.Checks.FirstOrDefault(x => x.Check == TM59AssessmentReport.Check_Criterion2);
                TM59AssessmentReportCheck any = check_Criterion1 ?? check_Criterion2 ?? space.Checks.First();

                rows.Add(
                [
                    any.SpaceName ?? "-",
                    any.InternalCondition ?? "-",
                    any.Use ?? "-",
                    .. CriterionCells(check_Criterion1),
                    .. CriterionCells(check_Criterion2),
                    Display(space.ComplianceStatus),
                ]);
            }

            AppendTable(stringBuilder, rows, rightAligned: [false, false, false, true, true, true, false, true, true, true, false, false]);
        }

        /// <summary>
        /// Descriptive only - the fixed assessment periods TM59:2017 defines for each criterion, plus the
        /// one genuinely data-derived figure: the real annual series length this run used
        /// (<see cref="TM59AssessmentReport.AnnualHours"/>). The two "clock hours" figures beside the
        /// natural-ventilation study period and the bedroom night-time window are calendar constants of a
        /// standard (365- or 366-day) year - 1 May-30 September is always 153 days regardless of a leap
        /// year, since February never falls in that range - stated only when the real annual series
        /// confirms the run actually covers a standard year; never printed against a partial or non-standard
        /// series, where they would be misleading. Never recomputes anything a check already states, and
        /// never alters compliance.
        /// </summary>
        private static void AppendAssessmentBasisSection(StringBuilder stringBuilder, int? annualHours)
        {
            AppendHeading(stringBuilder, Heading_AssessmentBasis);

            int? standardYearDays = StandardYearDays(annualHours);

            string simulationPeriod = annualHours.HasValue
                ? string.Format(CultureInfo.InvariantCulture, "1 Jan - 31 Dec ({0} hours)", annualHours.Value)
                : "1 Jan - 31 Dec";

            //153 days (May 31 + Jun 30 + Jul 31 + Aug 31 + Sep 30) - fixed regardless of leap year, since
            //the range never includes February.
            string naturalVentilationStudy = standardYearDays.HasValue
                ? string.Format(CultureInfo.InvariantCulture, "1 May - 30 Sep ({0} clock hours)", 153 * 24)
                : "1 May - 30 Sep";

            string bedroomNightWindow = standardYearDays.HasValue
                ? string.Format(CultureInfo.InvariantCulture, "22:00 - 07:00, full year ({0} clock hours)", 9 * standardYearDays.Value)
                : "22:00 - 07:00, full year";

            string communalCorridorCheck = annualHours.HasValue
                ? string.Format(CultureInfo.InvariantCulture, "Full year ({0} hours)", annualHours.Value)
                : "Full year";

            stringBuilder.AppendLine(string.Format("Method:                         {0}", "CIBSE TM59:2017"));
            stringBuilder.AppendLine(string.Format("Simulation period:              {0}", simulationPeriod));
            stringBuilder.AppendLine(string.Format("Natural ventilation study:      {0}", naturalVentilationStudy));
            stringBuilder.AppendLine(string.Format("Bedroom night-time window:      {0}", bedroomNightWindow));
            stringBuilder.AppendLine("Mechanical ventilation check:   Full year; annual occupied hours are space-specific");
            stringBuilder.AppendLine(string.Format("Communal corridor check:        {0}", communalCorridorCheck));
        }

        /// <summary>
        /// The number of days in the annual series, ONLY where that series is exactly a standard 365- or
        /// 366-day year (8760 or 8784 hours) - null for anything else (a partial run, an unusual series
        /// length, or no figure at all), so the calendar-derived clock-hour figures beside it are never
        /// printed against a series that does not actually support them.
        /// </summary>
        private static int? StandardYearDays(int? annualHours)
        {
            if (annualHours == 8760)
            {
                return 365;
            }

            if (annualHours == 8784)
            {
                return 366;
            }

            return null;
        }

        /// <summary>
        /// The basis hours behind Criterion 1 and Criterion 2, kept out of the main table so it stays
        /// compact - one row per space, grouped exactly as <see cref="AppendNaturalVentilationSection"/>
        /// groups them.
        /// </summary>
        private static void AppendAssessmentHoursSection(StringBuilder stringBuilder, TM59AssessmentReport tM59AssessmentReport)
        {
            AppendHeading(stringBuilder, Heading_AssessmentHours);

            List<TM59AssessmentReportSpace> spaces = NaturalVentilationSpaces(tM59AssessmentReport);

            if (spaces.Count == 0)
            {
                stringBuilder.AppendLine("No space was assessed against this criterion.");
                return;
            }

            List<string[]> rows =
            [
                ["Space", "Occupied Summer Hours", "Annual Night Occupied Hours"],
            ];

            foreach (TM59AssessmentReportSpace space in spaces)
            {
                TM59AssessmentReportCheck check_Criterion1 = space.Checks.FirstOrDefault(x => x.Check == TM59AssessmentReport.Check_Criterion1);
                TM59AssessmentReportCheck check_Criterion2 = space.Checks.FirstOrDefault(x => x.Check == TM59AssessmentReport.Check_Criterion2);
                TM59AssessmentReportCheck any = check_Criterion1 ?? check_Criterion2 ?? space.Checks.First();

                rows.Add([any.SpaceName ?? "-", Number(check_Criterion1?.BasisHours), Number(check_Criterion2?.BasisHours)]);
            }

            AppendTable(stringBuilder, rows, rightAligned: [false, true, true]);
        }

        private static void AppendMechanicalVentilationSection(StringBuilder stringBuilder, List<TM59AssessmentReportCheck> tM59AssessmentReportChecks)
        {
            AppendHeading(stringBuilder, Heading_MechanicalVentilation);

            if (tM59AssessmentReportChecks.Count == 0)
            {
                stringBuilder.AppendLine("No space was assessed against this criterion.");
                return;
            }

            List<string[]> rows =
            [
                ["Space", "Internal Condition", "TM59 Application", "Annual Occupied Hours", "Actual", "Limit", "Margin", "Status"],
                .. tM59AssessmentReportChecks.Select(x => new[] { x.SpaceName ?? "-", x.InternalCondition ?? "-", x.Use ?? "-", Number(x.BasisHours), Number(x.Actual), Number(x.Limit), Margin(x.Margin), Display(x.ComplianceStatus) }),
            ];

            AppendTable(stringBuilder, rows, rightAligned: [false, false, false, true, true, true, true, false]);
        }

        private static void AppendRiskSection(StringBuilder stringBuilder, string heading, List<TM59AssessmentReportCheck> tM59AssessmentReportChecks)
        {
            AppendHeading(stringBuilder, heading);

            if (tM59AssessmentReportChecks.Count == 0)
            {
                stringBuilder.AppendLine("No space was assessed against this criterion.");
                return;
            }

            List<string[]> rows =
            [
                ["Space", "Internal Condition", "Annual Hours", "Actual", "Limit", "Margin", "Risk"],
                .. tM59AssessmentReportChecks.Select(x => new[] { x.SpaceName ?? "-", x.InternalCondition ?? "-", Number(x.BasisHours), Number(x.Actual), Number(x.Limit), Margin(x.Margin), Display(x.RiskStatus) }),
            ];

            AppendTable(stringBuilder, rows, rightAligned: [false, false, true, true, true, true, false]);
        }

        /// <summary>
        /// The occupied spaces assessed under natural ventilation, in the order the assessment produced them -
        /// the <see cref="TM59AssessmentReport.OccupiedSpaces"/> entries that carry a natural-ventilation check.
        /// </summary>
        private static List<TM59AssessmentReportSpace> NaturalVentilationSpaces(TM59AssessmentReport tM59AssessmentReport)
        {
            HashSet<TM59AssessmentReportCheck> checks_Natural = [.. tM59AssessmentReport.NaturalVentilationChecks];

            return [.. tM59AssessmentReport.OccupiedSpaces.Where(x => x.Checks.Exists(checks_Natural.Contains))];
        }

        /// <summary>
        /// One criterion's Actual | Limit | Margin | Status - the same four columns, in the same order, as the
        /// mechanical-ventilation and &gt;28 C sections, so natural- and mechanical-ventilation results read with
        /// one convention (Margin = Limit - Actual). A criterion not required of the space (Criterion 2 of a
        /// non-bedroom) is "-" figures with an N/A status, never a blank or a FAIL.
        /// </summary>
        private static string[] CriterionCells(TM59AssessmentReportCheck check)
        {
            if (check == null || check.ComplianceStatus == TM59ComplianceStatus.NotApplicable)
            {
                return ["-", "-", "-", "N/A"];
            }

            return [Number(check.Actual), Number(check.Limit), Margin(check.Margin), Display(check.ComplianceStatus)];
        }

        private static void AppendHeading(StringBuilder stringBuilder, string heading)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(heading);
            stringBuilder.AppendLine(new string('-', heading.Length));
        }

        /// <summary>
        /// Lays out any rectangular table of already-rendered cells - the header is <c>rows[0]</c>. Column
        /// widths are computed from the actual content, so every section shares the same automatic layout
        /// regardless of how many columns it has.
        /// </summary>
        private static void AppendTable(StringBuilder stringBuilder, List<string[]> rows, bool[] rightAligned)
        {
            int[] widths = new int[rows[0].Length];
            for (int i = 0; i < widths.Length; i++)
            {
                widths[i] = rows.Max(x => x[i].Length);
            }

            foreach (string[] row in rows)
            {
                List<string> cells = [];
                for (int i = 0; i < row.Length; i++)
                {
                    cells.Add(rightAligned[i] ? row[i].PadLeft(widths[i]) : row[i].PadRight(widths[i]));
                }

                stringBuilder.AppendLine(string.Join("  ", cells).TrimEnd());
            }
        }

        private static string Number(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "-";
        }

        /// <summary>Signed, so remaining allowance and an exceeded threshold are distinguishable at a glance.</summary>
        private static string Margin(int? value)
        {
            if (!value.HasValue)
            {
                return "-";
            }

            return value.Value > 0 ? string.Format(CultureInfo.InvariantCulture, "+{0}", value.Value) : value.Value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Display(TM59ComplianceStatus tM59ComplianceStatus)
        {
            return tM59ComplianceStatus switch
            {
                TM59ComplianceStatus.Pass => "PASS",
                TM59ComplianceStatus.Fail => "FAIL",
                TM59ComplianceStatus.NotApplicable => "N/A",
                _ => "-",
            };
        }

        private static string Display(TM59RiskStatus tM59RiskStatus)
        {
            return tM59RiskStatus switch
            {
                TM59RiskStatus.Acceptable => "ACCEPTABLE",
                TM59RiskStatus.SignificantRisk => "SIGNIFICANT RISK",
                _ => "-",
            };
        }

        /// <summary>
        /// Short enough to stay useful in a panel. States the engineering meaning of each column and each
        /// criterion; the historical SAM-vs-TAS validation evidence lives in
        /// <c>documentation/PartO-TAS-VALIDATION.md</c>, not here.
        /// </summary>
        private static string Legend()
        {
            return
                "Actual  Hours exceeding the stated temperature/comfort threshold.\r\n" +
                "Limit   Maximum permitted hours for the criterion.\r\n" +
                "Margin  Limit - Actual. Positive = allowance remaining, negative = threshold exceeded.\r\n" +
                "        Status is taken from the assessment's own verdict, never re-derived from Margin:\r\n" +
                "        Criterion 1, >26 C and >28 C are strict, so a zero margin is a failure, while\r\n" +
                "        Criterion 2 is inclusive, so a zero margin passes.\r\n" +
                "\r\n" +
                "Criterion 1     Adaptive thermal comfort criterion for applicable naturally ventilated\r\n" +
                "                occupied rooms, against Occupied Summer Hours (see Assessment Basis and\r\n" +
                "                Assessment Hours).\r\n" +
                "Criterion 2     Bedroom night-time overheating criterion. Applies to bedrooms only.\r\n" +
                "                Assessed during 22:00-07:00 throughout the year against Annual Night\r\n" +
                "                Occupied Hours.\r\n" +
                "C1 / C2         Criterion 1 / Criterion 2 columns of the natural ventilation table, each\r\n" +
                "                stated as Actual, Limit, Margin and Status.\r\n" +
                ">26 C hours     Fixed-temperature criterion for applicable mechanically ventilated occupied\r\n" +
                "                rooms, against Annual Occupied Hours.\r\n" +
                ">28 C hours     Annual hours above 28 C.\r\n" +
                "                For TM59 communal corridors this is the TM59 overheating-risk check.\r\n" +
                "                For other spaces shown under Supplementary >28 C Checks it is advisory\r\n" +
                "                engineering information only.\r\n" +
                "\r\n" +
                TM59ApplicationLegend() +
                "\r\n" +
                "ComplianceStatus\r\n" +
                "    PASS  the applicable criterion was satisfied.\r\n" +
                "    FAIL  the applicable criterion was exceeded.\r\n" +
                "    N/A   the criterion is not required of that space.\r\n" +
                "RiskStatus\r\n" +
                "    ACCEPTABLE       the >28 C reference threshold was not exceeded.\r\n" +
                "    SIGNIFICANT RISK the threshold was exceeded. Highlight for design review. This is NOT\r\n" +
                "                     an occupied-space compliance failure and never changes the TM59\r\n" +
                "                     occupied-space assessment above.\r\n";
        }

        /// <summary>
        /// A short explanation of each TM59 Application a space can be assessed under - an assessment ROLE,
        /// not necessarily the complete architectural room type (a Studio is Sleeping, Living AND Cooking at
        /// once). The values listed are read off the real <see cref="TM59SpaceApplication"/> enum rather
        /// than an independently typed-out list, so a future application added to the enum appears here
        /// automatically. <c>Undefined</c> is excluded deliberately - it is never present in a check's
        /// <see cref="TM59AssessmentReportCheck.Use"/> string (see <see cref="TM59AssessmentReport"/>'s
        /// <c>Use</c> method, which only ever joins Sleeping/Living/Cooking), so explaining it here would
        /// describe a value a reader can never actually see.
        /// </summary>
        private static string TM59ApplicationLegend()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("TM59 Application");

            foreach (TM59SpaceApplication tM59SpaceApplication in Enum.GetValues(typeof(TM59SpaceApplication)))
            {
                if (tM59SpaceApplication == TM59SpaceApplication.Undefined)
                {
                    continue;
                }

                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "    {0,-9} {1}", tM59SpaceApplication, TM59ApplicationDescription(tM59SpaceApplication)));
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("    Multiple applications may apply to one space, for example:");
            stringBuilder.AppendLine("    Living, Cooking");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// A one-line explanation for a known application; falls back to the enum's own
        /// <c>[Description]</c> attribute for any value this formatter was not updated to describe by name,
        /// so a future application still gets a line rather than being silently skipped.
        /// </summary>
        private static string TM59ApplicationDescription(TM59SpaceApplication tM59SpaceApplication)
        {
            return tM59SpaceApplication switch
            {
                TM59SpaceApplication.Sleeping => "Space assessed as a bedroom/sleeping space.",
                TM59SpaceApplication.Living => "Space assessed as a living space.",
                TM59SpaceApplication.Cooking => "Space assessed as a kitchen/cooking space.",
                _ => tM59SpaceApplication.Description(),
            };
        }
    }
}
