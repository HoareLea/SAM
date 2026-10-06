// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Reporting;
using SAM.Core;
using SAM.Core.Reporting;
using SAM.Tests.Helpers;
using SAM.Units;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using ReportingCreate = SAM.Analytical.Reporting.Create;

namespace SAM.Tests
{
    /// <summary>
    /// The Phase 1 Space Assumptions document: typed collector values, section order, missing data, design-load
    /// freshness, and deterministic golden JSON snapshots in SI and Imperial.
    /// </summary>
    public class SpaceAssumptionsTests
    {
        private static SpaceDocumentData Collect(AnalyticalModel analyticalModel, UnitStyle unitStyle, out DocumentContext documentContext)
        {
            documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options(unitStyle));
            return ReportingCreate.SpaceDocumentData(documentContext, ReportingFixture.Stored(analyticalModel));
        }

        private static Document Build(AnalyticalModel analyticalModel, UnitStyle unitStyle, out DocumentContext documentContext)
        {
            documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options(unitStyle));
            return ReportingCreate.SpaceAssumptions(documentContext, ReportingFixture.Stored(analyticalModel));
        }

        private static void AssertQuantity(ReportValue<Quantity> reportValue, double expected, UnitCategory unitCategory, ReportValueSource reportValueSource, double tolerance = 1e-9)
        {
            Assert.True(reportValue.HasValue, reportValue.Note);
            Assert.Equal(unitCategory, reportValue.Value.Category);
            Assert.Equal(reportValueSource, reportValue.Source);
            Assert.Equal(expected, reportValue.Value.Value, tolerance);
        }

        // ---------- typed collector values ----------

        [Fact]
        public void Collector_Full_ReadsTypedValuesInCanonicalUnits()
        {
            SpaceDocumentData data = Collect(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            Assert.Equal(ReportingFixture.SpaceGuid, data.Identity.Guid);
            Assert.Equal(ReportingFixture.SpaceName, data.Identity.Name.Value);
            Assert.Equal("Level 00", data.Identity.LevelName.Value);
            Assert.Equal("S39_OfficeOpen", data.Identity.InternalConditionName.Value);

            AssertQuantity(data.Geometry.Area, 30, UnitCategory.Area, ReportValueSource.SAM);
            AssertQuantity(data.Geometry.Volume, 90, UnitCategory.Volume, ReportValueSource.SAM);
            AssertQuantity(data.Geometry.AverageHeight, 3, UnitCategory.Length, ReportValueSource.Derived);

            AssertQuantity(data.Occupancy.People, 3, UnitCategory.Count, ReportValueSource.Derived);
            AssertQuantity(data.Occupancy.AreaPerPerson, 10, UnitCategory.AreaPerPerson, ReportValueSource.SAM);
            AssertQuantity(data.Occupancy.SensibleGainPerPerson, 75, UnitCategory.PowerPerPerson, ReportValueSource.SAM);
            AssertQuantity(data.Occupancy.SensibleGain, 225, UnitCategory.Power, ReportValueSource.Derived);
            AssertQuantity(data.Occupancy.LatentGain, 165, UnitCategory.Power, ReportValueSource.Derived);
            AssertQuantity(data.Occupancy.OccupiedHoursPerYear, 11 * 365, UnitCategory.Time, ReportValueSource.Derived);
            Assert.Equal("Occ 8to19", data.Occupancy.Profile.Value);

            AssertQuantity(data.Lighting.GainPerArea, 8, UnitCategory.SpecificPower, ReportValueSource.SAM);
            AssertQuantity(data.Lighting.Gain, 240, UnitCategory.Power, ReportValueSource.Derived);
            AssertQuantity(data.Lighting.Illuminance, 500, UnitCategory.Illuminance, ReportValueSource.SAM);
            AssertQuantity(data.EquipmentSensible.Gain, 750, UnitCategory.Power, ReportValueSource.Derived);
            Assert.Equal(Availability.NotApplicable, data.EquipmentSensible.Illuminance.Availability);
            Assert.Equal(Availability.NotAvailable, data.EquipmentLatent.GainPerArea.Availability);
            Assert.Equal(Availability.NotAvailable, data.EquipmentLatent.Gain.Availability);
            Assert.Equal("No equipment latent gain authored", data.EquipmentLatent.Gain.Note);

            AssertQuantity(data.Infiltration.AirChangeRate, 0.2, UnitCategory.AirChangeRate, ReportValueSource.SAM);
            AssertQuantity(data.Infiltration.AirFlow, 90 * 0.2 / 3600, UnitCategory.AirFlow, ReportValueSource.Derived);

            AssertQuantity(data.DesignCriteria.HeatingSetPoint, 21, UnitCategory.Temperature, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.CoolingSetPoint, 24, UnitCategory.Temperature, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.HumidificationSetPoint, 40, UnitCategory.Ratio, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.DehumidificationSetPoint, 60, UnitCategory.Ratio, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.OutdoorHeatingDryBulb, -3, UnitCategory.Temperature, ReportValueSource.SAM);
            AssertQuantity(data.DesignCriteria.OutdoorHeatingRelativeHumidity, 86.9, UnitCategory.Ratio, ReportValueSource.SAM);
            AssertQuantity(data.DesignCriteria.OutdoorCoolingDryBulb, 32.1, UnitCategory.Temperature, ReportValueSource.SAM);
            AssertQuantity(data.DesignCriteria.OutdoorCoolingRelativeHumidity, 35.9, UnitCategory.Ratio, ReportValueSource.SAM);

            AssertQuantity(data.Ventilation.SupplyAirFlow, 0.198, UnitCategory.AirFlow, ReportValueSource.SAM);
            AssertQuantity(data.Ventilation.ExtractAirFlow, 0.180, UnitCategory.AirFlow, ReportValueSource.SAM);
            AssertQuantity(data.Ventilation.OutsideAirFlow, 0.040, UnitCategory.AirFlow, ReportValueSource.SAM);
            AssertQuantity(data.Ventilation.SupplyAirChangeRate, 0.198 * 3600 / 90, UnitCategory.AirChangeRate, ReportValueSource.Derived);
            Assert.Equal("VAV", data.Ventilation.SystemType.Value);
            Assert.Equal("AHU1S", data.Ventilation.SupplyUnit.Value);
            Assert.Equal("AHU1E", data.Ventilation.ExtractUnit.Value);

            Assert.Equal("UFH", data.Systems.HeatingSystem.Value);
            Assert.Equal("FCU", data.Systems.CoolingSystem.Value);
            Assert.Equal("V R1", data.Systems.VentilationRiser.Value);
            Assert.False(data.Systems.CoolingRiser.HasValue);

            AssertQuantity(data.Sizing.HeatingSizingFactor, 1.2, UnitCategory.Ratio, ReportValueSource.SAM);
            AssertQuantity(data.Sizing.CoolingSizingFactor, 1.1, UnitCategory.Ratio, ReportValueSource.SAM);
            Assert.Equal("Model default", data.Sizing.CoolingSizingFactor.Note);
        }

        [Fact]
        public void Collector_Fabric_SplitsByExposure_AndSeparatesPaneAndFrame()
        {
            SpaceDocumentData data = Collect(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            Dictionary<FabricCategory, FabricAreaRow> rows = data.Fabric.Rows.ToDictionary(x => x.Category);
            Assert.Equal(Enum.GetValues(typeof(FabricCategory)).Length, rows.Count);

            // External wall 12 m² less the 2.4 m² window; internal wall 12 m² less the 1.8 m² door.
            AssertQuantity(rows[FabricCategory.Walls].ExternalArea, 9.6, UnitCategory.Area, ReportValueSource.Derived, 1e-6);
            AssertQuantity(rows[FabricCategory.Walls].InternalArea, 10.2, UnitCategory.Area, ReportValueSource.Derived, 1e-6);
            Assert.Equal(Availability.NotApplicable, rows[FabricCategory.Walls].ExternalFrameArea.Availability);

            double windowPane = rows[FabricCategory.Windows].ExternalArea.Value.Value;
            double windowFrame = rows[FabricCategory.Windows].ExternalFrameArea.Value.Value;
            Assert.Equal(2.4, windowPane + windowFrame, 6);
            Assert.Equal(0, rows[FabricCategory.Windows].InternalArea.Value.Value, 9);

            double doorPane = rows[FabricCategory.Doors].InternalArea.Value.Value;
            double doorFrame = rows[FabricCategory.Doors].InternalFrameArea.Value.Value;
            Assert.Equal(1.8, doorPane + doorFrame, 6);

            AssertQuantity(rows[FabricCategory.GroundFloors].ExternalArea, 30, UnitCategory.Area, ReportValueSource.Derived, 1e-6);
            AssertQuantity(rows[FabricCategory.RoofsAndCeilings].ExternalArea, 30, UnitCategory.Area, ReportValueSource.Derived, 1e-6);
        }

        [Fact]
        public void EquipmentLatent_AuthoredZero_StaysZero_NotAuthored_IsNotAvailable()
        {
            // SAM's CalculatedEquipmentLatentGain returns 0 W both when nothing is authored and when 0 is authored; the
            // internal condition itself still tells them apart, and the report follows the internal condition.
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            Space space = ReportingFixture.Stored(analyticalModel);
            Assert.Equal(0, Analytical.Query.CalculatedEquipmentLatentGain(space));

            InternalCondition internalCondition = space.InternalCondition;
            internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, 0.0);
            space.InternalCondition = internalCondition;
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(space);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            SpaceDocumentData data = Collect(analyticalModel, UnitStyle.SI, out _);

            AssertQuantity(data.EquipmentLatent.Gain, 0, UnitCategory.Power, ReportValueSource.Derived);
            AssertQuantity(data.EquipmentLatent.GainPerArea, 0, UnitCategory.SpecificPower, ReportValueSource.SAM);
        }

        [Fact]
        public void OccupancyGain_NotAuthored_IsNotAvailable_AuthoredZero_StaysZero()
        {
            // Query.OccupancySensibleGain / OccupancyLatentGain return 0 W when the per-person gain is not authored
            // (their TryGetValue out-parameter overwrites the NaN default), so the total must follow the internal
            // condition, like the per-person value beside it.
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            Space space = ReportingFixture.Stored(analyticalModel);
            InternalCondition internalCondition = space.InternalCondition;
            internalCondition.RemoveValue(InternalConditionParameter.OccupancyLatentGainPerPerson);
            internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, 0.0);
            space.InternalCondition = internalCondition;
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(space);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            Space space_Stored = ReportingFixture.Stored(analyticalModel);
            Assert.Equal(0, Analytical.Query.OccupancyLatentGain(space_Stored));

            SpaceDocumentData data = Collect(analyticalModel, UnitStyle.SI, out _);

            Assert.Equal(Availability.NotAvailable, data.Occupancy.LatentGainPerPerson.Availability);
            Assert.Equal(Availability.NotAvailable, data.Occupancy.LatentGain.Availability);
            Assert.Equal("No occupancy latent gain authored", data.Occupancy.LatentGain.Note);
            AssertQuantity(data.Occupancy.SensibleGain, 0, UnitCategory.Power, ReportValueSource.Derived);

            // In the document: a missing gain prints "—", an authored zero prints "0".
            TableBlock gains = Table(Build(analyticalModel, UnitStyle.SI, out _), "internal-condition", "gains");
            Assert.Equal(new[] { "0", "—" }, gains.Rows.Take(2).Select(x => x.Cells[3].Text));
            Assert.Equal(new[] { Availability.Available, Availability.NotAvailable }, gains.Rows.Take(2).Select(x => x.Cells[3].Availability));
        }

        // ---------- set points and humidity ----------

        [Fact]
        public void SetPoints_UseTheYearlyExpansion_ForProfilesMadeOfProfiles()
        {
            // Profile.MinValue takes the maximum of each child profile, so a week built from day profiles would report
            // the night set-back (28 C) as the cooling set point, and 100 % (control off) as the dehumidification limit.
            Profile day_Cooling = new Profile("Cool day", ProfileType.Cooling, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 19 ? 24.0 : 28.0));
            Profile cooling = new Profile("Cool week", ProfileType.Cooling);
            Profile day_Dehumidification = new Profile("Dehum day", ProfileType.Dehumidification, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 19 ? 60.0 : 100.0));
            Profile dehumidification = new Profile("Dehum week", ProfileType.Dehumidification);
            for (int i = 0; i < 7; i++)
            {
                cooling.Add(day_Cooling);
                dehumidification.Add(day_Dehumidification);
            }

            // The defect this test guards the report against.
            Assert.Equal(28, cooling.MinValue);

            SpaceDocumentData data = Collect(WithProfiles(ReportingFixture.Full(out _), cooling, dehumidification), UnitStyle.SI, out _);

            AssertQuantity(data.DesignCriteria.CoolingSetPoint, 24, UnitCategory.Temperature, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.DehumidificationSetPoint, 60, UnitCategory.Ratio, ReportValueSource.Derived);
        }

        [Fact]
        public void HumidityControlOff_IsNotApplicable()
        {
            // Tas convention: "No Humidification" is a 0 % lower limit, "No Dehumidification" a 100 % upper limit.
            Profile humidification = new Profile("No Humidification", ProfileType.Humidification, Enumerable.Repeat(0.0, 24));
            Profile dehumidification = new Profile("No Dehumidification", ProfileType.Dehumidification, Enumerable.Repeat(100.0, 24));

            SpaceDocumentData data = Collect(WithProfiles(ReportingFixture.Full(out _), humidification, dehumidification), UnitStyle.SI, out _);

            Assert.Equal(Availability.NotApplicable, data.DesignCriteria.HumidificationSetPoint.Availability);
            Assert.Equal(Availability.NotApplicable, data.DesignCriteria.DehumidificationSetPoint.Availability);

            KeyValueBlock keyValueBlock = Build(WithProfiles(ReportingFixture.Full(out _), humidification, dehumidification), UnitStyle.SI, out _)
                .Sections.Single(x => x.Id == "design-criteria").Blocks.OfType<KeyValueBlock>().Single(x => x.Id == "room-humidity");
            // PR2F: a control that is off is a known state, shown as "none" with its reason, not the n/a marker.
            Assert.Equal(new[] { "none", "none" }, keyValueBlock.Rows.Select(x => x.Value.Text));
            Assert.All(keyValueBlock.Rows, x => Assert.Equal(Availability.NotApplicable, x.Value.Availability));
            Assert.Equal(new[] { "No humidification (lower RH limit 0 %)", "No dehumidification (upper RH limit 100 %)" }, keyValueBlock.Rows.Select(x => x.Value.Note));
        }

        /// <summary>
        /// PR2F A1: SAM's "No Heating" (−50 °C) and "No Cooling" (150 °C) thermostats held all year are off switches, not
        /// set points, in SI and IP, and add no legend marker. A schedule that controls for even one hour still prints
        /// its set point (the heating value is the yearly maximum, the cooling one the yearly minimum).
        /// </summary>
        [Theory]
        [InlineData(UnitStyle.SI)]
        [InlineData(UnitStyle.Imperial)]
        public void ThermostatOffAllYear_IsNone_NotASetPoint(UnitStyle unitStyle)
        {
            Profile heating = new Profile("No Heating", ProfileType.Heating, Enumerable.Repeat(SAM.Analytical.Query.NoHeatingSetPoint, 24));
            Profile cooling = new Profile("No Cooling", ProfileType.Cooling, Enumerable.Repeat(SAM.Analytical.Query.NoCoolingSetPoint, 24));

            SpaceDocumentData data = Collect(WithProfiles(ReportingFixture.Full(out _), heating, cooling), unitStyle, out _);
            Assert.Equal(Availability.NotApplicable, data.DesignCriteria.HeatingSetPoint.Availability);
            Assert.Equal(Availability.NotApplicable, data.DesignCriteria.CoolingSetPoint.Availability);
            Assert.Equal("No heating (thermostat at -50 °C all year)", data.DesignCriteria.HeatingSetPoint.Note);
            Assert.Equal("No cooling (thermostat at 150 °C all year)", data.DesignCriteria.CoolingSetPoint.Note);

            Document document = Build(WithProfiles(ReportingFixture.Full(out _), heating, cooling), unitStyle, out _);
            TableRow setPoint = Table(document, "design-criteria", "design-criteria").Rows.Single(x => x.Cells[0].Text == "Room set point");
            Assert.Equal(new[] { SpaceDesignCriteriaSectionBuilder.ControlOffText, SpaceDesignCriteriaSectionBuilder.ControlOffText }, setPoint.Cells.Skip(1).Select(x => x.Text));
            Assert.All(setPoint.Cells.Skip(1), x => Assert.Null(x.Unit));
            Assert.DoesNotContain(document.FormattedValues(), x => x.Text.Contains("-50") || x.Text.Contains("150.0") || x.Text.Contains("-58") || x.Text.Contains("302"));
        }

        /// <summary>
        /// PR2F real-model regression: heating and cooling off all year, no humidity profile and no design days. The
        /// thermostats are known to be off, so Design Criteria is still a table ("none"), never the "no set point
        /// profiles" notice; with nothing known at all the notice stays.
        /// </summary>
        [Fact]
        public void DesignCriteria_ControlsOff_NothingElseKnown_KeepsTheTable()
        {
            DocumentContext documentContext = ReportingCreate.DocumentContext(ReportingFixture.Full(out _), ReportingFixture.Options(UnitStyle.SI));
            SpaceDesignCriteriaSectionBuilder sectionBuilder = new SpaceDesignCriteriaSectionBuilder();
            ReportValue<Quantity> missing = ReportValue<Quantity>.NotAvailable("No design day in model");

            SpaceDesignCriteriaData off = new SpaceDesignCriteriaData()
            {
                HeatingSetPoint = ReportValue<Quantity>.NotApplicable("No heating (thermostat at -50 °C all year)"),
                CoolingSetPoint = ReportValue<Quantity>.NotApplicable("No cooling (thermostat at 150 °C all year)"),
                HumidificationSetPoint = ReportValue<Quantity>.NotAvailable("No humidification profile"),
                DehumidificationSetPoint = ReportValue<Quantity>.NotAvailable("No dehumidification profile"),
                OutdoorHeatingDryBulb = missing,
                OutdoorHeatingRelativeHumidity = missing,
                OutdoorCoolingDryBulb = missing,
                OutdoorCoolingRelativeHumidity = missing,
            };

            DocumentSection documentSection = sectionBuilder.Build(new SpaceDocumentData() { DesignCriteria = off }, documentContext);
            Assert.DoesNotContain(documentSection.Blocks, x => x.Id == "design-criteria-missing");
            TableRow setPoint = documentSection.Blocks.OfType<TableBlock>().Single(x => x.Id == "design-criteria").Rows.Single(x => x.Cells[0].Text == "Room set point");
            Assert.Equal(new[] { "none", "none" }, setPoint.Cells.Skip(1).Select(x => x.Text));

            SpaceDesignCriteriaData unknown = new SpaceDesignCriteriaData()
            {
                HeatingSetPoint = ReportValue<Quantity>.NotAvailable("No heating profile"),
                CoolingSetPoint = ReportValue<Quantity>.NotAvailable("No cooling profile"),
                HumidificationSetPoint = off.HumidificationSetPoint,
                DehumidificationSetPoint = off.DehumidificationSetPoint,
                OutdoorHeatingDryBulb = missing,
                OutdoorHeatingRelativeHumidity = missing,
                OutdoorCoolingDryBulb = missing,
                OutdoorCoolingRelativeHumidity = missing,
            };

            documentSection = sectionBuilder.Build(new SpaceDocumentData() { DesignCriteria = unknown }, documentContext);
            Assert.Contains(documentSection.Blocks, x => x.Id == "design-criteria-missing");
        }

        [Fact]
        public void ThermostatOffForPartOfTheYear_StillPrintsTheSetPoint()
        {
            // Off overnight, controlling by day: the heating yearly maximum and cooling yearly minimum are real set points.
            Profile heating = new Profile("Heat 21 day", ProfileType.Heating, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 22 ? 21.0 : SAM.Analytical.Query.NoHeatingSetPoint));
            Profile cooling = new Profile("Cool 24 day", ProfileType.Cooling, Enumerable.Range(0, 24).Select(x => x >= 7 && x < 22 ? 24.0 : SAM.Analytical.Query.NoCoolingSetPoint));

            SpaceDocumentData data = Collect(WithProfiles(ReportingFixture.Full(out _), heating, cooling), UnitStyle.SI, out _);

            AssertQuantity(data.DesignCriteria.HeatingSetPoint, 21, UnitCategory.Temperature, ReportValueSource.Derived);
            AssertQuantity(data.DesignCriteria.CoolingSetPoint, 24, UnitCategory.Temperature, ReportValueSource.Derived);
        }

        /// <summary>
        /// PR2F batch API: a context for the next document shares the model snapshot and everything read from it, but
        /// starts an empty diagnostics log, and a document built with it is identical to one from a fresh context.
        /// </summary>
        [Fact]
        public void DocumentContext_WithNewDiagnostics_SharesTheSnapshot_StartsAnEmptyLog()
        {
            AnalyticalModel analyticalModel = ReportingFixture.Minimal(out _);
            Document first = Build(analyticalModel, UnitStyle.SI, out DocumentContext documentContext);
            Assert.NotEmpty(documentContext.Diagnostics);

            DocumentContext next = documentContext.WithNewDiagnostics();
            Assert.Same(documentContext.AnalyticalModel, next.AnalyticalModel);
            Assert.Same(documentContext.AdjacencyCluster, next.AdjacencyCluster);
            Assert.Same(documentContext.ProfileLibrary, next.ProfileLibrary);
            Assert.Same(documentContext.Options, next.Options);
            Assert.Same(documentContext.Formatter, next.Formatter);
            Assert.Same(documentContext.Provenance, next.Provenance);
            Assert.NotSame(documentContext.Diagnostics, next.Diagnostics);
            Assert.Empty(next.Diagnostics);

            int count = documentContext.Diagnostics.Count();
            Document second = ReportingCreate.SpaceAssumptions(next, ReportingFixture.Stored(analyticalModel));
            Assert.Equal(first.ToJson(), second.ToJson());
            Assert.Equal(count, next.Diagnostics.Count());
            Assert.Equal(count, documentContext.Diagnostics.Count());
        }

        [Fact]
        public void ThermostatOffLimits_AreSamsLibraryValues()
        {
            Assert.True(SAM.Analytical.Query.IsHeatingOff(-50));
            Assert.True(SAM.Analytical.Query.IsHeatingOff(-60));
            Assert.False(SAM.Analytical.Query.IsHeatingOff(-49.9));
            Assert.True(SAM.Analytical.Query.IsCoolingOff(150));
            Assert.True(SAM.Analytical.Query.IsCoolingOff(200));
            Assert.False(SAM.Analytical.Query.IsCoolingOff(149.9));
        }

        [Fact]
        public void RoomHumidity_IsLabelledByControl_NotByHeatingOrCooling()
        {
            Document document = Build(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            DocumentSection documentSection = document.Sections.Single(x => x.Id == "design-criteria");
            KeyValueBlock keyValueBlock = documentSection.Blocks.OfType<KeyValueBlock>().Single(x => x.Id == "room-humidity");
            Assert.Equal(new[] { "Humidification set point", "Dehumidification set point" }, keyValueBlock.Rows.Select(x => x.Label));
            Assert.Equal(new[] { "lower RH limit", "upper RH limit" }, keyValueBlock.Rows.Select(x => x.SubLabel));
            Assert.Equal(new[] { "40", "60" }, keyValueBlock.Rows.Select(x => x.Value.Text));

            TableBlock tableBlock = Table(document, "design-criteria", "design-criteria");
            Assert.DoesNotContain(tableBlock.Rows, x => x.Cells[0].Text.Contains("Room RH"));
        }

        /// <summary>
        /// The model with its internal condition pointing at the given cooling / humidification / dehumidification
        /// profiles, which are added to the profile library.
        /// </summary>
        private static AnalyticalModel WithProfiles(AnalyticalModel analyticalModel, params Profile[] profiles)
        {
            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;
            Space space = ReportingFixture.Stored(analyticalModel);
            InternalCondition internalCondition = space.InternalCondition;
            foreach (Profile profile in profiles)
            {
                profileLibrary.Add(profile);

                InternalConditionParameter internalConditionParameter = profile.ProfileType == ProfileType.Heating ? InternalConditionParameter.HeatingProfileName
                    : profile.ProfileType == ProfileType.Cooling ? InternalConditionParameter.CoolingProfileName
                    : profile.ProfileType == ProfileType.Humidification ? InternalConditionParameter.HumidificationProfileName
                    : InternalConditionParameter.DehumidificationProfileName;

                internalCondition.SetValue(internalConditionParameter, profile.Name);
            }

            space.InternalCondition = internalCondition;
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(space);

            return new AnalyticalModel(analyticalModel.Name, null, null, null, adjacencyCluster, null, profileLibrary);
        }

        // ---------- design-load freshness ----------

        [Fact]
        public void DesignLoads_AreFromTBD_WithUnknownFreshness()
        {
            SpaceDocumentData data = Collect(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            Assert.Equal(DesignLoadStatus.Unknown, data.Sizing.DesignLoadStatus);
            foreach (ReportValue<Quantity> reportValue in new[] { data.Sizing.DesignHeatingLoad, data.Sizing.DesignCoolingLoad, data.Sizing.DesignHeatingLoadPerArea, data.Sizing.DesignCoolingLoadPerArea })
            {
                Assert.True(reportValue.HasValue);
                Assert.Equal(Freshness.Unknown, reportValue.Freshness);
            }

            Assert.Equal(ReportValueSource.TBD, data.Sizing.DesignHeatingLoad.Source);
            AssertQuantity(data.Sizing.DesignHeatingLoad, 779, UnitCategory.Power, ReportValueSource.TBD);
            AssertQuantity(data.Sizing.DesignCoolingLoadPerArea, 12400.0 / 30, UnitCategory.SpecificPower, ReportValueSource.Derived);
        }

        [Fact]
        public void DesignLoads_StayUnknown_EvenWithCurrentTsdProvenanceAndSimulationResults()
        {
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);

            // A simulation result with a different load, and a TSD provenance stamp that is current for this model:
            // neither may leak into Phase 1 (no SpaceSimulationResult is read; TSD provenance never implies TBD
            // freshness).
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            SpaceSimulationResult spaceSimulationResult = new SpaceSimulationResult(ReportingFixture.SpaceName, "Tas", ReportingFixture.SpaceGuid.ToString());
            spaceSimulationResult.SetValue(SpaceSimulationResultParameter.Load, 99999.0);
            spaceSimulationResult.SetValue(SpaceSimulationResultParameter.LoadType, LoadType.Heating.ToString());
            adjacencyCluster.AddObject(spaceSimulationResult);
            adjacencyCluster.AddRelation(ReportingFixture.Stored(analyticalModel), spaceSimulationResult);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            string path_TSD = Path.GetTempFileName();
            try
            {
                analyticalModel.SetValue(AnalyticalModelParameter.SimulationResultProvenance, new SimulationResultProvenance(analyticalModel, path_TSD));

                SpaceDocumentData data = Collect(analyticalModel, UnitStyle.SI, out _);

                Assert.Equal(779, data.Sizing.DesignHeatingLoad.Value.Value);
                Assert.Equal(Freshness.Unknown, data.Sizing.DesignHeatingLoad.Freshness);
                Assert.Equal(Freshness.Unknown, data.Sizing.DesignCoolingLoad.Freshness);
            }
            finally
            {
                File.Delete(path_TSD);
            }
        }

        [Fact]
        public void DesignLoads_FromASavedModelWithLegacyDuplicateParameterSets_AreTheLatestPersistedValues()
        {
            // SAM#146 / Phase-2 audit B0: a saved model whose space carries one "SAM.Analytical" set per Tas run
            // (open_out.sam Bathroom_2 shape). Phase 1 printed the first run's 0 W; the latest run wrote 1139.87 W.
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            string name = SAM.Core.Query.Name(typeof(Space).Assembly);

            System.Text.Json.Nodes.JsonObject jsonObject_Model = analyticalModel.ToJsonObject();
            System.Text.Json.Nodes.JsonObject jsonObject_Space = FindSpace(jsonObject_Model, ReportingFixture.SpaceGuid);
            Assert.NotNull(jsonObject_Space);

            System.Text.Json.Nodes.JsonArray jsonArray_ParameterSets = (System.Text.Json.Nodes.JsonArray)jsonObject_Space["ParameterSets"];
            System.Text.Json.Nodes.JsonObject jsonObject_Authoring = (System.Text.Json.Nodes.JsonObject)jsonArray_ParameterSets.Single(x => (string)x["Name"] == name);
            jsonObject_Authoring["Guid"] = "990f1c57-aca9-4e09-8c0e-d5d538b111b1";
            System.Text.Json.Nodes.JsonArray jsonArray_Parameters = (System.Text.Json.Nodes.JsonArray)jsonObject_Authoring["Parameters"];
            foreach (System.Text.Json.Nodes.JsonNode jsonNode in jsonArray_Parameters.Where(x => (string)x["Name"] is "Design Heating Load" or "Design Cooling Load").ToList())
            {
                jsonArray_Parameters.Remove(jsonNode);
            }

            foreach ((string guid, double designHeatingLoad) in new[] { ("cc94e7a1-faf2-40de-b400-82b7e8a573fc", 0.0), ("feae3a10-9c31-4413-8d20-797b65029737", 1139.87451171875) })
            {
                ParameterSet parameterSet = new ParameterSet(new Guid(guid), name);
                parameterSet.Add("IsUsed", true);
                parameterSet.Add("Design Heating Load", designHeatingLoad);
                parameterSet.Add("Design Cooling Load", 0.0);
                jsonArray_ParameterSets.Add(parameterSet.ToJsonObject());
            }

            AnalyticalModel analyticalModel_Saved = SAM.Core.Create.IJSAMObject<AnalyticalModel>(jsonObject_Model.ToJsonString());

            SpaceDocumentData data = Collect(analyticalModel_Saved, UnitStyle.SI, out _);

            AssertQuantity(data.Sizing.DesignHeatingLoad, 1139.87451171875, UnitCategory.Power, ReportValueSource.TBD);
            AssertQuantity(data.Sizing.DesignCoolingLoad, 0.0, UnitCategory.Power, ReportValueSource.TBD);
            AssertQuantity(data.Sizing.DesignHeatingLoadPerArea, 1139.87451171875 / 30, UnitCategory.SpecificPower, ReportValueSource.Derived);
            Assert.Equal(DesignLoadStatus.Unknown, data.Sizing.DesignLoadStatus);
        }

        private static System.Text.Json.Nodes.JsonObject FindSpace(System.Text.Json.Nodes.JsonNode jsonNode, Guid guid)
        {
            switch (jsonNode)
            {
                case System.Text.Json.Nodes.JsonObject jsonObject:
                    if (((string)jsonObject["_type"])?.StartsWith("SAM.Analytical.Space,") == true && (string)jsonObject["Guid"] == guid.ToString())
                    {
                        return jsonObject;
                    }

                    foreach (KeyValuePair<string, System.Text.Json.Nodes.JsonNode> keyValuePair in jsonObject)
                    {
                        System.Text.Json.Nodes.JsonObject result = keyValuePair.Value == null ? null : FindSpace(keyValuePair.Value, guid);
                        if (result != null)
                        {
                            return result;
                        }
                    }

                    return null;

                case System.Text.Json.Nodes.JsonArray jsonArray:
                    foreach (System.Text.Json.Nodes.JsonNode jsonNode_Item in jsonArray)
                    {
                        System.Text.Json.Nodes.JsonObject result = jsonNode_Item == null ? null : FindSpace(jsonNode_Item, guid);
                        if (result != null)
                        {
                            return result;
                        }
                    }

                    return null;

                default:
                    return null;
            }
        }

        // ---------- missing / not applicable ----------

        [Fact]
        public void Minimal_Model_ProducesAValidDocument_WithNoticesAndWarnings()
        {
            Document document = Build(ReportingFixture.Minimal(out _), UnitStyle.SI, out DocumentContext documentContext);

            Assert.Equal(SpaceDocumentDefinitions.SpaceAssumptions.Sections.Select(x => x.Id), document.Sections.Select(x => x.Id));

            List<string> notices = document.Sections.SelectMany(x => x.Blocks).OfType<NoticeBlock>().Select(x => x.Text).ToList();
            Assert.Contains("No internal condition assigned", notices);
            Assert.Contains("No panels bound this space", notices);
            Assert.Contains("No floor area or volume in model", notices);
            Assert.Contains(SpaceSizingSectionBuilder.DesignLoadsNoneNotice, notices);
            Assert.Contains("No set point profiles and no design days in model", notices);
            Assert.Contains("No heating or cooling system assigned", notices);

            // A section with no data at all is one notice, not a block of placeholders.
            foreach (string id in new[] { "geometry", "internal-condition", "design-criteria", "ventilation", "systems", "fabric", "sizing" })
            {
                Assert.All(document.Sections.Single(x => x.Id == id).Blocks, x => Assert.IsType<NoticeBlock>(x));
            }

            Assert.Contains(ReportingCreate.LegendNotAvailable, document.Footer.Legend);
            Assert.Contains(document.Footer.Lines, x => x.Contains("Design loads: none in model"));

            List<LogRecord> warnings = documentContext.Diagnostics.Where(x => x.LogRecordType == LogRecordType.Warning).ToList();
            Assert.Contains(warnings, x => x.Text.Contains("no design loads"));
            Assert.Contains(warnings, x => x.Text.Contains("no ventilation system"));
            Assert.Contains(warnings, x => x.Text.Contains("no heating design day"));
        }

        [Fact]
        public void InvalidModelValue_BecomesNotAvailable_WithAWarning()
        {
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            Space space = ReportingFixture.Stored(analyticalModel);
            // SAM rejects a negative volume at SetValue (minimum 0), but an infinite one gets through.
            Assert.True(space.SetValue(SpaceParameter.Volume, double.PositiveInfinity));
            adjacencyCluster.AddObject(space);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            SpaceDocumentData data = Collect(analyticalModel, UnitStyle.SI, out DocumentContext documentContext);

            Assert.Equal(Availability.NotAvailable, data.Geometry.Volume.Availability);
            Assert.Equal("invalid value in model", data.Geometry.Volume.Note);
            Assert.Contains(documentContext.Diagnostics, x => x.LogRecordType == LogRecordType.Warning && x.Text.Contains("invalid value in model"));
        }

        [Fact]
        public void SoftwareFailure_Propagates_AndProducesNoDocument()
        {
            DocumentDefinition<SpaceDocumentData> documentDefinition = new DocumentDefinition<SpaceDocumentData>(
                "faulty", "Faulty", DocumentScope.Space, new ISectionBuilder<SpaceDocumentData>[] { new SpaceIdentitySectionBuilder(), new ThrowingSectionBuilder() }, null, null);

            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options());
            SpaceDocumentData data = ReportingCreate.SpaceDocumentData(documentContext, ReportingFixture.Stored(analyticalModel));

            Assert.Throws<NotImplementedException>(() => ReportingCreate.Document(documentDefinition, data, documentContext));
        }

        private sealed class ThrowingSectionBuilder : ISectionBuilder<SpaceDocumentData>
        {
            public string Id => "throws";

            public DocumentSection Build(SpaceDocumentData data, DocumentContext documentContext) => throw new NotImplementedException("injected fault");
        }

        // ---------- sections and formatting ----------

        [Fact]
        public void Sections_AreInTheApprovedOrder()
        {
            Document document = Build(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            Assert.Equal(new[] { "identity", "geometry", "design-criteria", "internal-condition", "ventilation", "systems", "fabric", "sizing" }, document.Sections.Select(x => x.Id));

            // The v2 layout hints: consecutive half-width sections are paired (Geometry | Design criteria,
            // Ventilation | Systems, Fabric | Sizing); the internal condition is full width with the gains table
            // first, then its three titled key/value blocks.
            Assert.Equal(new[] { SectionWidth.Full, SectionWidth.Half, SectionWidth.Half, SectionWidth.Full, SectionWidth.Half, SectionWidth.Half, SectionWidth.Half, SectionWidth.Half }, document.Sections.Select(x => x.Width));
            Assert.Equal(new[] { "gains", "occupancy", "lighting", "infiltration" }, document.Sections.Single(x => x.Id == "internal-condition").Blocks.Select(x => x.Id));

            Assert.Equal("Space Assumptions", document.Metadata.Title);
            Assert.Equal(ReportingFixture.SpaceName, document.Metadata.Subject);
            Assert.Equal("Test Project", document.Metadata.ProjectName);
        }

        [Fact]
        public void Sizing_HeatingCoolingPair_SharesOneUnit_SI()
        {
            TableBlock tableBlock = Table(Build(ReportingFixture.Full(out _), UnitStyle.SI, out _), "sizing", "sizing");

            TableRow load = tableBlock.Rows[0];
            Assert.Equal("0.78", load.Cells[1].Text);
            Assert.Equal("12.40", load.Cells[2].Text);
            Assert.All(load.Cells.Skip(1), x => Assert.Equal("kW", x.Unit));
            Assert.All(load.Cells.Skip(1), x => Assert.Equal(Freshness.Unknown, x.Freshness));

            // The stored factor multiplies the design load: 1.2 is shown as the multiplier 1.20, not as a percentage.
            TableRow multiplier = tableBlock.Rows[2];
            Assert.Equal("Sizing multiplier", multiplier.Cells[0].Text);
            Assert.Equal(new[] { "1.20", "1.10" }, multiplier.Cells.Skip(1).Select(x => x.Text));
            Assert.All(multiplier.Cells.Skip(1), x => Assert.Null(x.Unit));
        }

        [Fact]
        public void Sizing_HasOneNotice_SayingFreshnessAndMultiplierInclusionAreNotRecorded()
        {
            DocumentSection documentSection = Build(ReportingFixture.Full(out _), UnitStyle.SI, out _).Sections.Single(x => x.Id == "sizing");

            NoticeBlock noticeBlock = Assert.Single(documentSection.Blocks.OfType<NoticeBlock>());
            Assert.Equal(SpaceSizingSectionBuilder.DesignLoadsUnknownSizingMultiplierNotice, noticeBlock.Text);
            Assert.Contains("date/currency not recorded", noticeBlock.Text);
            Assert.Contains("Sizing multiplier", noticeBlock.Text);
        }

        [Fact]
        public void SizingMultiplier_SetNowhere_IsNotSet_NotMissing()
        {
            // SAM_Tas Modify.UpdateSizingFactors: a space factor of 0 / unset falls back to the model factor, and with
            // neither set the zone design load is left unscaled - a known "no multiplier", not unknown data.
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            Space space = ReportingFixture.Stored(analyticalModel);
            Assert.True(space.SetValue(SpaceParameter.HeatingSizingFactor, 0.0));
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(space);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);
            analyticalModel.RemoveValue(AnalyticalModelParameter.CoolingSizingFactor);

            SpaceDocumentData data = Collect(analyticalModel, UnitStyle.SI, out _);
            Assert.Equal(Availability.NotApplicable, data.Sizing.HeatingSizingFactor.Availability);
            Assert.Equal(ReportingCreate.SizingFactorNotSet, data.Sizing.HeatingSizingFactor.Note);
            Assert.Equal(Availability.NotApplicable, data.Sizing.CoolingSizingFactor.Availability);

            Document document = Build(analyticalModel, UnitStyle.SI, out _);
            TableRow multiplier = Table(document, "sizing", "sizing").Rows[2];
            Assert.Equal(new[] { SpaceSizingSectionBuilder.SizingMultiplierNotSetText, SpaceSizingSectionBuilder.SizingMultiplierNotSetText }, multiplier.Cells.Skip(1).Select(x => x.Text));

            // No multiplier is applied, so the notice does not raise the inclusion question, and the design loads
            // keep their Unknown freshness.
            NoticeBlock noticeBlock = Assert.Single(document.Sections.Single(x => x.Id == "sizing").Blocks.OfType<NoticeBlock>());
            Assert.Equal(SpaceSizingSectionBuilder.DesignLoadsUnknownNotice, noticeBlock.Text);
            Assert.Equal(Freshness.Unknown, Table(document, "sizing", "sizing").Rows[0].Cells[1].Freshness);
        }

        [Fact]
        public void SizingMultiplier_Invalid_StaysNotAvailable()
        {
            // An invalid stored factor is unknown data: "—", never "not set", and it does not fall back to the model.
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            Space space = ReportingFixture.Stored(analyticalModel);
            Assert.True(space.SetValue(SpaceParameter.HeatingSizingFactor, double.PositiveInfinity));
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            adjacencyCluster.AddObject(space);
            analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster);

            TableRow multiplier = Table(Build(analyticalModel, UnitStyle.SI, out _), "sizing", "sizing").Rows[2];
            Assert.Equal(new[] { "—", "1.10" }, multiplier.Cells.Skip(1).Select(x => x.Text));
            Assert.Equal(Availability.NotAvailable, multiplier.Cells[1].Availability);
        }

        [Fact]
        public void Occupancy_ProfileIsShownOnce_InTheGainsTable()
        {
            Document document = Build(ReportingFixture.Full(out _), UnitStyle.SI, out _);

            KeyValueBlock occupancy = document.Sections.Single(x => x.Id == "internal-condition").Blocks.OfType<KeyValueBlock>().Single(x => x.Id == "occupancy");
            Assert.DoesNotContain(occupancy.Rows, x => x.Label == "Profile");
            Assert.Equal(new[] { "People", "Area per person", "Occupied hours per year" }, occupancy.Rows.Select(x => x.Label));

            TableBlock gains = Table(document, "internal-condition", "gains");
            Assert.Equal(new[] { "Occ 8to19", "Occ 8to19" }, gains.Rows.Take(2).Select(x => x.Cells[1].Text));
        }

        [Theory]
        [InlineData(UnitStyle.SI, "m²")]
        [InlineData(UnitStyle.Imperial, "ft²")]
        public void Fabric_ZeroAreaCategories_AreNamedInANote_NotListed(UnitStyle unitStyle, string areaUnit)
        {
            // The fixture has no intermediate floor ("Other floors" is zero on both sides), and its window and door
            // apertures have no frame.
            DocumentSection documentSection = Build(ReportingFixture.Full(out _), unitStyle, out _).Sections.Single(x => x.Id == "fabric");

            TableBlock tableBlock = documentSection.Blocks.OfType<TableBlock>().Single();
            List<string> labels = tableBlock.Rows.Select(x => x.Cells[0].Text).ToList();
            Assert.Equal(new[] { "Walls", "Windows (pane)", "Doors (pane)", "Roofs / ceilings", "Ground floors" }, labels);

            // Windows are internal-zero but external-present, so the row stays with its explicit zero.
            Assert.Matches(@"^0(\.0+)?$", tableBlock.Rows[1].Cells[2].Text);

            NoticeBlock noticeBlock = documentSection.Blocks.OfType<NoticeBlock>().Single(x => x.Id == "fabric-not-present");
            Assert.Equal(SpaceFabricSectionBuilder.NotPresentPrefix + "Windows (frame), Doors (frame), Other floors", noticeBlock.Text);
            Assert.Equal(NoticeLevel.Note, noticeBlock.Level);

            // The selected area unit is kept, and the note names no unit.
            Assert.All(tableBlock.Columns.Skip(1), x => Assert.Equal(areaUnit, x.Unit));
            Assert.DoesNotContain("m²", noticeBlock.Text);
            Assert.DoesNotContain("ft²", noticeBlock.Text);
        }

        [Fact]
        public void Fabric_WholeZeroOpening_IsNamedOnce_AndMissingAreaIsNotZero()
        {
            DocumentContext documentContext = ReportingCreate.DocumentContext(ReportingFixture.Full(out _), ReportingFixture.Options());
            ReportValue<Quantity> zero = ReportValue<Quantity>.Available(new Quantity(0, UnitType.SquareMeter), ReportValueSource.Derived);
            ReportValue<Quantity> ten = ReportValue<Quantity>.Available(new Quantity(10, UnitType.SquareMeter), ReportValueSource.Derived);
            ReportValue<Quantity> missing = ReportValue<Quantity>.NotAvailable("not computed");
            ReportValue<Quantity> opaque = ReportValue<Quantity>.NotApplicable("Opaque element");

            SpaceDocumentData data = new SpaceDocumentData()
            {
                Fabric = new SpaceFabricData()
                {
                    Rows = new List<FabricAreaRow>()
                    {
                        new FabricAreaRow() { Category = FabricCategory.Walls, ExternalArea = ten, InternalArea = zero, ExternalFrameArea = opaque, InternalFrameArea = opaque },
                        new FabricAreaRow() { Category = FabricCategory.Windows, ExternalArea = zero, InternalArea = zero, ExternalFrameArea = zero, InternalFrameArea = zero },
                        new FabricAreaRow() { Category = FabricCategory.GroundFloors, ExternalArea = missing, InternalArea = zero, ExternalFrameArea = opaque, InternalFrameArea = opaque },
                    }.AsReadOnly(),
                },
            };

            DocumentSection documentSection = new SpaceFabricSectionBuilder().Build(data, documentContext);

            TableBlock tableBlock = documentSection.Blocks.OfType<TableBlock>().Single();
            Assert.Equal(new[] { "Walls", "Ground floors" }, tableBlock.Rows.Select(x => x.Cells[0].Text));
            Assert.Equal("—", tableBlock.Rows[1].Cells[1].Text);
            Assert.Equal(SpaceFabricSectionBuilder.NotPresentPrefix + "Windows", documentSection.Blocks.OfType<NoticeBlock>().Single().Text);
        }

        [Fact]
        public void Imperial_Document_LeaksNoSIUnit()
        {
            string json = Build(ReportingFixture.Full(out _), UnitStyle.Imperial, out _).ToJson();

            foreach (string symbol in new[] { "m²", "m³", "°C", "l/s", "W/m²", "W/person", "\"W\"", "\"kW\"", "lux" })
            {
                Assert.DoesNotContain(symbol, json);
            }
        }

        [Fact]
        public void Imperial_Document_ConvertsEveryCategory()
        {
            Document document = Build(ReportingFixture.Full(out _), UnitStyle.Imperial, out _);

            KeyValueBlock geometry = document.Sections.Single(x => x.Id == "geometry").Blocks.OfType<KeyValueBlock>().Single();
            Assert.Equal("323", geometry.Rows[0].Value.Text);
            Assert.Equal("ft²", geometry.Rows[0].Value.Unit);

            KeyValueBlock ventilation = document.Sections.Single(x => x.Id == "ventilation").Blocks.OfType<KeyValueBlock>().Single();
            Assert.Equal("420", ventilation.Rows[0].Value.Text);
            Assert.All(ventilation.Rows.Take(3), x => Assert.Equal("cfm", x.Value.Unit));

            TableBlock designCriteria = Table(document, "design-criteria", "design-criteria");
            Assert.Equal("69.8", designCriteria.Rows[0].Cells[1].Text);
            Assert.Equal("°F", designCriteria.Rows[0].Cells[1].Unit);

            TableBlock sizing = Table(document, "sizing", "sizing");
            Assert.Equal("Btu/h", sizing.Rows[0].Cells[1].Unit);
            Assert.Equal("Btu/h", sizing.Rows[0].Cells[2].Unit);
            Assert.Equal("Btu/h·ft²", sizing.Rows[1].Cells[1].Unit);
        }

        [Fact]
        public void Gains_TotalColumn_SharesOneUnit()
        {
            TableBlock tableBlock = Table(Build(ReportingFixture.Full(out _), UnitStyle.SI, out _), "internal-condition", "gains");

            Assert.Equal("W", tableBlock.Columns[3].Unit);
            Assert.Equal(new[] { "225", "165", "240", "750", "—" }, tableBlock.Rows.Select(x => x.Cells[3].Text));
            Assert.Equal("W/person", tableBlock.Rows[0].Cells[2].Unit);
            Assert.Equal("W/m²", tableBlock.Rows[2].Cells[2].Unit);
        }

        private static TableBlock Table(Document document, string sectionId, string blockId)
        {
            return document.Sections.Single(x => x.Id == sectionId).Blocks.OfType<TableBlock>().Single(x => x.Id == blockId);
        }

        // ---------- golden snapshots ----------

        [Theory]
        [InlineData("SpaceAssumptions_Full_SI.json", false, UnitStyle.SI)]
        [InlineData("SpaceAssumptions_Full_IP.json", false, UnitStyle.Imperial)]
        [InlineData("SpaceAssumptions_Minimal_SI.json", true, UnitStyle.SI)]
        public void Snapshot_MatchesGolden(string fileName, bool minimal, UnitStyle unitStyle)
        {
            string json = Build(minimal ? ReportingFixture.Minimal(out _) : ReportingFixture.Full(out _), unitStyle, out _).ToJson();

            // Deterministic: a second build of the same input gives the same bytes.
            Assert.Equal(json, Build(minimal ? ReportingFixture.Minimal(out _) : ReportingFixture.Full(out _), unitStyle, out _).ToJson());

            Golden.AssertMatches(fileName, json);
        }
    }
}
