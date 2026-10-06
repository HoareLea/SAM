// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Reporting;
using SAM.Core;
using SAM.Core.Reporting;
using SAM.Geometry.Spatial;
using SAM.Tests.Helpers;
using SAM.Units;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using ReportingCreate = SAM.Analytical.Reporting.Create;

namespace SAM.Tests
{
    /// <summary>
    /// Phase-2 PR2B: the typed Space Design Load data read from the PR2A result contract
    /// (<see cref="SpaceSimulationResultParameter.DesignDayPeak"/> / <see cref="SpaceSimulationResultParameter.AnnualPeak"/>).
    /// The Bathroom_2 values are its real TSD peaks (C:\TasOut\final1b\open.tsd, Leeds TRY), already converted to the
    /// SAM contract; the fixtures are typed so no Tas file or SAM_Tas code is involved.
    /// </summary>
    public class SpaceDesignLoadDataTests
    {
        private const string SpaceName = "Bathroom_2";

        // ---------- fixtures ----------

        private static SpaceLoadPeak BathroomHeatingDesignDay()
        {
            SpaceLoadPeak result = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 1139.79614)
            {
                DesignDayName = "Leeds_TRY ANN HTG 100% CONDS DB",
                HourOfDay = 23,
                DryBulbTemperature = 16.0,
                ResultantTemperature = 13.8705444,
                RelativeHumidity = 19.6374722,
                HumidityRatio = 0.00220824825,
            };

            result.SetComponent(LoadPeakComponent.InfiltrationVentilation, -111.737091);
            result.SetComponent(LoadPeakComponent.BuildingHeatTransfer, -1023.25586);
            result.SetComponent(LoadPeakComponent.ExternalConductionOpaque, -4.80297852);
            result.SetComponent(LoadPeakComponent.ExternalConductionGlazing, 0.0);
            return result;
        }

        private static SpaceLoadPeak BathroomHeatingAnnual()
        {
            SpaceLoadPeak result = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 104.009911)
            {
                HourOfYear = 8553,
                HourOfDay = 9,
                DryBulbTemperature = 16.0009079,
                RelativeHumidity = 34.96767,
                OutdoorDryBulbTemperature = -2.3,
                OutdoorRelativeHumidity = 100,
            };

