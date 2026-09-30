// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Mixed Design and the Part O Systems scope (SAM_UI PR-1). The previous Mixed fixtures were system-free, so they
    /// could not show that the Systems route handed SAM_Systems every ventilation system in the model. Here the
    /// baseline carries what <c>Modify.AddMechanicalSystems</c> really builds - <c>NV 1</c> and <c>UV 1</c> with no
    /// unit, <c>MV 1</c> naming <c>AHU1</c>, and the cooling / heating templates - and one dwelling is cooled, so the
    /// whole model goes on the Systems route. The scope is taken from the record's identities, as SAM_UI now does.
    /// <para>
    /// <c>MV 1</c> serves a plant room outside every zone, so the materialiser's authored-plant rule (PR-2's subject)
    /// is not what these tests exercise.
    /// </para>
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        private const string PlantRoom = "Plant Room";

        /// <summary>
        /// The baseline with the <c>AddMechanicalSystems</c> scaffolding: every dwelling room natural (<c>NV</c>), the
        /// corridor uncontrolled (<c>UV</c>), an unzoned plant room mechanical (<c>MV</c>, unit <c>AHU1</c>), cooling
        /// <c>FCU</c> in the dwellings and <c>AHU</c> in the plant room, and heating <c>RAD</c> everywhere.
        /// </summary>
        private static AnalyticalModel ScaffoldedBaseline()
        {
            AnalyticalModel analyticalModel = Baseline();
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space_Plant = new(PlantRoom);
            space_Plant.SetValue(SpaceParameter.Area, 8.0);
            space_Plant.SetValue(SpaceParameter.Volume, 20.0);
            space_Plant.InternalCondition = new InternalCondition(PlantRoom + " IC");
            adjacencyCluster.AddObject(space_Plant);

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                InternalCondition internalCondition = space.InternalCondition is null ? new InternalCondition(space.Name + " IC") : new InternalCondition(space.InternalCondition);

                bool plant = space.Name == PlantRoom;
                bool corridor = space.Name == Corridor;

                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, plant ? "MV" : corridor ? "UV" : "NV");
                internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, plant ? "AHU" : "FCU");
                internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, "RAD");

                space.InternalCondition = internalCondition;
                adjacencyCluster.AddObject(space);
            }

            Core.SystemTypeLibrary systemTypeLibrary = Analytical.Query.DefaultSystemTypeLibrary();
            Assert.NotNull(systemTypeLibrary);

            Assert.NotEmpty(adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, null, "AHU1", "AHU1"));

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static List<Guid> SpaceGuidsOf(AnalyticalModel analyticalModel, IEnumerable<Guid> guids_Zone)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            List<Guid> result = [];
            foreach (Guid guid_Zone in guids_Zone)
            {
                result.AddRange((adjacencyCluster.GetRelatedObjects<Space>(adjacencyCluster.GetObject<Zone>(guid_Zone)) ?? []).ConvertAll(x => x.Guid));
            }

            return result;
        }

        private static PartOSystemsMaterialisationScope SystemsScope(PartOMaterialisation partOMaterialisation)
        {
            return Analytical.Query.PartOSystemsMaterialisationScope(
                partOMaterialisation.AnalyticalModel.AdjacencyCluster,
                partOMaterialisation.Record.VentilationSystemGuids.Values,
                SpaceGuidsOf(partOMaterialisation.AnalyticalModel, partOMaterialisation.Record.ZoneGuids_Assessed));
        }

        [Fact]
        public void SystemsScope_ScaffoldingIsTheAddMechanicalSystemsShape()
        {
            AdjacencyCluster adjacencyCluster = ScaffoldedBaseline().AdjacencyCluster;

            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetObjects<VentilationSystem>();
            Assert.Equal(["MV 1", "NV 1", "UV 1"], ventilationSystems.ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));

            //NV and UV name no unit, by construction; MV names AHU1, which exists.
            Assert.All(ventilationSystems.FindAll(x => x.FullName != "MV 1"), x => Assert.True(string.IsNullOrEmpty(x.GetValue<string>(VentilationSystemParameter.SupplyUnitName))));
            Assert.Equal("AHU1", ventilationSystems.Find(x => x.FullName == "MV 1").GetValue<string>(VentilationSystemParameter.SupplyUnitName));
            Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>(), x => x.Name == "AHU1");

            Assert.Equal(["AHU 1", "FCU 1"], adjacencyCluster.GetObjects<CoolingSystem>().ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));
            Assert.Equal(["RAD 1"], adjacencyCluster.GetObjects<HeatingSystem>().ConvertAll(x => x.FullName));
        }

        /// <summary>
        /// <b>The production defect, fixed.</b> One cooled dwelling puts the whole model on the Systems route. The scope
        /// taken from the record's identities hands SAM_Systems only the Part O systems: NV 1 and UV 1 - which name no
        /// unit and never will - are left out with a note, not refused.
        /// </summary>
        [Fact]
        public void SystemsScope_ACooledDwellingBesideNVAndUV_ScopesToThePartOSystemsOnly()
        {
            AnalyticalModel baseline = WithStrategies(ScaffoldedBaseline(), Cooled(Flat1, Small.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3));
            string json_Baseline = baseline.ToJsonObject().ToJsonString();

            PartOMaterialisation partOMaterialisation = MaterialiseCooled(baseline, [Cooling(Small)]);
            Assert.Equal(PartOSimulationRoute.Systems, partOMaterialisation.Route);
            Assert.Equal(2, partOMaterialisation.Record.VentilationSystemGuids.Count);

            AdjacencyCluster adjacencyCluster = partOMaterialisation.AnalyticalModel.AdjacencyCluster;
            List<VentilationSystem> ventilationSystems_Scaffold = adjacencyCluster.GetObjects<VentilationSystem>().FindAll(x => !partOMaterialisation.Record.VentilationSystemGuids.ContainsValue(x.Guid));
            Assert.Equal(["MV 1", "NV 1", "UV 1"], ventilationSystems_Scaffold.ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));

            PartOSystemsMaterialisationScope scope = SystemsScope(partOMaterialisation);
            output.WriteLine(string.Join(Environment.NewLine, scope.Notes));

            Assert.True(scope.IsScoped, scope.Refusal);

            List<Guid> guids_Built = [.. partOMaterialisation.Record.VentilationSystemGuids.Values];
            guids_Built.Sort();
            Assert.Equal(guids_Built, scope.Guids_Retained);

            List<Guid> guids_Scaffold = ventilationSystems_Scaffold.ConvertAll(x => x.Guid);
            guids_Scaffold.Sort();
            Assert.Equal(guids_Scaffold, scope.Guids_Removed);

            //What SAM_Systems is handed: the Part O systems, each naming a unit that exists.
            List<VentilationSystem> ventilationSystems_Working = scope.AdjacencyCluster.GetObjects<VentilationSystem>();
            Assert.Equal(guids_Built, ventilationSystems_Working.ConvertAll(x => x.Guid).OrderBy(x => x).ToList());
            Assert.All(ventilationSystems_Working, x => Assert.Contains(scope.AdjacencyCluster.GetObjects<AirHandlingUnit>(), y => y.Name == x.GetValue<string>(VentilationSystemParameter.SupplyUnitName)));

            //The cooling and heating templates are not consumed.
            Assert.Equal(2, scope.AdjacencyCluster.GetObjects<CoolingSystem>().Count);
            Assert.Single(scope.AdjacencyCluster.GetObjects<HeatingSystem>());

            //Nothing is mutated: the materialised model keeps its scaffolding, and the baseline is byte-identical.
            Assert.Equal(5, partOMaterialisation.AnalyticalModel.AdjacencyCluster.GetObjects<VentilationSystem>().Count);
            Assert.Equal(json_Baseline, baseline.ToJsonObject().ToJsonString());
        }

        /// <summary>
        /// The Iteration 3 safety rule carries over to Mixed: an authored mechanical system with real duty in a room the
        /// materialiser does not judge (the unzoned plant room) would lose that duty on the Systems route, so the scope
        /// refuses - where the materialisation alone would have proceeded.
        /// </summary>
        [Fact]
        public void SystemsScope_AnAuthoredMechanicalDutyOutsideTheDwellings_StillRefuses()
        {
            AnalyticalModel baseline = ScaffoldedBaseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            VentilationSystem ventilationSystem_MV = adjacencyCluster.GetObjects<VentilationSystem>().Find(x => x.FullName == "MV 1");
            Space space_Plant = adjacencyCluster.GetSpaces().Find(x => x.Name == PlantRoom);
            VentilationTerminal ventilationTerminal = new("Plant Room Supply", FlowClassification.Supply, 30.0);
            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationSystem_MV, ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, space_Plant);

            baseline = WithStrategies(new AnalyticalModel(baseline, adjacencyCluster), Cooled(Flat1, Small.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3));

            PartOMaterialisation partOMaterialisation = MaterialiseCooled(baseline, [Cooling(Small)]);

            PartOSystemsMaterialisationScope scope = SystemsScope(partOMaterialisation);

            Assert.False(scope.IsScoped);
            Assert.Null(scope.AdjacencyCluster);

            PartOSystemsScopeRefusal refusal = Assert.Single(scope.Refusals);
            Assert.Equal(PartOSystemsScopeRefusalReason.DutyOutsideDwellingScope, refusal.Reason);
            Assert.Equal(ventilationSystem_MV.Guid, refusal.Guid_VentilationSystem);
            Assert.Equal(space_Plant.Guid, refusal.Guid_Space);
            Assert.Equal(30.0, refusal.DesignFlowRate_Lps);
        }

        /// <summary>The system-free fixture every earlier Mixed test used: scoped, nothing left out.</summary>
        [Fact]
        public void SystemsScope_ASystemFreeBaseline_RetainsThePartOSystemsAndLeavesNothingOut()
        {
            AnalyticalModel baseline = WithStrategies(Baseline(), Cooled(Flat1, Small.VentilationUnitReference), Natural(Flat2), Mvhr(Flat3));

            PartOMaterialisation partOMaterialisation = MaterialiseCooled(baseline, [Cooling(Small)]);

            PartOSystemsMaterialisationScope scope = SystemsScope(partOMaterialisation);

            Assert.True(scope.IsScoped, scope.Refusal);
            Assert.Equal(2, scope.Guids_Retained.Count);
            Assert.Empty(scope.Guids_Removed);
        }
    }
}
