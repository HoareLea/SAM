// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Reporting;
using SAM.Core;
using SAM.Core.Reporting;
using SAM.Geometry.Spatial;
using SAM.Units;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using ReportingCreate = SAM.Analytical.Reporting.Create;

namespace SAM.Tests.Helpers
{
    /// <summary>
    /// The Space Design Load Summary cases (PR2C visual gate and tests). Results are typed <see cref="SpaceLoadPeak"/>s
    /// with the values the production Tas conversion (SAM_Tas#69) wrote from real TSDs: Bathroom_2 of final1b/open.tsd and
    /// Studio 1_0 of pr3/final/bridge.tsd, every stored term included, zeros too. Only the stress case is invented,
    /// and it says so in its space name. GUIDs, generation time and version are fixed, so documents are deterministic.
    /// </summary>
    public static class SpaceDesignLoadFixture
    {
        public const string HeatingDesignDayName = "Leeds_TRY ANN HTG 100% CONDS DB";
        public const string CoolingDesignDayName = "Leeds_TRY ANN CLG 0% CONDS DB=>GRad";

        public const string StressSpaceName = "02_214 Illustrative stress case - open-plan studio, meeting and breakout zone with long name (values invented)";
        public const string StressHeatingDesignDayName = "London_Heathrow_Illustrative_Heating_Design_Day_99.6%_Coincident_Wet_Bulb_Extreme_Winter_Condition";
        public const string StressCoolingDesignDayName = "London_Heathrow_Illustrative_Cooling_Design_Day_0.4%_Dry_Bulb=>Mean_Coincident_Wet_Bulb_With_Global_Radiation";

        public static readonly DateTime ConvertedAt = new DateTime(2026, 8, 27, 18, 38, 0);

        private static readonly Guid spaceGuid = new Guid("e2c8a683-410d-42fd-92cb-bcf1e87e5a3a");

        /// <summary>
        /// The gate cases: name, space name, results, unit system.
        /// </summary>
        public static IEnumerable<(string Name, string SpaceName, Func<SpaceSimulationResult[]> Results, UnitStyle UnitStyle)> Cases()
        {
            yield return ("A_Bathroom_2_SI", "Bathroom_2", Bathroom, UnitStyle.SI);
            yield return ("A_Bathroom_2_IP", "Bathroom_2", Bathroom, UnitStyle.Imperial);
            yield return ("B_Studio_1_0_SI", "Studio 1_0", Studio, UnitStyle.SI);
            yield return ("B_Studio_1_0_IP", "Studio 1_0", Studio, UnitStyle.Imperial);
            yield return ("C_NotSimulated_SI", "Corridor_1", () => new SpaceSimulationResult[0], UnitStyle.SI);
            yield return ("D_PeaksNotRecorded_SI", "Studio 1_0", Legacy, UnitStyle.SI);
            yield return ("E_Ambiguous_SI", "Bathroom_2", Ambiguous, UnitStyle.SI);
            yield return ("G_Stress_SI", StressSpaceName, Stress, UnitStyle.SI);
            yield return ("G_Stress_IP", StressSpaceName, Stress, UnitStyle.Imperial);
        }

        public static Document Document(string name, string resultSource = null)
        {
            (string _, string spaceName, Func<SpaceSimulationResult[]> results, UnitStyle unitStyle) = Cases().Single(x => x.Name == name);
            return Document(Model(spaceName, results()), unitStyle, resultSource);
        }

        public static Document Document(AnalyticalModel analyticalModel, UnitStyle unitStyle, string resultSource = null)
        {
            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options(unitStyle));
            return ReportingCreate.SpaceDesignLoadSummary(documentContext, analyticalModel.AdjacencyCluster.GetObject<Space>(spaceGuid), resultSource);
        }

        public static SpaceDesignLoadDocumentData Data(string name, string resultSource = null)
        {
            (string _, string spaceName, Func<SpaceSimulationResult[]> results, UnitStyle unitStyle) = Cases().Single(x => x.Name == name);
            AnalyticalModel analyticalModel = Model(spaceName, results());
            DocumentContext documentContext = ReportingCreate.DocumentContext(analyticalModel, ReportingFixture.Options(unitStyle));
            return ReportingCreate.SpaceDesignLoadDocumentData(documentContext, analyticalModel.AdjacencyCluster.GetObject<Space>(spaceGuid), resultSource);
        }