            result.SetComponent(LoadPeakComponent.InfiltrationVentilation, -93.37398);
            result.SetComponent(LoadPeakComponent.BuildingHeatTransfer, -3.15779853);
            result.SetComponent(LoadPeakComponent.ExternalConductionOpaque, -7.47790527);
            return result;
        }

        /// <summary>Bedroom 2_3's real annual cooling peak (pr3\final\bridge.tsd): positive gains, latent kept apart.</summary>
        private static SpaceLoadPeak CoolingAnnual()
        {
            SpaceLoadPeak result = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 1369.404)
            {
                HourOfYear = 5116,
                HourOfDay = 4,
                DryBulbTemperature = 17.2,
                RelativeHumidity = 100,
                OutdoorDryBulbTemperature = 15.2,
                OutdoorRelativeHumidity = 94,
            };

            result.SetComponent(LoadPeakComponent.Solar, 12.5);
            result.SetComponent(LoadPeakComponent.OccupancySensible, 140.0);
            result.SetComponent(LoadPeakComponent.BuildingHeatTransfer, 1250.0);
            result.SetComponent(LoadPeakComponent.InfiltrationVentilation, -33.1);
            result.SetComponent(LoadPeakComponent.OccupancyLatent, 120.0);
            result.SetComponent(LoadPeakComponent.EquipmentLatent, 45.5);
            return result;
        }

        private static SpaceSimulationResult Result(LoadType loadType, SpaceLoadPeak designDay, SpaceLoadPeak annual, string source = "Tas")
        {
            SpaceSimulationResult result = new SpaceSimulationResult(SpaceName, source, "zone-guid");
            result.SetValue(SpaceSimulationResultParameter.LoadType, loadType.Text());
            if (designDay != null)
            {
                result.SetValue(SpaceSimulationResultParameter.DesignDayPeak, designDay);
            }

            if (annual != null)
            {
                result.SetValue(SpaceSimulationResultParameter.AnnualPeak, annual);
            }

            return result;
        }

        private static AnalyticalModel Model(params SpaceSimulationResult[] spaceSimulationResults)
        {
            Space space = new Space(ReportingFixture.SpaceGuid, SpaceName, new Point3D(0, 0, 0));
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            foreach (SpaceSimulationResult spaceSimulationResult in spaceSimulationResults)
            {
                adjacencyCluster.AddObject(spaceSimulationResult);
                adjacencyCluster.AddRelation(space, spaceSimulationResult);
            }

            return new AnalyticalModel("Model", null, null, null, adjacencyCluster);
        }

        private static SpaceDesignLoadDocumentData Collect(AnalyticalModel analyticalModel, out DocumentContext documentContext, string resultSource = null)
        {
            documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options());
            return ReportingCreate.SpaceDesignLoadDocumentData(documentContext, ReportingFixture.Stored(analyticalModel), resultSource);
        }

        private static SpaceDesignLoadDocumentData Collect(AnalyticalModel analyticalModel)
        {
            return Collect(analyticalModel, out _);
        }

        private static void AssertQuantity(ReportValue<Quantity> reportValue, double expected, UnitType unitType, double tolerance = 1e-9)
        {
            Assert.True(reportValue.HasValue, reportValue.Note);
            Assert.Equal(ReportValueSource.SimulationResult, reportValue.Source);
            Assert.Equal(Freshness.Unknown, reportValue.Freshness);
            Assert.Equal(unitType, reportValue.Value.Unit);
            Assert.Equal(expected, reportValue.Value.Value, tolerance);
        }

        private static double Component(SpaceLoadPeakData spaceLoadPeakData, LoadPeakComponent loadPeakComponent)
        {
            return spaceLoadPeakData.SensibleComponents.Concat(spaceLoadPeakData.LatentComponents).Single(x => x.Component == loadPeakComponent).Value.Value.Value;
        }

        // ---------- 1. unavailable ----------

        [Fact]
        public void MissingPeak_StaysUnavailable_NeverZero()
        {
            SpaceDesignLoadDocumentData data = Collect(Model(Result(LoadType.Heating, BathroomHeatingDesignDay(), null)), out DocumentContext documentContext);

            Assert.Equal(LoadResultStatus.Available, data.Heating.Status);
            Assert.Equal(LoadPeakState.Value, data.Heating.DesignDay.State);

            SpaceLoadPeakData annual = data.Heating.Annual;
            Assert.Equal(LoadPeakBasis.AnnualSimulation, annual.Basis);
            Assert.Equal(LoadPeakState.Unavailable, annual.State);
            Assert.Equal(Availability.NotAvailable, annual.Load.Availability);
            Assert.Equal("No annual heating peak in the results", annual.Load.Note);
            Assert.False(annual.Load.TryGetValue(out _));
            Assert.Equal(Availability.NotAvailable, annual.Time.Availability);
            Assert.Empty(annual.SensibleComponents);
            Assert.Empty(annual.LatentComponents);

            //No cooling result at all: not simulated, both peaks unavailable.
            Assert.Equal(LoadResultStatus.NotSimulated, data.Cooling.Status);
            Assert.Equal(LoadPeakState.Unavailable, data.Cooling.DesignDay.State);
            Assert.Equal(LoadPeakState.Unavailable, data.Cooling.Annual.State);
            Assert.Equal(Availability.NotAvailable, data.Cooling.DesignDay.Load.Availability);

            Assert.Contains(documentContext.Diagnostics, x => x.Text.Contains("no annual heating peak in the results"));
            Assert.Contains(documentContext.Diagnostics, x => x.Text.Contains("no cooling results"));
        }

        [Fact]
        public void LegacyOnlyResult_IsPeaksNotRecorded_AndItsLegacyValuesAreNotRead()
        {
            SpaceSimulationResult legacy = Result(LoadType.Heating, null, null);
            legacy.SetValue(SpaceSimulationResultParameter.Load, 1139.8);
            legacy.SetValue(SpaceSimulationResultParameter.LoadIndex, 1608);
            legacy.SetValue(SpaceSimulationResultParameter.SizingMethod, "HDD");
            legacy.SetValue(SpaceSimulationResultParameter.DryBulbTempearture, -1.0);

            SpaceDesignLoadDocumentData data = Collect(Model(legacy));

            Assert.Equal(LoadResultStatus.PeaksNotRecorded, data.Heating.Status);
            Assert.Equal(LoadPeakState.Unavailable, data.Heating.DesignDay.State);
            Assert.Equal(LoadPeakState.Unavailable, data.Heating.Annual.State);
            Assert.False(data.Heating.DesignDay.Load.HasValue);
            Assert.False(data.Heating.DesignDay.RoomDryBulbTemperature.HasValue);
            Assert.Contains("re-run", data.Heating.DesignDay.Load.Note);
        }

        [Fact]
        public void TypedPeaks_WinOverConflictingLegacyValues()
        {
            SpaceSimulationResult result = Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual());
            result.SetValue(SpaceSimulationResultParameter.Load, -1.0);
            result.SetValue(SpaceSimulationResultParameter.LoadIndex, 0);

            SpaceDesignLoadDocumentData data = Collect(Model(result));

            AssertQuantity(data.Heating.DesignDay.Load, 1139.79614, UnitType.Watt);
            AssertQuantity(data.Heating.Annual.Load, 104.009911, UnitType.Watt);
        }

        // ---------- 2. real zero ----------

        [Fact]
        public void ZeroPeak_IsAValidZero_NotUnavailable()
        {
            SpaceSimulationResult cooling = Result(LoadType.Cooling, new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0) { DesignDayName = "CDD" }, new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 0));

            SpaceDesignLoadDocumentData data = Collect(Model(cooling));

            Assert.Equal(LoadResultStatus.Available, data.Cooling.Status);
            foreach (SpaceLoadPeakData peak in new[] { data.Cooling.DesignDay, data.Cooling.Annual })
            {
                Assert.Equal(LoadPeakState.Zero, peak.State);
                AssertQuantity(peak.Load, 0.0, UnitType.Watt);

                //No peak timestep: nothing to report, which is not the same as missing.
                Assert.Equal(Availability.NotApplicable, peak.HourOfDay.Availability);
                Assert.Equal(Availability.NotApplicable, peak.RoomDryBulbTemperature.Availability);
                Assert.Equal(Availability.NotApplicable, peak.OutdoorDryBulbTemperature.Availability);
                Assert.Equal(Availability.NotApplicable, peak.Time.Availability);
                Assert.Empty(peak.SensibleComponents);
            }

            Assert.Equal("CDD", data.Cooling.DesignDay.DesignDayName.Value);
        }

        // ---------- 3. design day ----------

        [Fact]
        public void DesignDayPeak_KeepsItsNameAndHour_AndGetsNoCalendarDate()
        {
            SpaceLoadPeakData designDay = Collect(Model(Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual()))).Heating.DesignDay;

            Assert.Equal(LoadPeakBasis.DesignDay, designDay.Basis);
            Assert.Equal("Leeds_TRY ANN HTG 100% CONDS DB", designDay.DesignDayName.Value);
            Assert.Equal(23, designDay.HourOfDay.Value);
            Assert.Equal(Availability.NotApplicable, designDay.HourOfYear.Availability);
            Assert.Equal(Availability.NotApplicable, designDay.Time.Availability);
            Assert.Equal("Design day: no calendar date", designDay.Time.Note);

            //Tas design-day data sets carry no weather results: the outdoor state is not reported, not invented.
            Assert.Equal(Availability.NotAvailable, designDay.OutdoorDryBulbTemperature.Availability);
            Assert.Equal(Availability.NotAvailable, designDay.OutdoorRelativeHumidity.Availability);

            AssertQuantity(designDay.RoomDryBulbTemperature, 16.0, UnitType.Celsius);
            AssertQuantity(designDay.RoomResultantTemperature, 13.8705444, UnitType.Celsius);
            AssertQuantity(designDay.RoomRelativeHumidity, 19.6374722, UnitType.Percent);
            AssertQuantity(designDay.RoomHumidityRatio, 0.00220824825, UnitType.KilogramPerKilogram);
        }

        [Fact]
        public void DesignDayPeak_WithAStrayHourOfYear_StillGetsNoCalendarDate()
        {
            SpaceLoadPeak stray = BathroomHeatingDesignDay();
            stray.HourOfYear = 1607;

            SpaceLoadPeakData designDay = Collect(Model(Result(LoadType.Heating, stray, null))).Heating.DesignDay;

            Assert.Equal(Availability.NotApplicable, designDay.HourOfYear.Availability);
            Assert.Equal(Availability.NotApplicable, designDay.Time.Availability);
        }

        // ---------- 4. annual ----------

        [Fact]
        public void AnnualPeak_KeepsItsNormalizedTime_AndOutdoorState()
        {
            SpaceLoadPeakData annual = Collect(Model(Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual()))).Heating.Annual;

            Assert.Equal(LoadPeakBasis.AnnualSimulation, annual.Basis);
            Assert.Equal(8553, annual.HourOfYear.Value);
            Assert.Equal(9, annual.HourOfDay.Value);
            Assert.Equal(new DateTime(ReportingCreate.ReferenceYear, 12, 23, 9, 0, 0), annual.Time.Value);
            Assert.Equal(Availability.NotApplicable, annual.DesignDayName.Availability);

            AssertQuantity(annual.OutdoorDryBulbTemperature, -2.3, UnitType.Celsius);
            AssertQuantity(annual.OutdoorRelativeHumidity, 100, UnitType.Percent);

            //Resultant and humidity ratio were not reported at this peak: unavailable, not zero.
            Assert.Equal(Availability.NotAvailable, annual.RoomResultantTemperature.Availability);
            Assert.Equal("Not reported by the simulation", annual.RoomResultantTemperature.Note);
            Assert.Equal(Availability.NotAvailable, annual.RoomHumidityRatio.Availability);
        }

        [Fact]
        public void AnnualPeak_OnTheFirstHourOfTheYear_IsHourZero_NotMissing()
        {
            SpaceLoadPeak first = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 50) { HourOfYear = 0, HourOfDay = 0 };

            SpaceLoadPeakData annual = Collect(Model(Result(LoadType.Heating, null, first))).Heating.Annual;

            Assert.Equal(0, annual.HourOfYear.Value);
            Assert.Equal(0, annual.HourOfDay.Value);
            Assert.Equal(new DateTime(ReportingCreate.ReferenceYear, 1, 1, 0, 0, 0), annual.Time.Value);
        }

        // ---------- 5. independent peaks ----------

        [Fact]
        public void DesignDayAndAnnualPeaks_StaySeparate_WhicheverIsLarger()
        {
            SpaceLoadPeak smallDesignDay = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 50) { DesignDayName = "HDD", HourOfDay = 6 };
            SpaceLoadPeak largeAnnual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 5000) { HourOfYear = 100, HourOfDay = 4 };

            SpaceDesignLoadDocumentData data = Collect(Model(Result(LoadType.Heating, smallDesignDay, largeAnnual)));

            AssertQuantity(data.Heating.DesignDay.Load, 50, UnitType.Watt);
            Assert.Equal(6, data.Heating.DesignDay.HourOfDay.Value);
            AssertQuantity(data.Heating.Annual.Load, 5000, UnitType.Watt);
            Assert.Equal(4, data.Heating.Annual.HourOfDay.Value);

            //And the other way round (Bathroom_2: the design day is ~11x the annual peak).
            data = Collect(Model(Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual())));
            AssertQuantity(data.Heating.DesignDay.Load, 1139.79614, UnitType.Watt);
            AssertQuantity(data.Heating.Annual.Load, 104.009911, UnitType.Watt);
        }

        [Fact]
        public void PeakInTheWrongSlot_IsUnavailable_NotReinterpreted()
        {
            SpaceSimulationResult result = Result(LoadType.Heating, BathroomHeatingAnnual(), null);

            SpaceDesignLoadDocumentData data = Collect(Model(result), out DocumentContext documentContext);

            Assert.Equal(LoadPeakState.Unavailable, data.Heating.DesignDay.State);
            Assert.Equal("invalid value in results", data.Heating.DesignDay.Load.Note);
            Assert.Contains(documentContext.Diagnostics, x => x.Text.Contains("recorded as AnnualSimulation"));
        }

        // ---------- 6. independent heating / cooling ----------

        [Fact]
        public void HeatingAndCooling_DoNotLeakIntoEachOther()
        {
            SpaceSimulationResult heating = Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual());
            SpaceSimulationResult cooling = Result(LoadType.Cooling, new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0), CoolingAnnual());

            SpaceDesignLoadDocumentData data = Collect(Model(heating, cooling));

            Assert.Equal(LoadType.Heating, data.Heating.LoadType);
            Assert.Equal(LoadType.Cooling, data.Cooling.LoadType);

            AssertQuantity(data.Heating.DesignDay.Load, 1139.79614, UnitType.Watt);
            AssertQuantity(data.Heating.Annual.Load, 104.009911, UnitType.Watt);
            Assert.Equal(8553, data.Heating.Annual.HourOfYear.Value);

            Assert.Equal(LoadPeakState.Zero, data.Cooling.DesignDay.State);
            AssertQuantity(data.Cooling.Annual.Load, 1369.404, UnitType.Watt);
            Assert.Equal(5116, data.Cooling.Annual.HourOfYear.Value);
            Assert.Equal(new DateTime(ReportingCreate.ReferenceYear, 8, 2, 4, 0, 0), data.Cooling.Annual.Time.Value);

            //Heating only: its cooling stays not simulated.
            SpaceDesignLoadDocumentData heatingOnly = Collect(Model(heating));
            Assert.Equal(LoadResultStatus.NotSimulated, heatingOnly.Cooling.Status);
            Assert.Equal(LoadPeakState.Unavailable, heatingOnly.Cooling.Annual.State);
        }

        // ---------- 7. signed components ----------

        [Fact]
        public void Components_KeepTheirStoredSign_AndLatentStaysApart()
        {
            SpaceSimulationResult heating = Result(LoadType.Heating, BathroomHeatingDesignDay(), null);
            SpaceSimulationResult cooling = Result(LoadType.Cooling, null, CoolingAnnual());

            SpaceDesignLoadDocumentData data = Collect(Model(heating, cooling));

            SpaceLoadPeakData heatingPeak = data.Heating.DesignDay;
            Assert.Equal(-111.737091, Component(heatingPeak, LoadPeakComponent.InfiltrationVentilation), 9);
            Assert.Equal(-1023.25586, Component(heatingPeak, LoadPeakComponent.BuildingHeatTransfer), 9);
            Assert.Equal(-4.80297852, Component(heatingPeak, LoadPeakComponent.ExternalConductionOpaque), 9);
            Assert.Equal(0.0, Component(heatingPeak, LoadPeakComponent.ExternalConductionGlazing), 9);
            Assert.Empty(heatingPeak.LatentComponents);

            //Only the stored terms: nothing added, no net balance.
            Assert.Equal(new[] { LoadPeakComponent.InfiltrationVentilation, LoadPeakComponent.BuildingHeatTransfer, LoadPeakComponent.ExternalConductionOpaque, LoadPeakComponent.ExternalConductionGlazing },
                heatingPeak.SensibleComponents.Select(x => x.Component));

            SpaceLoadPeakData coolingPeak = data.Cooling.Annual;
            Assert.Equal(12.5, Component(coolingPeak, LoadPeakComponent.Solar), 9);
            Assert.Equal(1250.0, Component(coolingPeak, LoadPeakComponent.BuildingHeatTransfer), 9);
            Assert.Equal(-33.1, Component(coolingPeak, LoadPeakComponent.InfiltrationVentilation), 9);
            Assert.Equal(new[] { LoadPeakComponent.OccupancyLatent, LoadPeakComponent.EquipmentLatent }, coolingPeak.LatentComponents.Select(x => x.Component));
            Assert.DoesNotContain(coolingPeak.SensibleComponents, x => x.Component == LoadPeakComponent.OccupancyLatent || x.Component == LoadPeakComponent.EquipmentLatent);
            Assert.All(coolingPeak.SensibleComponents.Concat(coolingPeak.LatentComponents), x => Assert.Equal(UnitType.Watt, x.Value.Value.Unit));
        }

        // ---------- 8. solver-neutral ----------

        [Fact]
        public void OpenStudioShapedPeak_IsReadWithoutTasAssumptions()
        {
            //OpenStudio's interval-end timestamps are already 0-based in SAM (Core.Query.IntervalHourOfYear): 23 Dec
            //10:00 interval end = hour 8553. No design-day name, no components, no resultant temperature.
            SpaceLoadPeak annual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 2500.5)
            {
                HourOfYear = 8553,
                HourOfDay = 9,
                DryBulbTemperature = 21.0,
                OutdoorDryBulbTemperature = -4.0,
            };

            SpaceDesignLoadDocumentData data = Collect(Model(Result(LoadType.Heating, null, annual, "OpenStudio")));

            Assert.Equal(LoadResultStatus.Available, data.Heating.Status);
            Assert.Equal("OpenStudio", data.Heating.ResultSource.Value);
            Assert.Equal(ReportValueSource.SimulationResult, data.Heating.ResultSource.Source);

            SpaceLoadPeakData peak = data.Heating.Annual;
            AssertQuantity(peak.Load, 2500.5, UnitType.Watt);
            Assert.Equal(new DateTime(ReportingCreate.ReferenceYear, 12, 23, 9, 0, 0), peak.Time.Value);
            AssertQuantity(peak.OutdoorDryBulbTemperature, -4.0, UnitType.Celsius);
            Assert.Equal(Availability.NotAvailable, peak.RoomResultantTemperature.Availability);
            Assert.Empty(peak.SensibleComponents);
            Assert.Equal(LoadPeakState.Unavailable, data.Heating.DesignDay.State);
        }

        [Fact]
        public void TwoSourcesWithPeaks_AreAmbiguous_UnlessASourceIsChosen()
        {
            SpaceSimulationResult tas = Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual(), "Tas");
            SpaceSimulationResult openStudio = Result(LoadType.Heating, null, new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 2500.5) { HourOfYear = 8553, HourOfDay = 9 }, "OpenStudio");
            AnalyticalModel analyticalModel = Model(tas, openStudio);

            SpaceDesignLoadDocumentData data = Collect(analyticalModel);
            Assert.Equal(LoadResultStatus.Ambiguous, data.Heating.Status);
            Assert.Equal(LoadPeakState.Unavailable, data.Heating.DesignDay.State);
            Assert.Equal(LoadPeakState.Unavailable, data.Heating.Annual.State);

            data = Collect(analyticalModel, out _, "OpenStudio");
            Assert.Equal(LoadResultStatus.Available, data.Heating.Status);
            AssertQuantity(data.Heating.Annual.Load, 2500.5, UnitType.Watt);

            data = Collect(analyticalModel, out _, "Tas");
            AssertQuantity(data.Heating.Annual.Load, 104.009911, UnitType.Watt);
        }

        // ---------- 9. whole-object serialization ----------

        [Fact]
        public void Peaks_SurviveAWholeModelStringRoundTrip_AndCollectTheSame()
        {
            SpaceSimulationResult heating = Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual());
            SpaceSimulationResult cooling = Result(LoadType.Cooling, new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0) { DesignDayName = "CDD" }, CoolingAnnual());

            //Through a string, so the reopened values are JsonElement-backed exactly as from a saved file.
            string json = Model(heating, cooling).ToJsonObject().ToJsonString();
            AnalyticalModel reopened = new AnalyticalModel(JsonNode.Parse(json) as JsonObject);

            SpaceDesignLoadDocumentData data = Collect(reopened);

            Assert.Equal(LoadResultStatus.Available, data.Heating.Status);
            AssertQuantity(data.Heating.DesignDay.Load, 1139.79614, UnitType.Watt, 1e-6);
            Assert.Equal(23, data.Heating.DesignDay.HourOfDay.Value);
            Assert.Equal("Leeds_TRY ANN HTG 100% CONDS DB", data.Heating.DesignDay.DesignDayName.Value);
            Assert.Equal(Availability.NotApplicable, data.Heating.DesignDay.Time.Availability);
            Assert.Equal(-1023.25586, Component(data.Heating.DesignDay, LoadPeakComponent.BuildingHeatTransfer), 6);

            AssertQuantity(data.Heating.Annual.Load, 104.009911, UnitType.Watt, 1e-6);
            Assert.Equal(8553, data.Heating.Annual.HourOfYear.Value);
            AssertQuantity(data.Heating.Annual.OutdoorDryBulbTemperature, -2.3, UnitType.Celsius, 1e-6);

            Assert.Equal(LoadPeakState.Zero, data.Cooling.DesignDay.State);
            AssertQuantity(data.Cooling.DesignDay.Load, 0.0, UnitType.Watt);
            AssertQuantity(data.Cooling.Annual.Load, 1369.404, UnitType.Watt, 1e-6);
            Assert.Equal(45.5, Component(data.Cooling.Annual, LoadPeakComponent.EquipmentLatent), 6);
            Assert.Equal("Tas", data.Cooling.ResultSource.Value);
        }

        // ---------- 10. Bathroom_2 ----------

        [Fact]
        public void Bathroom2_BothHeatingPeaks_AreVisibleIndependently()
        {
            SpaceDesignLoadDocumentData data = Collect(Model(Result(LoadType.Heating, BathroomHeatingDesignDay(), BathroomHeatingAnnual())));

            AssertQuantity(data.Heating.DesignDay.Load, 1139.796, UnitType.Watt, 1e-3);
            AssertQuantity(data.Heating.Annual.Load, 104.010, UnitType.Watt, 1e-3);
            Assert.Equal(LoadPeakState.Value, data.Heating.DesignDay.State);
            Assert.Equal(LoadPeakState.Value, data.Heating.Annual.State);
        }

        // ---------- reuse of the Phase-1 data ----------

        [Fact]
        public void Document_ReusesThePhase1IdentityCriteriaAndSizing()
        {
            AnalyticalModel analyticalModel = ReportingFixture.Full(out _);
            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options());
            Space space = ReportingFixture.Stored(analyticalModel);

            SpaceDesignLoadDocumentData data = ReportingCreate.SpaceDesignLoadDocumentData(documentContext, space);
            SpaceDocumentData phase1 = ReportingCreate.SpaceDocumentData(ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options()), space);

            Assert.Equal(phase1.Identity.Guid, data.Identity.Guid);
            Assert.Equal(phase1.Identity.Name.Value, data.Identity.Name.Value);
            Assert.Equal(phase1.DesignCriteria.HeatingSetPoint.Value.Value, data.DesignCriteria.HeatingSetPoint.Value.Value);
            Assert.Equal(phase1.Sizing.DesignHeatingLoad.Value.Value, data.Sizing.DesignHeatingLoad.Value.Value);
            Assert.Equal(phase1.Sizing.DesignHeatingLoadPerArea.Value.Value, data.Sizing.DesignHeatingLoadPerArea.Value.Value);
            Assert.Same(documentContext.Provenance, data.Provenance);

            Assert.Equal(LoadResultStatus.NotSimulated, data.Heating.Status);
            Assert.Equal(LoadResultStatus.NotSimulated, data.Cooling.Status);
        }

        [Fact]
        public void OnlyHeatingAndCooling_HavePeaks()
        {
            AnalyticalModel analyticalModel = Model();
            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options());

            Assert.Throws<ArgumentOutOfRangeException>(() => ReportingCreate.SpaceLoadResultData(documentContext, ReportingFixture.Stored(analyticalModel), LoadType.Undefined));
        }
    }
}
