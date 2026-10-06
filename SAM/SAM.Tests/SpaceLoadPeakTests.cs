// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Phase-2 audit B2/B3/B4 (PR2A-1): the typed per-simulation peak on <see cref="SpaceSimulationResult"/>. The
    /// values are Bathroom_2's real TSD peaks (C:\TasOut\final1b\open.tsd, Leeds TRY): heating design day
    /// 1139.796 W at Tas hour 1608, annual 104.010 W at Tas hour 8554 (23 Dec 09:00–10:00).
    /// </summary>
    public class SpaceLoadPeakTests
    {
        private static SpaceLoadPeak DesignDayPeak()
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

        private static SpaceLoadPeak AnnualPeak()
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

        private static SpaceSimulationResult HeatingResult()
        {
            SpaceSimulationResult result = new SpaceSimulationResult("Bathroom_2", "Tas", "zone-guid");
            result.SetValue(SpaceSimulationResultParameter.LoadType, LoadType.Heating.Text());
            result.SetValue(SpaceSimulationResultParameter.DesignDayPeak, DesignDayPeak());
            result.SetValue(SpaceSimulationResultParameter.AnnualPeak, AnnualPeak());
            return result;
        }

        private static SpaceLoadPeak RoundTrip(SpaceLoadPeak spaceLoadPeak)
        {
            //Through a string, so the reread values are JsonElement-backed exactly as from a saved file.
            string json = spaceLoadPeak.ToJsonObject().ToJsonString();
            return new SpaceLoadPeak(JsonNode.Parse(json) as JsonObject);
        }

        [Fact]
        public void BothPeaks_SurviveAWholeModelSaveAndReopen_Independently()
        {
            Space space = new Space("Bathroom_2", new Point3D(0, 0, 0));
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            SpaceSimulationResult spaceSimulationResult = HeatingResult();
            adjacencyCluster.AddObject(spaceSimulationResult);
            adjacencyCluster.AddRelation(space, spaceSimulationResult);

            AnalyticalModel analyticalModel = new AnalyticalModel("Model", null, null, null, adjacencyCluster);
            string json = analyticalModel.ToJsonObject().ToJsonString();
            AnalyticalModel reopened = new AnalyticalModel(JsonNode.Parse(json) as JsonObject);

            SpaceSimulationResult result = reopened.AdjacencyCluster.GetObjects<SpaceSimulationResult>().Single();
            Assert.True(result.TryGetValue(SpaceSimulationResultParameter.DesignDayPeak, out SpaceLoadPeak designDay));
            Assert.True(result.TryGetValue(SpaceSimulationResultParameter.AnnualPeak, out SpaceLoadPeak annual));

            Assert.Equal(LoadPeakBasis.DesignDay, designDay.Basis);
            Assert.Equal(1139.79614, designDay.Load, 6);
            Assert.Equal("Leeds_TRY ANN HTG 100% CONDS DB", designDay.DesignDayName);
            Assert.Equal(23, designDay.HourOfDay);
            Assert.Null(designDay.HourOfYear);
            Assert.Null(designDay.OutdoorDryBulbTemperature);
            Assert.Equal(19.6374722, designDay.RelativeHumidity!.Value, 6);

            Assert.Equal(LoadPeakBasis.AnnualSimulation, annual.Basis);
            Assert.Equal(104.009911, annual.Load, 6);
            Assert.Equal(8553, annual.HourOfYear);
            Assert.Equal(-2.3, annual.OutdoorDryBulbTemperature!.Value, 6);
            Assert.Null(annual.DesignDayName);
        }

        [Fact]
        public void HeatingComponents_KeepTheirSign_AndCloseOnTheLoad()
        {
            foreach (SpaceLoadPeak spaceLoadPeak in new[] { RoundTrip(DesignDayPeak()), RoundTrip(AnnualPeak()) })
            {
                Assert.True(spaceLoadPeak.Load > 0);
                Assert.All(spaceLoadPeak.Components.Values, x => Assert.True(x <= 0));

                //Heating: the load replaces the net loss, so load = -(sum of gains to room air).
                Assert.Equal(spaceLoadPeak.Load, -spaceLoadPeak.Components.Values.Sum(), 3);
            }
        }

        [Fact]
        public void ZeroPeak_IsARealZero_WithNoTimeStateOrComponents()
        {
            SpaceLoadPeak zero = RoundTrip(new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0) { DesignDayName = "CDD" });

            Assert.Equal(0.0, zero.Load);
            Assert.Null(zero.HourOfDay);
            Assert.Null(zero.HourOfYear);
            Assert.Null(zero.DryBulbTemperature);
            Assert.Empty(zero.Components);
            Assert.False(zero.TryGetDateTime(2018, out _));

            string json = zero.ToJsonObject().ToJsonString();
            Assert.DoesNotContain("-1", json);
        }

        [Fact]
        public void MissingPeak_IsUnavailable_NotZero()
        {
            SpaceSimulationResult spaceSimulationResult = new SpaceSimulationResult("Studio 1_0", "Tas", "zone-guid");
            spaceSimulationResult.SetValue(SpaceSimulationResultParameter.LoadType, LoadType.Cooling.Text());
            spaceSimulationResult.SetValue(SpaceSimulationResultParameter.AnnualPeak, new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 0));

            Assert.False(spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.DesignDayPeak, out SpaceLoadPeak _));
            Assert.True(spaceSimulationResult.TryGetValue(SpaceSimulationResultParameter.AnnualPeak, out SpaceLoadPeak annual));
            Assert.Equal(0.0, annual.Load);
        }

        [Fact]
        public void GenuineNegativeOneValues_AreKeptAsValues()
        {
            SpaceLoadPeak spaceLoadPeak = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 250) { HourOfYear = 100, HourOfDay = 4, OutdoorDryBulbTemperature = -1.0 };
            spaceLoadPeak.SetComponent(LoadPeakComponent.Solar, -1.0);

            SpaceLoadPeak reread = RoundTrip(spaceLoadPeak);
            Assert.Equal(-1.0, reread.OutdoorDryBulbTemperature);
            Assert.True(reread.TryGetComponent(LoadPeakComponent.Solar, out double solar));
            Assert.Equal(-1.0, solar);
            Assert.False(reread.TryGetComponent(LoadPeakComponent.Lighting, out _));
        }

        [Fact]
        public void NonFiniteComponent_IsNotRecorded()
        {
            SpaceLoadPeak spaceLoadPeak = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 1);
            Assert.False(spaceLoadPeak.SetComponent(LoadPeakComponent.Solar, double.NaN));
            Assert.False(spaceLoadPeak.SetComponent(LoadPeakComponent.Solar, double.PositiveInfinity));
            Assert.Empty(spaceLoadPeak.Components);
        }

        /// <summary>
        /// B4: one hour, two engine conventions, one SAM value. Tas numbers the hour 23 Dec 09:00–10:00 as 8554
        /// (1-based); OpenStudio stamps it with its interval end, 23 Dec 10:00, which
        /// <see cref="Core.Query.IntervalHourOfYear"/> turns into the 0-based 8553. SAM stores 8553.
        /// </summary>
        [Fact]
        public void AnnualHour_IsZeroBased_AndMapsToTheSameDateForEitherEngine()
        {
            const int tasHourIndex = 8554;
            int openStudioHourOfYear = Core.Query.IntervalHourOfYear(new DateTime(2018, 12, 23, 10, 0, 0));

            Assert.Equal(tasHourIndex - 1, openStudioHourOfYear);

            SpaceLoadPeak spaceLoadPeak = RoundTrip(AnnualPeak());
            Assert.Equal(openStudioHourOfYear, spaceLoadPeak.HourOfYear);
            Assert.True(spaceLoadPeak.TryGetDateTime(2018, out DateTime dateTime));
            Assert.Equal(new DateTime(2018, 12, 23, 9, 0, 0), dateTime);
            Assert.Equal(dateTime.Hour, spaceLoadPeak.HourOfDay);
        }

        [Fact]
        public void DesignDayPeak_HasNoCalendarDate()
        {
            SpaceLoadPeak spaceLoadPeak = RoundTrip(DesignDayPeak());
            Assert.Equal(23, spaceLoadPeak.HourOfDay);
            Assert.False(spaceLoadPeak.TryGetDateTime(2018, out _));

            //Even a stray HourOfYear on a design-day peak must not become a date.
            spaceLoadPeak.HourOfYear = 1607;
            Assert.False(spaceLoadPeak.TryGetDateTime(2018, out _));
        }

        [Fact]
        public void Clone_IsIndependent()
        {
            SpaceLoadPeak source = AnnualPeak();
            SpaceLoadPeak clone = new SpaceLoadPeak(source);
            clone.SetComponent(LoadPeakComponent.Solar, 5);
            clone.HourOfYear = 1;

            Assert.False(source.TryGetComponent(LoadPeakComponent.Solar, out _));
            Assert.Equal(8553, source.HourOfYear);
            Assert.Equal(source.ToJsonObject().ToJsonString(), new SpaceLoadPeak(source).ToJsonObject().ToJsonString());
        }
    }
}