        /// <summary>
        /// A space with an internal condition, heating / cooling set points, design days and stored design loads,
        /// related to the given results.
        /// </summary>
        public static AnalyticalModel Model(string spaceName, params SpaceSimulationResult[] spaceSimulationResults)
        {
            ProfileLibrary profileLibrary = new ProfileLibrary("Profiles");
            profileLibrary.Add(new Profile("Heat 16", ProfileType.Heating, Enumerable.Repeat(16.0, 24)));
            profileLibrary.Add(new Profile("Cool 26", ProfileType.Cooling, Enumerable.Repeat(26.0, 24)));

            InternalCondition internalCondition = new InternalCondition(new Guid("0b7e2c8a-7a51-4d6c-9e37-51c4bd0b5c11"), "TM59_Bathroom");
            internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, "Heat 16");
            internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, "Cool 26");

            Space space = new Space(spaceGuid, spaceName, new Point3D(1, 1, 1.5));
            space.InternalCondition = internalCondition;
            space.SetValue(SpaceParameter.Area, 5.2);
            space.SetValue(SpaceParameter.Volume, 13.0);
            space.SetValue(SpaceParameter.LevelName, "Level 0");
            space.SetValue(SpaceParameter.DesignHeatingLoad, 1368.0);
            space.SetValue(SpaceParameter.DesignCoolingLoad, 0.0);

            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();
            adjacencyCluster.AddObject(space);
            foreach (SpaceSimulationResult spaceSimulationResult in spaceSimulationResults)
            {
                adjacencyCluster.AddObject(spaceSimulationResult);
                adjacencyCluster.AddRelation(space, spaceSimulationResult);
            }

            AnalyticalModel analyticalModel = new AnalyticalModel("000000_SAM_AnalyticalModel", null, null, null, adjacencyCluster, null, profileLibrary);
            analyticalModel.SetValue(AnalyticalModelParameter.HeatingDesignDays, new SAMCollection<DesignDay>() { DesignDay(HeatingDesignDayName, -4.9, 90, false) });
            analyticalModel.SetValue(AnalyticalModelParameter.CoolingDesignDays, new SAMCollection<DesignDay>() { DesignDay(CoolingDesignDayName, 28.4, 40, true) });

            return analyticalModel;
        }

        // ---------- results ----------

        /// <summary>
        /// Bathroom_2 (final1b/open.tsd): heating 1139.796 W at hour 23 of the heating design day and 104.010 W on
        /// 23 Dec 09:00 over the year; no cooling demand in either (a real zero).
        /// </summary>
        public static SpaceSimulationResult[] Bathroom()
        {
            SpaceLoadPeak heatingDesignDay = Peak(LoadPeakBasis.DesignDay, 1139.796143, HeatingDesignDayName, null, 23, 16, 13.870544, 19.637472, 0.002208, null, null,
                infiltrationVentilation: -111.737091, buildingHeatTransfer: -1023.255859, opaque: -4.802979, glazing: 0);
            SpaceLoadPeak heatingAnnual = Peak(LoadPeakBasis.AnnualSimulation, 104.009911, null, 8553, 9, 16.000908, 15.93449, 34.96767, 0.003943, -2.3, 100,
                infiltrationVentilation: -93.373978, buildingHeatTransfer: -3.157799, opaque: -7.477905, glazing: 0);

            SpaceLoadPeak coolingDesignDay = new SpaceLoadPeak(LoadPeakBasis.DesignDay, 0) { DesignDayName = CoolingDesignDayName };
            SpaceLoadPeak coolingAnnual = new SpaceLoadPeak(LoadPeakBasis.AnnualSimulation, 0);

            return new[] { Result(LoadType.Heating, heatingDesignDay, heatingAnnual), Result(LoadType.Cooling, coolingDesignDay, coolingAnnual) };
        }

        /// <summary>
        /// Studio 1_0 (pr3/final/bridge.tsd): heated and cooled; four independent peaks.
        /// </summary>
        public static SpaceSimulationResult[] Studio()
        {
            SpaceLoadPeak heatingDesignDay = Peak(LoadPeakBasis.DesignDay, 2267.908447, HeatingDesignDayName, null, 15, 15.079316, 13.245728, 20.831598, 0.002208, null, null,
                infiltrationVentilation: -321.118866, buildingHeatTransfer: -664.186035, opaque: -894.853943, glazing: -387.750549);
            SpaceLoadPeak heatingAnnual = Peak(LoadPeakBasis.AnnualSimulation, 802.083435, null, 0, 0, 13.369207, 12.633975, 84.367348, 0.008081, 3.3, 100,
                occupancySensible: 105, equipmentSensible: 85.004997, infiltrationVentilation: -154.12381, buildingHeatTransfer: -326.554138, opaque: -327.757477, glazing: -183.65416, occupancyLatent: 77);

            SpaceLoadPeak coolingDesignDay = Peak(LoadPeakBasis.DesignDay, 1973.467651, CoolingDesignDayName, null, 0, 19.758215, 21.455769, 99.999992, 0.014528, null, null,
                occupancySensible: 105, equipmentSensible: 85.004997, infiltrationVentilation: -8.544277, buildingHeatTransfer: 971.441589, opaque: 898.013489, glazing: -77.442261, occupancyLatent: 77);
            SpaceLoadPeak coolingAnnual = Peak(LoadPeakBasis.AnnualSimulation, 1972.137451, null, 4411, 19, 18.957739, 20.592365, 58.269211, 0.007971, 17.5, 59,
                solar: 835.963074, lighting: 150, occupancySensible: 150, equipmentSensible: 450, infiltrationVentilation: -52.705479, buildingHeatTransfer: 298.012939, opaque: 219.199493, glazing: -78.335747, occupancyLatent: 110);

            return new[] { Result(LoadType.Heating, heatingDesignDay, heatingAnnual), Result(LoadType.Cooling, coolingDesignDay, coolingAnnual) };
        }

        /// <summary>
        /// Results written before the typed peaks: load type and the legacy values only.
        /// </summary>
        public static SpaceSimulationResult[] Legacy()
        {
            SpaceSimulationResult heating = new SpaceSimulationResult("Studio 1_0", "Tas", "zone-guid");
            heating.SetValue(SpaceSimulationResultParameter.LoadType, LoadType.Heating.Text());
            heating.SetValue(SpaceSimulationResultParameter.Load, -1.0);

            SpaceSimulationResult cooling = new SpaceSimulationResult("Studio 1_0", "Tas", "zone-guid");
            cooling.SetValue(SpaceSimulationResultParameter.LoadType, LoadType.Cooling.Text());
            cooling.SetValue(SpaceSimulationResultParameter.Load, -1.0);

            return new[] { Converted(heating), Converted(cooling) };
        }

        /// <summary>
        /// Bathroom_2's heating from two sources, both with peaks (the second differs so a pick would show); its
        /// cooling from Tas only.
        /// </summary>
        public static SpaceSimulationResult[] Ambiguous()
        {
            SpaceSimulationResult[] bathroom = Bathroom();
            SpaceSimulationResult openStudio = Result(LoadType.Heating, new SpaceLoadPeak(LoadPeakBasis.DesignDay, 1500.0) { DesignDayName = "OpenStudio heating DD", HourOfDay = 6 }, null, "OpenStudio");
            return new[] { bathroom[0], openStudio, bathroom[1] };
        }

        /// <summary>
        /// Invented, for layout only: long names, kW loads and every component, sensible and latent.
        /// </summary>
        public static SpaceSimulationResult[] Stress()
        {
            SpaceLoadPeak heatingDesignDay = Peak(LoadPeakBasis.DesignDay, 23456.7, StressHeatingDesignDayName, null, 6, 18.2, 16.9, 22.5, 0.0025, null, null,
                solar: 0, lighting: 0, occupancySensible: 0, equipmentSensible: 0, infiltrationVentilation: -8123.4, airMovement: -1234.5, buildingHeatTransfer: -3456.7, opaque: -6789.1, glazing: -3853.0, airHandlingUnit: 0, occupancyLatent: 0, equipmentLatent: 0);
            SpaceLoadPeak heatingAnnual = Peak(LoadPeakBasis.AnnualSimulation, 15432.1, null, 8759, 23, 18.0, 17.1, 45.2, 0.0051, -6.8, 95,
                solar: 0, lighting: 350, occupancySensible: 420, equipmentSensible: 1250, infiltrationVentilation: -6012.3, airMovement: -987.6, buildingHeatTransfer: -2345.6, opaque: -5123.4, glazing: -2983.2, airHandlingUnit: 0, occupancyLatent: 310, equipmentLatent: 45);
            SpaceLoadPeak coolingDesignDay = Peak(LoadPeakBasis.DesignDay, 31234.5, StressCoolingDesignDayName, null, 15, 24.0, 25.8, 55.1, 0.0102, null, null,
                solar: 12345.6, lighting: 2100, occupancySensible: 3150, equipmentSensible: 7500, infiltrationVentilation: 1234.5, airMovement: 456.7, buildingHeatTransfer: 1987.6, opaque: 2345.6, glazing: 1234.5, airHandlingUnit: -1120.0, occupancyLatent: 2300, equipmentLatent: 150);
            SpaceLoadPeak coolingAnnual = Peak(LoadPeakBasis.AnnualSimulation, 28765.4, null, 4983, 15, 24.0, 25.2, 52.3, 0.0098, 30.1, 38,
                solar: 11234.5, lighting: 2100, occupancySensible: 3150, equipmentSensible: 7500, infiltrationVentilation: 987.6, airMovement: 345.6, buildingHeatTransfer: 1456.7, opaque: 1876.5, glazing: 1109.0, airHandlingUnit: -994.0, occupancyLatent: 2300, equipmentLatent: 150);

            return new[] { Result(LoadType.Heating, heatingDesignDay, heatingAnnual), Result(LoadType.Cooling, coolingDesignDay, coolingAnnual) };
        }

        public static SpaceSimulationResult Result(LoadType loadType, SpaceLoadPeak designDay, SpaceLoadPeak annual, string source = "Tas")
        {
            SpaceSimulationResult result = new SpaceSimulationResult("zone", source, "zone-guid");
            result.SetValue(SpaceSimulationResultParameter.LoadType, loadType.Text());
            if (designDay != null)
            {
                result.SetValue(SpaceSimulationResultParameter.DesignDayPeak, designDay);
            }

            if (annual != null)
            {
                result.SetValue(SpaceSimulationResultParameter.AnnualPeak, annual);
            }

            return Converted(result);
        }

        /// <summary>
        /// The result with the fixed time it was read into the model (Result.DateTime has no setter; a result takes
        /// the current time when created, which would make documents differ run to run).
        /// </summary>
        private static SpaceSimulationResult Converted(SpaceSimulationResult spaceSimulationResult)
        {
            System.Text.Json.Nodes.JsonObject jsonObject = spaceSimulationResult.ToJsonObject();
            jsonObject["DateTime"] = SAM.Core.Query.ToJsonNode(ConvertedAt);
            // Through a string, as from a saved model: parsed numbers are JsonElement-backed.
            return new SpaceSimulationResult(System.Text.Json.Nodes.JsonNode.Parse(jsonObject.ToJsonString()).AsObject());
        }

        /// <summary>
        /// A peak as the Tas conversion stores it: every term, zeros included.
        /// </summary>
        private static SpaceLoadPeak Peak(LoadPeakBasis basis, double load, string designDayName, int? hourOfYear, int? hourOfDay, double? dryBulb, double? resultant, double? relativeHumidity, double? humidityRatio, double? outdoorDryBulb, double? outdoorRelativeHumidity,
            double solar = 0, double lighting = 0, double occupancySensible = 0, double equipmentSensible = 0, double infiltrationVentilation = 0, double airMovement = 0, double buildingHeatTransfer = 0, double opaque = 0, double glazing = 0, double airHandlingUnit = 0, double occupancyLatent = 0, double equipmentLatent = 0)
        {
            SpaceLoadPeak result = new SpaceLoadPeak(basis, load)
            {
                DesignDayName = designDayName,
                HourOfYear = hourOfYear,
                HourOfDay = hourOfDay,
                DryBulbTemperature = dryBulb,
                ResultantTemperature = resultant,
                RelativeHumidity = relativeHumidity,
                HumidityRatio = humidityRatio,
                OutdoorDryBulbTemperature = outdoorDryBulb,
                OutdoorRelativeHumidity = outdoorRelativeHumidity,
            };

            result.SetComponent(LoadPeakComponent.Solar, solar);
            result.SetComponent(LoadPeakComponent.Lighting, lighting);
            result.SetComponent(LoadPeakComponent.OccupancySensible, occupancySensible);
            result.SetComponent(LoadPeakComponent.EquipmentSensible, equipmentSensible);
            result.SetComponent(LoadPeakComponent.InfiltrationVentilation, infiltrationVentilation);
            result.SetComponent(LoadPeakComponent.AirMovement, airMovement);
            result.SetComponent(LoadPeakComponent.BuildingHeatTransfer, buildingHeatTransfer);
            result.SetComponent(LoadPeakComponent.ExternalConductionOpaque, opaque);
            result.SetComponent(LoadPeakComponent.ExternalConductionGlazing, glazing);
            result.SetComponent(LoadPeakComponent.AirHandlingUnit, airHandlingUnit);
            result.SetComponent(LoadPeakComponent.OccupancyLatent, occupancyLatent);
            result.SetComponent(LoadPeakComponent.EquipmentLatent, equipmentLatent);
            return result;
        }

        private static DesignDay DesignDay(string name, double extremeDryBulb, double relativeHumidity, bool cooling)
        {
            DesignDay designDay = new DesignDay(name, 2018, (byte)(cooling ? 7 : 1), 15);

            int hour = cooling ? 15 : 5;
            designDay[WeatherDataType.DryBulbTemperature] = Enumerable.Range(0, 24).Select(x => x == hour ? extremeDryBulb : extremeDryBulb + (cooling ? -5.0 : 4.0)).ToArray();
            designDay[WeatherDataType.RelativeHumidity] = Enumerable.Range(0, 24).Select(x => x == hour ? relativeHumidity : 50.0).ToArray();
            return designDay;
        }
    }
}
