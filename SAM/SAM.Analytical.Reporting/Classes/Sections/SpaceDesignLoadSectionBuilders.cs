// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core.Reporting;
using SAM.Units;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SAM.Analytical.Reporting
{
    /// <summary>
    /// Runs a Phase-1 Space Assumptions section builder (identity, design criteria, sizing) on the Space Design Load
    /// data, which carries the same typed parts, so the two documents share one builder and one layout for them.
    /// </summary>
    internal sealed class SpaceDesignLoadPhase1SectionBuilder : ISectionBuilder<SpaceDesignLoadDocumentData>
    {
        private readonly ISectionBuilder<SpaceDocumentData> sectionBuilder;

        public SpaceDesignLoadPhase1SectionBuilder(ISectionBuilder<SpaceDocumentData> sectionBuilder)
        {
            this.sectionBuilder = sectionBuilder ?? throw new ArgumentNullException(nameof(sectionBuilder));
        }

        public string Id => sectionBuilder.Id;

        public DocumentSection Build(SpaceDesignLoadDocumentData data, DocumentContext documentContext)
        {
            return sectionBuilder.Build(Phase1(data), documentContext);
        }

        /// <summary>
        /// The parts a Phase-1 identity, design criteria, sizing or footer builder reads. Nothing is recollected.
        /// </summary>
        internal static SpaceDocumentData Phase1(SpaceDesignLoadDocumentData data)
        {
            return new SpaceDocumentData()
            {
                Identity = data.Identity,
                DesignCriteria = data.DesignCriteria,
                Sizing = data.Sizing,
                Provenance = data.Provenance,
            };
        }
    }

    /// <summary>
    /// The Phase-1 footer (model, SAM version, design-load status, generation time, space GUID), unchanged.
    /// </summary>
    internal sealed class SpaceDesignLoadFooterBuilder : IFooterBuilder<SpaceDesignLoadDocumentData>
    {
        private readonly SpaceFooterBuilder spaceFooterBuilder = new SpaceFooterBuilder();

        public IEnumerable<string> Build(SpaceDesignLoadDocumentData data, DocumentContext documentContext)
        {
            return spaceFooterBuilder.Build(SpaceDesignLoadPhase1SectionBuilder.Phase1(data), documentContext);
        }
    }

    /// <summary>
    /// One load type (heating or cooling) of the Space Design Load Summary: the design-day peak and the full-year peak
    /// side by side, never merged, then the heat-balance components at each peak, signed as stored. It lays out the
    /// typed <see cref="SpaceLoadResultData"/> only: it reads no result, computes no load and applies no rule.
    /// <para>
    /// A load type with no single usable result (<see cref="LoadResultStatus.NotSimulated"/>,
    /// <see cref="LoadResultStatus.PeaksNotRecorded"/>, <see cref="LoadResultStatus.Ambiguous"/>) is one notice saying
    /// why, never a table of zeros; an ambiguous result is never resolved here.
    /// </para>
    /// </summary>
    internal sealed class SpaceLoadResultSectionBuilder : ISectionBuilder<SpaceDesignLoadDocumentData>
    {
        public const string DesignDayColumn = "Design day";
        public const string FullYearColumn = "Full year";

        /// <summary>
        /// Starts the note naming the terms stored as exactly 0 at both peaks, which are not listed. It carries no
        /// unit, so it reads the same in SI and IP.
        /// </summary>
        public const string ZeroAtBothPeaksPrefix = "Zero at both peaks (not listed): ";

        /// <summary>
        /// Starts the one line shown instead of a latent table when every latent term is exactly 0 at both peaks.
        /// </summary>
        public const string LatentZeroAtBothPeaksPrefix = "Latent gains: zero at both peaks";

        /// <summary>
        /// The latent table lists the internal latent gains stored at the sensible peak hour, not the room's latent load.
        /// </summary>
        public const string LatentTableTitle = "Latent gains at the sensible peak hour";

        public const string LatentGainsNote = "Latent gains: internal gains only (occupancy, equipment) at the hour of the sensible peak. The room's latent load, which also includes moisture from infiltration and ventilation, is not in the results and is not derived.";

        private readonly LoadType loadType;

        public SpaceLoadResultSectionBuilder(LoadType loadType)
        {
            if (loadType != LoadType.Heating && loadType != LoadType.Cooling)
            {
                throw new ArgumentOutOfRangeException(nameof(loadType), loadType, "Only heating and cooling have load peaks.");
            }

            this.loadType = loadType;
        }

        public string Id => loadType == LoadType.Heating ? "heating" : "cooling";

        private string Title => loadType == LoadType.Heating ? "Heating" : "Cooling";

        private string What => loadType == LoadType.Heating ? "heating" : "cooling";

        public DocumentSection Build(SpaceDesignLoadDocumentData data, DocumentContext documentContext)
        {
            SpaceLoadResultData spaceLoadResultData = loadType == LoadType.Heating ? data.Heating : data.Cooling;
            if (spaceLoadResultData == null)
            {
                throw new InvalidOperationException(string.Format("The {0} results were not collected.", What));
            }

            if (spaceLoadResultData.Status != LoadResultStatus.Available)
            {
                return new DocumentSection(Id, Title, new DocumentBlock[] { StatusNotice(spaceLoadResultData.Status) });
            }

            IQuantityFormatter quantityFormatter = documentContext.Formatter;
            SpaceLoadPeakData designDay = spaceLoadResultData.DesignDay;
            SpaceLoadPeakData annual = spaceLoadResultData.Annual;

            // One power unit for the section: both peak loads and every component, so all its figures compare directly.
            DisplayUnit displayUnit = SectionFormat.Unit(quantityFormatter, UnitCategory.Power, new[] { designDay, annual }.SelectMany(x => new[] { x.Load }.Concat(x.SensibleComponents.Concat(x.LatentComponents).Select(y => y.Value))));

            List<DocumentBlock> documentBlocks = new List<DocumentBlock>()
            {
                new KeyValueBlock(Id + "-design-day", null, new[] { SectionFormat.Row("Design day", quantityFormatter.Format(designDay.DesignDayName)) }),
            };

            // With no peak above zero there is no peak hour, state or component to show: the loads, and why.
            if (designDay.State != LoadPeakState.Value && annual.State != LoadPeakState.Value)
            {
                documentBlocks.Add(PeakTable(new List<TableRow>() { Pair("Peak sensible load", quantityFormatter, designDay.Load, annual.Load, displayUnit) }));
                if (designDay.State == LoadPeakState.Zero || annual.State == LoadPeakState.Zero)
                {
                    string when = designDay.State != LoadPeakState.Zero ? "over the full year" : annual.State != LoadPeakState.Zero ? "on the design day" : "on the design day or over the full year";
                    documentBlocks.Add(new NoticeBlock(Id + "-no-demand", string.Format("0 {0}: no {1} demand {2}, so no peak hour, room or outdoor state or component breakdown.", displayUnit.Symbol, What, when), NoticeLevel.Note));
                }

                return new DocumentSection(Id, Title, documentBlocks);
            }

            List<TableRow> tableRows = new List<TableRow>()
            {
                Pair("Peak sensible load", quantityFormatter, designDay.Load, annual.Load, displayUnit),
                new TableRow(SectionFormat.Label("Peak hour"), DesignDayHour(quantityFormatter, designDay.HourOfDay), AnnualHour(quantityFormatter, annual.Time, annual.HourOfYear)),
                Pair("Room dry bulb", quantityFormatter, UnitCategory.Temperature, designDay.RoomDryBulbTemperature, annual.RoomDryBulbTemperature),
                Pair("Room resultant", quantityFormatter, UnitCategory.Temperature, designDay.RoomResultantTemperature, annual.RoomResultantTemperature),
                Pair("Room RH", quantityFormatter, UnitCategory.RelativeHumidity, designDay.RoomRelativeHumidity, annual.RoomRelativeHumidity),
                Pair("Room humidity ratio", quantityFormatter, UnitCategory.HumidityRatio, designDay.RoomHumidityRatio, annual.RoomHumidityRatio),
                Pair("Outdoor dry bulb", quantityFormatter, UnitCategory.Temperature, designDay.OutdoorDryBulbTemperature, annual.OutdoorDryBulbTemperature),
                Pair("Outdoor RH", quantityFormatter, UnitCategory.RelativeHumidity, designDay.OutdoorRelativeHumidity, annual.OutdoorRelativeHumidity),
            };

            documentBlocks.Add(PeakTable(tableRows));

            // Sensible and latent terms are folded separately, so each zero note sits under the table of its own kind.
            List<LoadPeakComponent> zeros_Sensible = new List<LoadPeakComponent>();
            List<LoadPeakComponent> zeros_Latent = new List<LoadPeakComponent>();
            TableBlock tableBlock_Sensible = ComponentTable(Id + "-sensible", "Sensible load components at peak", quantityFormatter, displayUnit, designDay, annual, false, zeros_Sensible);
            TableBlock tableBlock_Latent = ComponentTable(Id + "-latent", LatentTableTitle, quantityFormatter, displayUnit, designDay, annual, true, zeros_Latent);
            if (tableBlock_Sensible == null && tableBlock_Latent == null && zeros_Sensible.Count == 0 && zeros_Latent.Count == 0)
            {
                documentBlocks.Add(new NoticeBlock(Id + "-components-missing", "No component breakdown in the results", NoticeLevel.Note));
                return new DocumentSection(Id, Title, documentBlocks);
            }

            // With no latent table, all-zero latent gains are one sentence of their own, named by source, so they never
            // read as part of the sensible list. It shares the sensible note's block so a one-page report stays one page.
            string latentZeroSentence = tableBlock_Latent == null && zeros_Latent.Count != 0
                ? string.Format("{0} ({1}).", LatentZeroAtBothPeaksPrefix, string.Join(", ", zeros_Latent.Select(LatentSourceLabel)))
                : null;

            documentBlocks.Add(tableBlock_Sensible);
            if (zeros_Sensible.Count != 0)
            {
                string text = ZeroAtBothPeaksPrefix + string.Join(", ", zeros_Sensible.Select(ComponentLabel));
                documentBlocks.Add(new NoticeBlock(Id + "-components-zero", latentZeroSentence == null ? text : text + ". " + latentZeroSentence, NoticeLevel.Note));
            }
            else if (latentZeroSentence != null)
            {
                documentBlocks.Add(new NoticeBlock(Id + "-latent-zero", latentZeroSentence, NoticeLevel.Note));
            }

            if (tableBlock_Latent != null)
            {
                documentBlocks.Add(tableBlock_Latent);
                if (zeros_Latent.Count != 0)
                {
                    documentBlocks.Add(new NoticeBlock(Id + "-latent-zero", ZeroAtBothPeaksPrefix + string.Join(", ", zeros_Latent.Select(ComponentLabel)), NoticeLevel.Note));
                }

                documentBlocks.Add(new NoticeBlock(Id + "-latent-note", LatentGainsNote, NoticeLevel.Note));
            }

            documentBlocks.Add(new NoticeBlock(Id + "-components-note", "Components as the simulation reports them at each peak hour: + gain to the room air, − loss from it. No total is derived.", NoticeLevel.Note));

            return new DocumentSection(Id, Title, documentBlocks);
        }

        /// <summary>
        /// Why a load type shows no peaks. Not simulated is information; results that cannot be reported (recorded
        /// before the peak contract, or more than one candidate) are warnings.
        /// </summary>
        private NoticeBlock StatusNotice(LoadResultStatus loadResultStatus)
        {
            switch (loadResultStatus)
            {
                case LoadResultStatus.NotSimulated:
                    return new NoticeBlock(Id + "-status", string.Format("Not simulated: this space has no {0} simulation results, so no {0} peaks are reported. This is not a zero load.", What), NoticeLevel.Information);

                case LoadResultStatus.PeaksNotRecorded:
                    return new NoticeBlock(Id + "-status", string.Format("Peaks not recorded: the {0} results pre-date the design-day and full-year peak record. Re-run the simulation to report the {0} peaks.", What), NoticeLevel.Warning);

                case LoadResultStatus.Ambiguous:
                    return new NoticeBlock(Id + "-status", string.Format("Ambiguous: more than one {0} result records peaks, and none is chosen. Select the result source to report.", What), NoticeLevel.Warning);
            }

            throw new InvalidOperationException(string.Format("No notice for result status {0}.", loadResultStatus));
        }

        private TableBlock PeakTable(IEnumerable<TableRow> tableRows)
        {
            return new TableBlock(Id + "-peak", null, new[]
            {
                new TableColumn(string.Empty, alignment: ColumnAlignment.Left),
                new TableColumn(DesignDayColumn),
                new TableColumn(FullYearColumn),
            }, tableRows);
        }

        /// <summary>
        /// The stored terms of one kind (sensible or latent) at either peak, in <see cref="LoadPeakComponent"/> order,
        /// signs untouched. A term one peak stores and the other does not is "not reported" at the other; at a zero
        /// peak there is no peak hour, so it is not applicable. As for zero-area fabric (Phase 1), a term stored as
        /// exactly 0 at both peaks is not listed but named in <paramref name="zeros"/>; a missing value is never taken
        /// as zero. Null when no term is left to list.
        /// </summary>
        private TableBlock ComponentTable(string id, string title, IQuantityFormatter quantityFormatter, DisplayUnit displayUnit, SpaceLoadPeakData designDay, SpaceLoadPeakData annual, bool latent, List<LoadPeakComponent> zeros)
        {
            IReadOnlyList<SpaceLoadPeakComponentData> components_DesignDay = latent ? designDay.LatentComponents : designDay.SensibleComponents;
            IReadOnlyList<SpaceLoadPeakComponentData> components_Annual = latent ? annual.LatentComponents : annual.SensibleComponents;

            List<TableRow> tableRows = new List<TableRow>();
            foreach (LoadPeakComponent loadPeakComponent in components_DesignDay.Concat(components_Annual).Select(x => x.Component).Distinct().OrderBy(x => x))
            {
                ReportValue<Quantity> designDayValue = Component(designDay, components_DesignDay, loadPeakComponent);
                ReportValue<Quantity> annualValue = Component(annual, components_Annual, loadPeakComponent);
                if (IsZero(designDayValue) && IsZero(annualValue))
                {
                    zeros.Add(loadPeakComponent);
                    continue;
                }

                tableRows.Add(new TableRow(SectionFormat.Label(ComponentLabel(loadPeakComponent)), quantityFormatter.Format(designDayValue, displayUnit), quantityFormatter.Format(annualValue, displayUnit)));
            }

            if (tableRows.Count == 0)
            {
                return null;
            }

            return new TableBlock(id, title, new[]
            {
                new TableColumn("Component", alignment: ColumnAlignment.Left),
                new TableColumn(DesignDayColumn, displayUnit.Symbol),
                new TableColumn(FullYearColumn, displayUnit.Symbol),
            }, tableRows);
        }

        private static bool IsZero(ReportValue<Quantity> reportValue)
        {
            return reportValue.TryGetValue(out Quantity quantity) && quantity.Value == 0;
        }

        private static ReportValue<Quantity> Component(SpaceLoadPeakData spaceLoadPeakData, IReadOnlyList<SpaceLoadPeakComponentData> components, LoadPeakComponent loadPeakComponent)
        {
            SpaceLoadPeakComponentData spaceLoadPeakComponentData = components.FirstOrDefault(x => x.Component == loadPeakComponent);
            if (spaceLoadPeakComponentData != null)
            {
                return spaceLoadPeakComponentData.Value;
            }

            switch (spaceLoadPeakData.State)
            {
                case LoadPeakState.Zero:
                    return ReportValue<Quantity>.NotApplicable("No demand: no peak timestep");

                case LoadPeakState.Unavailable:
                    return ReportValue<Quantity>.NotAvailable(spaceLoadPeakData.Load.Note);
            }

            return ReportValue<Quantity>.NotAvailable("Not reported by the simulation");
        }

        private static TableRow Pair(string label, IQuantityFormatter quantityFormatter, ReportValue<Quantity> designDay, ReportValue<Quantity> annual, DisplayUnit displayUnit)
        {
            return new TableRow(SectionFormat.Label(label), quantityFormatter.Format(designDay, displayUnit), quantityFormatter.Format(annual, displayUnit));
        }

        private static TableRow Pair(string label, IQuantityFormatter quantityFormatter, UnitCategory unitCategory, ReportValue<Quantity> designDay, ReportValue<Quantity> annual)
        {
            return SpaceDesignCriteriaSectionBuilder.Pair(quantityFormatter, label, unitCategory, designDay, annual);
        }

        /// <summary>
        /// A design-day peak hour as the hour of the day it covers ("23:00–24:00"): a design day has no calendar date,
        /// so none is shown.
        /// </summary>
        internal static FormattedValue DesignDayHour(IQuantityFormatter quantityFormatter, ReportValue<int> hourOfDay)
        {
            if (!hourOfDay.TryGetValue(out int hour))
            {
                return SectionFormat.Placeholder(quantityFormatter, hourOfDay);
            }

            return SectionFormat.Text(string.Format(CultureInfo.InvariantCulture, "{0:00}:00–{1:00}:00", hour, hour + 1), hourOfDay);
        }

        /// <summary>
        /// A full-year peak hour as day, month, the hour it covers and its user-facing hour of the year
        /// ("23 Dec 09:00–10:00 (HOY 8554)"). Results carry no year, so none is shown. The HOY is
        /// <see cref="UserHourOfYear"/> of the typed 0-based <paramref name="hourOfYear"/>, the same normalized value the
        /// date comes from; with no hour of the year it is left out, never estimated.
        /// </summary>
        internal static FormattedValue AnnualHour(IQuantityFormatter quantityFormatter, ReportValue<DateTime> time, ReportValue<int> hourOfYear)
        {
            if (!time.TryGetValue(out DateTime dateTime))
            {
                return SectionFormat.Placeholder(quantityFormatter, time);
            }

            string end = dateTime.Hour == 23 ? "24:00" : dateTime.AddHours(1).ToString("HH:mm", CultureInfo.InvariantCulture);
            string text = string.Format("{0}–{1}", dateTime.ToString("d MMM HH:mm", CultureInfo.InvariantCulture), end);
            if (hourOfYear != null && hourOfYear.TryGetValue(out int hour))
            {
                text = string.Format(CultureInfo.InvariantCulture, "{0} (HOY {1})", text, UserHourOfYear(hour));
            }

            return SectionFormat.Text(text, time);
        }

        /// <summary>
        /// The user-facing hour of the year, 1–8760 (HOY 1 = 1 January 00:00–01:00, HOY 8760 = 31 December
        /// 23:00–24:00), of a 0-based <see cref="SpaceLoadPeak.HourOfYear"/>.
        /// </summary>
        internal static int UserHourOfYear(int hourOfYear)
        {
            return hourOfYear + 1;
        }

        internal static string ComponentLabel(LoadPeakComponent loadPeakComponent)
        {
            switch (loadPeakComponent)
            {
                case LoadPeakComponent.Solar:
                    return "Solar";

                case LoadPeakComponent.Lighting:
                    return "Lighting";

                case LoadPeakComponent.OccupancySensible:
                    return "Occupancy (sensible)";

                case LoadPeakComponent.EquipmentSensible:
                    return "Equipment (sensible)";

                case LoadPeakComponent.InfiltrationVentilation:
                    return "Infiltration / ventilation";

                case LoadPeakComponent.AirMovement:
                    return "Air movement (inter-zone)";

                case LoadPeakComponent.BuildingHeatTransfer:
                    return "Building heat transfer";

                case LoadPeakComponent.ExternalConductionOpaque:
                    return "External conduction — opaque";

                case LoadPeakComponent.ExternalConductionGlazing:
                    return "External conduction — glazing";

                case LoadPeakComponent.AirHandlingUnit:
                    return "Air handling unit";

                case LoadPeakComponent.OccupancyLatent:
                    return "Occupancy (latent)";

                case LoadPeakComponent.EquipmentLatent:
                    return "Equipment (latent)";
            }

            return loadPeakComponent.ToString();
        }

        /// <summary>
        /// A latent term named by its source only ("Occupancy"), for the line that already says "Latent gains".
        /// </summary>
        internal static string LatentSourceLabel(LoadPeakComponent loadPeakComponent)
        {
            switch (loadPeakComponent)
            {
                case LoadPeakComponent.OccupancyLatent:
                    return "Occupancy";

                case LoadPeakComponent.EquipmentLatent:
                    return "Equipment";
            }

            return ComponentLabel(loadPeakComponent);
        }

        /// <summary>
        /// True when the section prints a "Peak hour" row: the results are available and at least one peak is above zero.
        /// </summary>
        internal static bool HasPeakHour(SpaceLoadResultData spaceLoadResultData)
        {
            return spaceLoadResultData != null
                && spaceLoadResultData.Status == LoadResultStatus.Available
                && (spaceLoadResultData.DesignDay?.State == LoadPeakState.Value || spaceLoadResultData.Annual?.State == LoadPeakState.Value);
        }
    }

    /// <summary>
    /// The results behind the heating and cooling sections, side by side: status, source, when they were read into
    /// the model and whether they are known to match it, then the reading notes. Only what the typed data records:
    /// currency against the model is shown as recorded, never assumed.
    /// </summary>
    internal sealed class SpaceLoadResultsSectionBuilder : ISectionBuilder<SpaceDesignLoadDocumentData>
    {
        public const string CurrencyNotRecordedText = "not recorded";

        public const string PeakHourNote = "Peak hour: a design-day peak has an hour of the day only, no date; a full-year peak shows day and hour, no year, and its hour of the year (HOY 1 = 1 Jan 00:00–01:00, HOY 8760 = 31 Dec 23:00–24:00).";

        public string Id => "results";

        public DocumentSection Build(SpaceDesignLoadDocumentData data, DocumentContext documentContext)
        {
            IQuantityFormatter quantityFormatter = documentContext.Formatter;
            SpaceLoadResultData heating = data.Heating;
            SpaceLoadResultData cooling = data.Cooling;

            // Text columns, left-aligned so they share the width rather than crowd at the right.
            TableBlock tableBlock = new TableBlock("results", null, new[]
            {
                new TableColumn(string.Empty, alignment: ColumnAlignment.Left),
                new TableColumn("Heating", alignment: ColumnAlignment.Left),
                new TableColumn("Cooling", alignment: ColumnAlignment.Left),
            }, new[]
            {
                new TableRow(SectionFormat.Label("Status"), SectionFormat.Label(StatusText(heating.Status)), SectionFormat.Label(StatusText(cooling.Status))),
                new TableRow(SectionFormat.Label("Result source"), quantityFormatter.Format(heating.ResultSource), quantityFormatter.Format(cooling.ResultSource)),
                new TableRow(SectionFormat.Label("Read into model"), quantityFormatter.Format(heating.ConvertedAt), quantityFormatter.Format(cooling.ConvertedAt)),
                new TableRow(SectionFormat.Label("Matches current model"), Currency(quantityFormatter, heating.ConvertedAt), Currency(quantityFormatter, cooling.ConvertedAt)),
            });

            List<DocumentBlock> documentBlocks = new List<DocumentBlock>() { tableBlock };

            // The peak-hour note explains a row that only a section with a peak above zero prints.
            if (SpaceLoadResultSectionBuilder.HasPeakHour(heating) || SpaceLoadResultSectionBuilder.HasPeakHour(cooling))
            {
                documentBlocks.Add(new NoticeBlock("results-time-note", PeakHourNote, NoticeLevel.Note));
            }

            documentBlocks.Add(new NoticeBlock("results-comparison-note", "Design loads (sizing) and simulated peaks are shown side by side for information; no acceptance rule is applied.", NoticeLevel.Note));

            return new DocumentSection(Id, "Results", documentBlocks);
        }

        internal static string StatusText(LoadResultStatus loadResultStatus)
        {
            switch (loadResultStatus)
            {
                case LoadResultStatus.Available:
                    return "Available";

                case LoadResultStatus.NotSimulated:
                    return "Not simulated";

                case LoadResultStatus.PeaksNotRecorded:
                    return "Peaks not recorded";

                case LoadResultStatus.Ambiguous:
                    return "Ambiguous: none chosen";
            }

            return loadResultStatus.ToString();
        }

        /// <summary>
        /// Whether the results are known to match the current model, from the freshness the data carries: "not
        /// recorded" for Unknown, never "yes".
        /// </summary>
        private static FormattedValue Currency(IQuantityFormatter quantityFormatter, ReportValue<DateTime> convertedAt)
        {
            if (!convertedAt.HasValue)
            {
                return SectionFormat.Placeholder(quantityFormatter, convertedAt);
            }

            switch (convertedAt.Freshness)
            {
                case Freshness.Current:
                    return SectionFormat.Label("Yes");

                case Freshness.OutOfDate:
                    return SectionFormat.Label("No: the model changed since the run");
            }

            return new FormattedValue(CurrencyNotRecordedText, null, Availability.NotAvailable, note: "No record ties these results to the current model");
        }
    }
}
