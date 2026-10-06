// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Reporting;
using SAM.Core.Reporting;
using SAM.Tests.Helpers;
using SAM.Units;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Phase-2 PR2C: the Space Design Load Summary document built from the PR2B typed data
    /// (<see cref="SpaceDesignLoadDocumentData"/>). Values come from <see cref="SpaceDesignLoadFixture"/> (real Tas
    /// peaks of Bathroom_2 and Studio 1_0 as the production conversion stores them). The PDF itself is checked in
    /// PdfRendererTests.SpaceDesignLoad.
    /// </summary>
    public class SpaceDesignLoadSummaryTests
    {
        private const string Bathroom_SI = "A_Bathroom_2_SI";
        private const string Bathroom_IP = "A_Bathroom_2_IP";
        private const string Studio_SI = "B_Studio_1_0_SI";

        private static DocumentSection Section(Document document, string id)
        {
            return document.Sections.Single(x => x.Id == id);
        }

        private static TableBlock Table(Document document, string sectionId, string blockId)
        {
            return Section(document, sectionId).Blocks.OfType<TableBlock>().Single(x => x.Id == blockId);
        }

        private static TableRow Row(TableBlock tableBlock, string label)
        {
            return tableBlock.Rows.Single(x => x.Cells[0].Text == label);
        }

        private static string Value(Document document, string sectionId, string label)
        {
            return Section(document, sectionId).Blocks.OfType<KeyValueBlock>().SelectMany(x => x.Rows).Single(x => x.Label == label).Value.Text;
        }

        private static string Text(Document document)
        {
            List<string> texts = new List<string>();
            foreach (DocumentSection documentSection in document.Sections)
            {
                texts.Add(documentSection.Title);
                foreach (DocumentBlock documentBlock in documentSection.Blocks)
                {
                    switch (documentBlock)
                    {
                        case NoticeBlock noticeBlock:
                            texts.Add(noticeBlock.Text);
                            break;

                        case TableBlock tableBlock:
                            texts.Add(tableBlock.Title);
                            texts.AddRange(tableBlock.Columns.Select(x => x.Header));
                            break;

                        case KeyValueBlock keyValueBlock:
                            texts.AddRange(keyValueBlock.Rows.Select(x => x.Label));
                            break;
                    }
                }
            }

            texts.AddRange(document.FormattedValues().Select(x => x.Text));
            texts.AddRange(document.Footer.Lines);
            return string.Join("\n", texts.Where(x => x != null));
        }

        // ---------- structure and reuse ----------

        [Fact]
        public void Sections_AreIdentityCriteriaSizingHeatingCoolingResults()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);

            Assert.Equal("space-design-load-summary", document.Id);
            Assert.Equal("Space Design Load Summary", document.Metadata.Title);
            Assert.Equal("Bathroom_2", document.Metadata.Subject);
            Assert.Equal(new[] { "identity", "design-criteria", "sizing", "heating", "cooling", "results" }, document.Sections.Select(x => x.Id));
        }

        /// <summary>
        /// The identity, design-criteria and sizing sections and the footer are the Phase-1 builders' own output: the
        /// same blocks as in the Space Assumptions document of the same space.
        /// </summary>
        [Fact]
        public void Phase1Sections_AndFooter_AreTheSpaceAssumptionsOnes()
        {
            AnalyticalModel analyticalModel = SpaceDesignLoadFixture.Model("Bathroom_2", SpaceDesignLoadFixture.Bathroom());
            Space space = analyticalModel.AdjacencyCluster.GetSpaces().Single();

            Document summary = SpaceDesignLoadFixture.Document(analyticalModel, UnitStyle.SI);
            Document assumptions = SAM.Analytical.Reporting.Create.SpaceAssumptions(SAM.Analytical.Reporting.Create.DocumentContext(analyticalModel, ReportingFixture.Options()), space);

            foreach (string id in new[] { "identity", "design-criteria", "sizing" })
            {
                Assert.Equal(Json(assumptions, id), Json(summary, id));
            }

            Assert.Equal(assumptions.Footer.Lines, summary.Footer.Lines);
        }

        private static string Json(Document document, string sectionId)
        {
            DocumentSection documentSection = Section(document, sectionId);
            return new Document("section", new DocumentMetadata(), new[] { documentSection }, null).ToJson();
        }

        // ---------- Bathroom_2 ----------

        /// <summary>
        /// Bathroom_2's two very different heating peaks stay independent, each with its own time and state.
        /// </summary>
        [Fact]
        public void Bathroom_SI_HeatingDesignDayAndFullYear_AreIndependent()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);
            TableBlock peak = Table(document, "heating", "heating-peak");

            Assert.Equal(new[] { string.Empty, SpaceLoadResultSectionBuilder_Columns.DesignDay, SpaceLoadResultSectionBuilder_Columns.FullYear }, peak.Columns.Select(x => x.Header));
            Assert.Equal(SpaceDesignLoadFixture.HeatingDesignDayName, Value(document, "heating", "Design day"));

            TableRow load = Row(peak, "Peak sensible load");
            Assert.Equal(("1,140", "W"), (load.Cells[1].Text, load.Cells[1].Unit));
            Assert.Equal(("104", "W"), (load.Cells[2].Text, load.Cells[2].Unit));

            Assert.Equal(new[] { "16.0", "16.0" }, Row(peak, "Room dry bulb").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "13.9", "15.9" }, Row(peak, "Room resultant").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "20", "35" }, Row(peak, "Room RH").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "2.2", "3.9" }, Row(peak, "Room humidity ratio").Cells.Skip(1).Select(x => x.Text));

            // The design day's outdoor state is not in the results: shown as unavailable, never invented.
            TableRow outdoor = Row(peak, "Outdoor dry bulb");
            Assert.Equal(Availability.NotAvailable, outdoor.Cells[1].Availability);
            Assert.Equal("—", outdoor.Cells[1].Text);
            Assert.Equal(("-2.3", "°C"), (outdoor.Cells[2].Text, outdoor.Cells[2].Unit));
            Assert.Equal("100", Row(peak, "Outdoor RH").Cells[2].Text);
        }

        [Fact]
        public void Bathroom_IP_UsesImperialUnits_AndTheSameStates()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_IP);
            TableBlock peak = Table(document, "heating", "heating-peak");

            TableRow load = Row(peak, "Peak sensible load");
            Assert.Equal(("3,889", "Btu/h"), (load.Cells[1].Text, load.Cells[1].Unit));
            Assert.Equal(("355", "Btu/h"), (load.Cells[2].Text, load.Cells[2].Unit));
            Assert.Equal(("60.8", "°F"), (Row(peak, "Room dry bulb").Cells[1].Text, Row(peak, "Room dry bulb").Cells[1].Unit));
            Assert.Equal("27.9", Row(peak, "Outdoor dry bulb").Cells[2].Text);
            Assert.Equal("—", Row(peak, "Outdoor dry bulb").Cells[1].Text);

            TableBlock sensible = Table(document, "heating", "heating-sensible");
            Assert.All(sensible.Columns.Skip(1), x => Assert.Equal("Btu/h", x.Unit));

            Assert.Equal(new[] { "0", "0" }, Row(Table(document, "cooling", "cooling-peak"), "Peak sensible load").Cells.Skip(1).Select(x => x.Text));

            string json = document.ToJson();
            foreach (string symbol in new[] { "\"W\"", "\"kW\"", "°C", "W/m²" })
            {
                Assert.DoesNotContain(symbol, json);
            }
        }

        /// <summary>
        /// Bathroom_2 has no cooling demand: a real zero at both peaks, shown as 0 W with the reason, never as
        /// unavailable, and with no invented hour, state or breakdown.
        /// </summary>
        [Fact]
        public void Bathroom_CoolingZero_IsZero_NotUnavailable()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);
            DocumentSection cooling = Section(document, "cooling");
            TableBlock peak = Table(document, "cooling", "cooling-peak");

            Assert.Equal(new[] { "Peak sensible load" }, peak.Rows.Select(x => x.Cells[0].Text));
            Assert.All(peak.Rows[0].Cells.Skip(1), x =>
            {
                Assert.Equal("0", x.Text);
                Assert.Equal("W", x.Unit);
                Assert.Equal(Availability.Available, x.Availability);
            });

            Assert.Equal(SpaceDesignLoadFixture.CoolingDesignDayName, Value(document, "cooling", "Design day"));
            Assert.Contains(cooling.Blocks.OfType<NoticeBlock>(), x => x.Level == NoticeLevel.Note && x.Text == "0 W: no cooling demand on the design day or over the full year, so no peak hour, room or outdoor state or component breakdown.");
            Assert.DoesNotContain(cooling.Blocks.OfType<TableBlock>(), x => x.Id.EndsWith("-sensible") || x.Id.EndsWith("-latent"));
            Assert.Equal("Available", Row(Table(document, "results", "results"), "Status").Cells[2].Text);
        }

        // ---------- Studio 1_0: heated and cooled ----------

        [Fact]
        public void Studio_CoolingDesignDayAndFullYear_AreIndependent()
        {
            Document document = SpaceDesignLoadFixture.Document(Studio_SI);
            TableBlock peak = Table(document, "cooling", "cooling-peak");

            Assert.Equal(new[] { "1,973", "1,972" }, Row(peak, "Peak sensible load").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "00:00–01:00", "3 Jul 19:00–20:00 (HOY 4412)" }, Row(peak, "Peak hour").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "19.8", "19.0" }, Row(peak, "Room dry bulb").Cells.Skip(1).Select(x => x.Text));

            // Solar is 0 at the design-day peak and 836 W at the full-year one: nothing is shared between them.
            TableBlock sensible = Table(document, "cooling", "cooling-sensible");
            Assert.Equal(new[] { "0", "836" }, Row(sensible, "Solar").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "971", "298" }, Row(sensible, "Building heat transfer").Cells.Skip(1).Select(x => x.Text));

            TableBlock latent = Table(document, "cooling", "cooling-latent");
            Assert.Equal(new[] { "77", "110" }, Row(latent, "Occupancy (latent)").Cells.Skip(1).Select(x => x.Text));
        }

        /// <summary>
        /// The headline is the sensible load the solver reports (dry-bulb control); latent terms are listed apart and
        /// never folded into it, and no sensible + latent total is shown.
        /// </summary>
        [Fact]
        public void PeakLoad_IsLabelledSensible_LatentKeptApart_NoTotal()
        {
            Document document = SpaceDesignLoadFixture.Document(Studio_SI);

            foreach (string what in new[] { "heating", "cooling" })
            {
                Assert.Equal("Peak sensible load", Table(document, what, what + "-peak").Rows[0].Cells[0].Text);
                Assert.Equal("Sensible load components at peak", Table(document, what, what + "-sensible").Title);
                Assert.Equal("Latent gains at the sensible peak hour", Table(document, what, what + "-latent").Title);
            }

            // The relabel leaves the solver values untouched.
            Assert.Equal(new[] { "2,268", "802" }, Row(Table(document, "heating", "heating-peak"), "Peak sensible load").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "1,973", "1,972" }, Row(Table(document, "cooling", "cooling-peak"), "Peak sensible load").Cells.Skip(1).Select(x => x.Text));

            IEnumerable<TableBlock> tables = document.Sections.SelectMany(x => x.Blocks).OfType<TableBlock>();
            Assert.DoesNotContain(tables.SelectMany(x => x.Rows), x => x.Cells[0].Text.IndexOf("total", System.StringComparison.OrdinalIgnoreCase) >= 0);
            Assert.DoesNotContain(tables.SelectMany(x => x.Rows), x => x.Cells[0].Text == "Peak load");
        }

        /// <summary>
        /// Component values keep the sign the result stores: heating losses negative, cooling gains positive, and a
        /// negative term on the cooling side stays negative.
        /// </summary>
        [Fact]
        public void Components_KeepTheirStoredSign()
        {
            Document document = SpaceDesignLoadFixture.Document(Studio_SI);

            TableBlock heating = Table(document, "heating", "heating-sensible");
            Assert.Equal(new[] { "-321", "-154" }, Row(heating, "Infiltration / ventilation").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "-895", "-328" }, Row(heating, "External conduction — opaque").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "0", "105" }, Row(heating, "Occupancy (sensible)").Cells.Skip(1).Select(x => x.Text));

            TableBlock cooling = Table(document, "cooling", "cooling-sensible");
            Assert.Equal(new[] { "898", "219" }, Row(cooling, "External conduction — opaque").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "-77", "-78" }, Row(cooling, "External conduction — glazing").Cells.Skip(1).Select(x => x.Text));

            // Every value in a component table shares the section's power unit, printed once in the header.
            Assert.All(heating.Columns.Skip(1), x => Assert.Equal("W", x.Unit));
        }

        /// <summary>
        /// Sensible and latent terms are separate tables; no total or net balance is added.
        /// </summary>
        [Fact]
        public void Components_LatentApart_AndNoDerivedTotal()
        {
            Document document = SpaceDesignLoadFixture.Document(Studio_SI);

            Assert.DoesNotContain(Table(document, "heating", "heating-sensible").Rows, x => x.Cells[0].Text.Contains("latent"));
            Assert.All(Table(document, "heating", "heating-latent").Rows, x => Assert.Contains("latent", x.Cells[0].Text));

            string text = Text(document);
            foreach (string word in new[] { "Net", "Total", "Sum", "Balance", "Derived" })
            {
                Assert.DoesNotContain(document.Sections.SelectMany(x => x.Blocks).OfType<TableBlock>().SelectMany(x => x.Rows), x => x.Cells[0].Text.StartsWith(word));
            }

            Assert.Contains("No total is derived", text);
        }

        /// <summary>
        /// As for zero-area fabric, a term stored as exactly 0 at both peaks is named in one note rather than listed;
        /// a term missing at one peak is never taken as zero, so it stays listed.
        /// </summary>
        [Fact]
        public void Components_ZeroAtBothPeaks_AreNamedInANote_MissingIsNotZero()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);
            TableBlock sensible = Table(document, "heating", "heating-sensible");

            Assert.Equal(new[] { "Infiltration / ventilation", "Building heat transfer", "External conduction — opaque" }, sensible.Rows.Select(x => x.Cells[0].Text));
            Assert.DoesNotContain(Section(document, "heating").Blocks, x => x.Id == "heating-latent");

            NoticeBlock zero = Section(document, "heating").Blocks.OfType<NoticeBlock>().Single(x => x.Id == "heating-components-zero");
            // Sensible and latent are folded separately: no latent table, so the latent terms are their own sentence.
            Assert.Equal("Zero at both peaks (not listed): Solar, Lighting, Occupancy (sensible), Equipment (sensible), Air movement (inter-zone), External conduction — glazing, Air handling unit. Latent gains: zero at both peaks (Occupancy, Equipment).", zero.Text);
            Assert.DoesNotContain(Section(document, "heating").Blocks, x => x.Id == "heating-latent-note");

            // Glazing stored at the design day only: 0 there, not reported over the year - listed, not folded.
            SpaceLoadPeak designDay = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 100) { DesignDayName = "DD", HourOfDay = 5 };
            designDay.SetComponent(LoadPeakComponent.ExternalConductionGlazing, 0);
            designDay.SetComponent(LoadPeakComponent.InfiltrationVentilation, -100);
            SpaceLoadPeak annual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 50) { HourOfYear = 10, HourOfDay = 10 };
            annual.SetComponent(LoadPeakComponent.InfiltrationVentilation, -50);

            Document partial = SpaceDesignLoadFixture.Document(SpaceDesignLoadFixture.Model("Space", SpaceDesignLoadFixture.Result(LoadType.Heating, designDay, annual)), UnitStyle.SI);
            TableRow glazing = Row(Table(partial, "heating", "heating-sensible"), "External conduction — glazing");
            Assert.Equal("0", glazing.Cells[1].Text);
            Assert.Equal(Availability.NotAvailable, glazing.Cells[2].Availability);
            Assert.Equal("Not reported by the simulation", glazing.Cells[2].Note);
            Assert.DoesNotContain(Section(partial, "heating").Blocks, x => x.Id == "heating-components-zero");
        }

        /// <summary>
        /// PR2F B1 / A2: with a latent table, each kind's zero note sits under its own table, the sensible note names no
        /// latent term, and the latent table says it holds internal gains at the sensible peak hour, not the latent load.
        /// </summary>
        [Fact]
        public void Components_ZeroNotes_AreSplitByKind_UnderTheirOwnTable()
        {
            Document document = SpaceDesignLoadFixture.Document(Studio_SI);
            List<string> ids = Section(document, "heating").Blocks.Select(x => x.Id).ToList();

            NoticeBlock sensible = Notice(document, "heating", "heating-components-zero");
            NoticeBlock latent = Notice(document, "heating", "heating-latent-zero");
            Assert.Equal("Zero at both peaks (not listed): Solar, Lighting, Air movement (inter-zone), Air handling unit", sensible.Text);
            Assert.Equal("Zero at both peaks (not listed): Equipment (latent)", latent.Text);

            Assert.True(ids.IndexOf("heating-sensible") < ids.IndexOf("heating-components-zero"));
            Assert.True(ids.IndexOf("heating-components-zero") < ids.IndexOf("heating-latent"));
            Assert.True(ids.IndexOf("heating-latent") < ids.IndexOf("heating-latent-zero"));
            Assert.True(ids.IndexOf("heating-latent-zero") < ids.IndexOf("heating-latent-note"));

            string latentNote = Notice(document, "heating", "heating-latent-note").Text;
            Assert.StartsWith("Latent gains: internal gains only (occupancy, equipment) at the hour of the sensible peak.", latentNote);
            Assert.Contains("latent load", latentNote);
            Assert.Contains("is not in the results and is not derived", latentNote);
        }

        /// <summary>
        /// PR2F C1: the peak-hour note explains the "Peak hour" row, so it is printed only when a section prints one.
        /// </summary>
        [Theory]
        [InlineData(Bathroom_SI, true)]
        [InlineData(Studio_SI, true)]
        [InlineData("C_NotSimulated_SI", false)]
        [InlineData("D_PeaksNotRecorded_SI", false)]
        [InlineData("E_Ambiguous_SI", false)]
        public void PeakHourNote_OnlyWhenAPeakHourIsPrinted(string name, bool expected)
        {
            Document document = SpaceDesignLoadFixture.Document(name);
            bool peakHour = document.Sections.SelectMany(x => x.Blocks).OfType<TableBlock>().SelectMany(x => x.Rows).Any(x => x.Cells[0].Text == "Peak hour");

            Assert.Equal(expected, peakHour);
            Assert.Equal(expected, Section(document, "results").Blocks.Any(x => x.Id == "results-time-note"));
        }

        [Fact]
        public void PeakHourNote_NotPrinted_WhenBothPeaksAreZero()
        {
            SpaceLoadPeak designDay = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0) { DesignDayName = "DD" };
            SpaceLoadPeak annual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 0);
            Document document = SpaceDesignLoadFixture.Document(SpaceDesignLoadFixture.Model("Space", SpaceDesignLoadFixture.Result(LoadType.Heating, designDay, annual)), UnitStyle.SI);

            Assert.DoesNotContain(Section(document, "results").Blocks, x => x.Id == "results-time-note");
        }

        private static NoticeBlock Notice(Document document, string section, string id)
        {
            return Section(document, section).Blocks.OfType<NoticeBlock>().Single(x => x.Id == id);
        }

        // ---------- time ----------

        /// <summary>
        /// A design-day peak shows the hour of the design day it covers and the design day's name, never a date.
        /// </summary>
        [Fact]
        public void DesignDayTime_IsAnHourOfTheDay_WithNoCalendarDate()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);
            FormattedValue designDay = Row(Table(document, "heating", "heating-peak"), "Peak hour").Cells[1];

            Assert.Equal("23:00–24:00", designDay.Text);
            Assert.Equal(Availability.Available, designDay.Availability);
            foreach (string month in new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "2018", "HOY" })
            {
                Assert.DoesNotContain(month, designDay.Text);
            }
        }

        /// <summary>
        /// Every design-day peak of the real fixtures, heating and cooling, is an hour of the day only: no HOY, even when
        /// the stored peak carries a stray hour of the year.
        /// </summary>
        [Fact]
        public void DesignDayTime_NeverShowsAnHourOfTheYear()
        {
            foreach (string name in new[] { Bathroom_SI, Studio_SI })
            {
                Document document = SpaceDesignLoadFixture.Document(name);
                foreach (string section in new[] { "heating", "cooling" })
                {
                    TableBlock peak = document.Sections.Single(x => x.Id == section).Blocks.OfType<TableBlock>().SingleOrDefault(x => x.Id == section + "-peak");
                    //An all-zero section (Bathroom_2 cooling) has no peak hour row at all.
                    TableRow peakHour = peak?.Rows.SingleOrDefault(x => x.Cells[0].Text == "Peak hour");
                    if (peakHour != null)
                    {
                        Assert.DoesNotContain("HOY", peakHour.Cells[1].Text);
                        Assert.Contains("HOY", peakHour.Cells[2].Text);
                    }
                }
            }

            SpaceLoadPeak stray = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 100) { DesignDayName = "DD", HourOfDay = 5, HourOfYear = 1607 };
            Document strayDocument = SpaceDesignLoadFixture.Document(SpaceDesignLoadFixture.Model("Space", SpaceDesignLoadFixture.Result(LoadType.Heating, stray, null)), UnitStyle.SI);
            Assert.Equal("05:00–06:00", Row(Table(strayDocument, "heating", "heating-peak"), "Peak hour").Cells[1].Text);
        }

        /// <summary>
        /// Real Tas peaks (SAM_Tas#69 on final1b/open.tsd and pr3/final/bridge.tsd) with their user-facing HOY, the
        /// stored 0-based hour of the year + 1: Bathroom_2 heating 8553 → HOY 8554, Studio 1_0 heating 0 → HOY 1 and
        /// cooling 4411 → HOY 4412. The date and the HOY come from the same stored hour, so they cannot drift apart.
        /// </summary>
        [Fact]
        public void AnnualTime_OfRealFixturePeaks_CarriesTheOneBasedHourOfTheYear()
        {
            Assert.Equal("23 Dec 09:00–10:00 (HOY 8554)", Row(Table(SpaceDesignLoadFixture.Document(Bathroom_SI), "heating", "heating-peak"), "Peak hour").Cells[2].Text);

            Document studio = SpaceDesignLoadFixture.Document(Studio_SI);
            Assert.Equal("1 Jan 00:00–01:00 (HOY 1)", Row(Table(studio, "heating", "heating-peak"), "Peak hour").Cells[2].Text);
            Assert.Equal("3 Jul 19:00–20:00 (HOY 4412)", Row(Table(studio, "cooling", "cooling-peak"), "Peak hour").Cells[2].Text);
        }

        /// <summary>
        /// The HOY convention at its ends and against the calendar: HOY = (day of year − 1) × 24 + hour + 1, for the first
        /// hour of every month and both ends of the year. The stored hour is 0-based; the displayed one is 1–8760.
        /// </summary>
        [Fact]
        public void AnnualTime_HourOfTheYear_Is1To8760_AndAgreesWithTheDate()
        {
            Assert.Equal("1 Jan 00:00–01:00 (HOY 1)", AnnualPeakHour(0));
            Assert.Equal("31 Dec 23:00–24:00 (HOY 8760)", AnnualPeakHour(8759));

            // One past the last hour is not a valid stored hour: no date and no HOY 8761.
            Assert.Equal("—", AnnualPeakHour(8760));

            System.DateTime start = new System.DateTime(Analytical.Reporting.Create.ReferenceYear, 1, 1);
            for (int month = 1; month <= 12; month++)
            {
                System.DateTime dateTime = new System.DateTime(Analytical.Reporting.Create.ReferenceYear, month, 1, 0, 0, 0);
                int hourOfYear = (int)(dateTime - start).TotalHours;
                Assert.Equal(string.Format(System.Globalization.CultureInfo.InvariantCulture, "1 {0:MMM} 00:00–01:00 (HOY {1})", dateTime, (dateTime.DayOfYear - 1) * 24 + 1), AnnualPeakHour(hourOfYear));
            }
        }

        private static string AnnualPeakHour(int hourOfYear)
        {
            SpaceLoadPeak annual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 250) { HourOfYear = hourOfYear, HourOfDay = hourOfYear % 24 };
            Document document = SpaceDesignLoadFixture.Document(SpaceDesignLoadFixture.Model("Space", SpaceDesignLoadFixture.Result(LoadType.Heating, null, annual)), UnitStyle.SI);
            return Row(Table(document, "heating", "heating-peak"), "Peak hour").Cells[2].Text;
        }

        [Theory]
        [InlineData(8553, 9, "23 Dec 09:00–10:00 (HOY 8554)")]
        [InlineData(0, 0, "1 Jan 00:00–01:00 (HOY 1)")]
        [InlineData(8759, 23, "31 Dec 23:00–24:00 (HOY 8760)")]
        [InlineData(1416, 0, "1 Mar 00:00–01:00 (HOY 1417)")]
        public void AnnualTime_IsDayMonthAndHour_WithNoYear(int hourOfYear, int hourOfDay, string expected)
        {
            SpaceLoadPeak annual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 250) { HourOfYear = hourOfYear, HourOfDay = hourOfDay };
            Document document = SpaceDesignLoadFixture.Document(SpaceDesignLoadFixture.Model("Space", SpaceDesignLoadFixture.Result(LoadType.Heating, null, annual)), UnitStyle.SI);

            TableRow peakHour = Row(Table(document, "heating", "heating-peak"), "Peak hour");
            Assert.Equal(expected, peakHour.Cells[2].Text);
            Assert.DoesNotContain(Create_ReferenceYear, peakHour.Cells[2].Text);

            // The design-day peak is missing from this result: unavailable, not zero and not a date.
            Assert.Equal(("—", Availability.NotAvailable), (peakHour.Cells[1].Text, peakHour.Cells[1].Availability));
            Assert.Equal("—", Row(Table(document, "heating", "heating-peak"), "Peak sensible load").Cells[1].Text);
        }

        private static readonly string Create_ReferenceYear = SAM.Analytical.Reporting.Create.ReferenceYear.ToString();

        // ---------- result states ----------

        [Fact]
        public void NotSimulated_IsANotice_NotAZeroLoad()
        {
            Document document = SpaceDesignLoadFixture.Document("C_NotSimulated_SI");

            foreach (string id in new[] { "heating", "cooling" })
            {
                DocumentSection documentSection = Section(document, id);
                NoticeBlock noticeBlock = Assert.IsType<NoticeBlock>(Assert.Single(documentSection.Blocks));
                Assert.Equal(NoticeLevel.Information, noticeBlock.Level);
                Assert.StartsWith("Not simulated:", noticeBlock.Text);
                Assert.Contains("This is not a zero load.", noticeBlock.Text);
            }

            TableBlock results = Table(document, "results", "results");
            Assert.Equal(new[] { "Not simulated", "Not simulated" }, Row(results, "Status").Cells.Skip(1).Select(x => x.Text));
            Assert.All(Row(results, "Result source").Cells.Skip(1), x => Assert.Equal(Availability.NotAvailable, x.Availability));
        }

        [Fact]
        public void PeaksNotRecorded_SaysToReRun_AndReadsNoLegacyValue()
        {
            Document document = SpaceDesignLoadFixture.Document("D_PeaksNotRecorded_SI");

            foreach (string id in new[] { "heating", "cooling" })
            {
                NoticeBlock noticeBlock = Assert.IsType<NoticeBlock>(Assert.Single(Section(document, id).Blocks));
                Assert.Equal(NoticeLevel.Warning, noticeBlock.Level);
                Assert.StartsWith("Peaks not recorded:", noticeBlock.Text);
                Assert.Contains("Re-run the simulation", noticeBlock.Text);
            }

            Assert.Equal(new[] { "Peaks not recorded", "Peaks not recorded" }, Row(Table(document, "results", "results"), "Status").Cells.Skip(1).Select(x => x.Text));

            // The legacy -1 is never shown.
            Assert.DoesNotContain("-1", Text(document));
        }

        /// <summary>
        /// Two heating results with peaks: nothing is chosen - not the first, not the largest, not Tas. The cooling,
        /// with one result, is reported. Choosing happens only upstream, through the result source.
        /// </summary>
        [Fact]
        public void Ambiguous_ChoosesNothing_UntilASourceIsGiven()
        {
            Document document = SpaceDesignLoadFixture.Document("E_Ambiguous_SI");

            NoticeBlock noticeBlock = Assert.IsType<NoticeBlock>(Assert.Single(Section(document, "heating").Blocks));
            Assert.Equal(NoticeLevel.Warning, noticeBlock.Level);
            Assert.StartsWith("Ambiguous:", noticeBlock.Text);

            string text = Text(document);
            foreach (string value in new[] { "1,140", "1,500", "104", "OpenStudio heating DD" })
            {
                Assert.DoesNotContain(value, text);
            }

            TableBlock results = Table(document, "results", "results");
            Assert.Equal(new[] { "Ambiguous: none chosen", "Available" }, Row(results, "Status").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(Availability.NotAvailable, Row(results, "Result source").Cells[1].Availability);
            Assert.Equal("0", Row(Table(document, "cooling", "cooling-peak"), "Peak sensible load").Cells[1].Text);

            Document tas = SpaceDesignLoadFixture.Document("E_Ambiguous_SI", "Tas");
            Assert.Equal("1,140", Row(Table(tas, "heating", "heating-peak"), "Peak sensible load").Cells[1].Text);

            Document openStudio = SpaceDesignLoadFixture.Document("E_Ambiguous_SI", "OpenStudio");
            Assert.Equal("1,500", Row(Table(openStudio, "heating", "heating-peak"), "Peak sensible load").Cells[1].Text);
            Assert.Equal("OpenStudio", Row(Table(openStudio, "results", "results"), "Result source").Cells[1].Text);
        }

        // ---------- provenance ----------

        /// <summary>
        /// The results section shows the source and the time the results were read in, and says the match with the
        /// current model is not recorded (Freshness Unknown): it never claims the results are current.
        /// </summary>
        [Fact]
        public void Results_ShowSourceAndReadTime_AndNeverClaimCurrency()
        {
            Document document = SpaceDesignLoadFixture.Document(Bathroom_SI);
            TableBlock results = Table(document, "results", "results");

            Assert.Equal(new[] { "Tas", "Tas" }, Row(results, "Result source").Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(new[] { "27 Aug 2026 18:38", "27 Aug 2026 18:38" }, Row(results, "Read into model").Cells.Skip(1).Select(x => x.Text));
            Assert.All(Row(results, "Matches current model").Cells.Skip(1), x =>
            {
                Assert.Equal("not recorded", x.Text);
                Assert.NotEqual(Availability.Available, x.Availability);
            });

            Assert.All(document.FormattedValues().Where(x => x.Source == ReportValueSource.SimulationResult), x => Assert.Equal(Freshness.Unknown, x.Freshness));
        }

        /// <summary>
        /// Design loads and simulated peaks are shown side by side for reading, with no acceptance rule, verdict or ratio.
        /// </summary>
        [Theory]
        [InlineData(Bathroom_SI)]
        [InlineData(Studio_SI)]
        [InlineData("G_Stress_SI")]
        public void NoPassFailComplianceOrRatio(string name)
        {
            string text = Text(SpaceDesignLoadFixture.Document(name)).ToLowerInvariant();

            foreach (string word in new[] { "pass", "fail", "complian", "undersized", "oversized", "ratio of", "margin" })
            {
                Assert.DoesNotContain(word, text.Replace("humidity ratio", string.Empty));
            }

            Assert.Contains("no acceptance rule is applied", text);
        }

        // ---------- snapshots ----------

        [Theory]
        [InlineData("SpaceDesignLoad_Bathroom_2_SI.json", Bathroom_SI)]
        [InlineData("SpaceDesignLoad_Bathroom_2_IP.json", Bathroom_IP)]
        [InlineData("SpaceDesignLoad_Studio_1_0_SI.json", Studio_SI)]
        [InlineData("SpaceDesignLoad_NotSimulated_SI.json", "C_NotSimulated_SI")]
        [InlineData("SpaceDesignLoad_PeaksNotRecorded_SI.json", "D_PeaksNotRecorded_SI")]
        [InlineData("SpaceDesignLoad_Ambiguous_SI.json", "E_Ambiguous_SI")]
        public void Snapshot_MatchesGolden(string fileName, string name)
        {
            string json = SpaceDesignLoadFixture.Document(name).ToJson();

            // Deterministic: a second build of the same input gives the same text.
            Assert.Equal(json, SpaceDesignLoadFixture.Document(name).ToJson());

            Golden.AssertMatches(fileName, json);
        }

        /// <summary>
        /// The column headers the tests read, as the builder names them.
        /// </summary>
        private static class SpaceLoadResultSectionBuilder_Columns
        {
            public const string DesignDay = "Design day";
            public const string FullYear = "Full year";
        }
    }
}
