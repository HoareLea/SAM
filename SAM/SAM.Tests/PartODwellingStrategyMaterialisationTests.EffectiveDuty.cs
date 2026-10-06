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
    /// Part O PR-2: effective-duty classification of authored ventilation plant (<c>Query.PartOAuthoredPlantDuty</c>)
    /// and its use in Mixed Design's authored-plant rule. The fixture is the owner's real model's shape, built by
    /// <c>Modify.AddMechanicalSystems</c> itself: Flat 1 natural (<c>NV 1</c>), the corridor uncontrolled
    /// (<c>UV 1</c>), and Flats 2 and 3 on ONE template <c>MV 1</c> naming <c>AHU1</c> - no terminal, no movement, no
    /// airflow, no product. The owner's selection is Flat 1 natural, Flat 2 MVHR, Flat 3 MVHR with cooling.
    /// </summary>
    public partial class PartODwellingStrategyMaterialisationTests
    {
        private const string Unit_Legacy = "AHU1";
        private const string System_Legacy = "MV 1";

        private static readonly List<VentilationUnitCapacityDescriptor> Catalogue_Owner = [Small, Large];

        /// <summary>
        /// The real model's shape: <c>AddMechanicalSystems</c> over every space, with each space's internal condition
        /// naming its template - <c>NV</c> in Flat 1, <c>UV</c> in the corridor, <c>MV</c> in Flats 2 and 3 - and
        /// <c>AHU1</c> as the unit, exactly as the SAM Assign/Add Mechanical Systems workflow builds it.
        /// </summary>
        private static AnalyticalModel LegacyScaffoldBaseline()
        {
            AnalyticalModel analyticalModel = Baseline();
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Dictionary<Guid, string> dictionary_Template = [];
            foreach (Zone zone in adjacencyCluster.GetObjects<Zone>())
            {
                foreach (Space space in adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [])
                {
                    dictionary_Template[space.Guid] = zone.Name == Flat1 ? "NV" : zone.Name == Corridor ? "UV" : "MV";
                }
            }

            SetTemplates(adjacencyCluster, dictionary_Template);

            Assert.NotEmpty(adjacencyCluster.AddMechanicalSystems(Analytical.Query.DefaultSystemTypeLibrary(), null, Unit_Legacy, Unit_Legacy));

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static void SetTemplates(AdjacencyCluster adjacencyCluster, Dictionary<Guid, string> dictionary_Template)
        {
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                if (!dictionary_Template.TryGetValue(space.Guid, out string name_Template))
                {
                    continue;
                }

                InternalCondition internalCondition = space.InternalCondition is null ? new InternalCondition(space.Name + " IC") : new InternalCondition(space.InternalCondition);
                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, name_Template);
                internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, "FCU");
                internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, "RAD");

                space.InternalCondition = internalCondition;
                adjacencyCluster.AddObject(space);
            }
        }

        /// <summary>Flat 1 natural, Flat 2 MVHR with one product, Flat 3 MVHR with another and supply-air cooling.</summary>
        private static AnalyticalModel OwnerSelection(AnalyticalModel baseline)
        {
            return WithStrategies(baseline, Natural(Flat1), Mvhr(Flat2, Small.VentilationUnitReference), Cooled(Flat3, Large.VentilationUnitReference));
        }

        private PartOMaterialisation MaterialiseOwner(AnalyticalModel analyticalModel)
        {
            PartOMaterialisation result = analyticalModel.MaterialisePartODwellingStrategies(Catalogue_Owner, null, [Cooling(Small), Cooling(Large)]);
            output.WriteLine(result.Refusal ?? "(materialised)");
            result.Notes.ForEach(output.WriteLine);

            return result;
        }

        private static AnalyticalModel Edited(AnalyticalModel analyticalModel, Action<AdjacencyCluster> action)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            action(adjacencyCluster);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        private static VentilationSystem SystemNamed(AdjacencyCluster adjacencyCluster, string fullName) => adjacencyCluster.GetObjects<VentilationSystem>().Single(x => x.FullName == fullName);

        private static AirHandlingUnit UnitNamed(AdjacencyCluster adjacencyCluster, string name) => adjacencyCluster.GetObjects<AirHandlingUnit>().Single(x => x.Name == name);

        private static Space SpaceOf(AdjacencyCluster adjacencyCluster, string name_Zone) => adjacencyCluster.GetRelatedObjects<Space>(adjacencyCluster.GetObjects<Zone>().Single(x => x.Name == name_Zone)).OrderBy(x => x.Name, StringComparer.Ordinal).First();

        private static void AssertSharedLegacyPlantRefused(PartOMaterialisation materialisation)
        {
            Assert.Null(materialisation.AnalyticalModel);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.SharedSystem && x.Subject == System_Legacy);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.SharedSystem && x.Subject == Unit_Legacy);
        }

        private static void AssertNoSharedSystem(PartOMaterialisation materialisation)
        {
            Assert.DoesNotContain(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.SharedSystem);
        }

        // =================================================================================================
        // The fixture
        // =================================================================================================

        [Fact]
        public void EffectiveDuty_TheScaffoldIsTheRealModelsShape_AndEveryTemplateIsInert()
        {
            AdjacencyCluster adjacencyCluster = LegacyScaffoldBaseline().AdjacencyCluster;

            Assert.Equal(["MV 1", "NV 1", "UV 1"], adjacencyCluster.GetObjects<VentilationSystem>().ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));
            Assert.Equal(Unit_Legacy, Assert.Single(adjacencyCluster.GetObjects<AirHandlingUnit>()).Name);

            //MV 1 names AHU1 and is related to rooms of BOTH Flat 2 and Flat 3 - the owner's shared legacy system.
            VentilationSystem ventilationSystem = SystemNamed(adjacencyCluster, System_Legacy);
            Assert.Equal(Unit_Legacy, ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName));
            Assert.Equal(Unit_Legacy, ventilationSystem.GetValue<string>(VentilationSystemParameter.ExhaustUnitName));
            HashSet<Guid> guids_Space = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem).Select(x => x.Guid).ToHashSet();
            foreach (string name_Zone in new[] { Flat2, Flat3 })
            {
                Assert.All(adjacencyCluster.GetRelatedObjects<Space>(adjacencyCluster.GetObjects<Zone>().Single(x => x.Name == name_Zone)), x => Assert.Contains(x.Guid, guids_Space));
            }

            //Nothing on it states duty: no terminal, no movement, no product; a template unit's default 23 degC summer supply.
            Assert.Empty(adjacencyCluster.GetObjects<VentilationTerminal>() ?? []);
            Assert.Empty(adjacencyCluster.GetObjects<SpaceAirMovement>() ?? []);
            Assert.Empty(adjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? []);
            Assert.Null(UnitNamed(adjacencyCluster, Unit_Legacy).SelectedVentilationUnitReference());

            foreach (VentilationSystem ventilationSystem_Template in adjacencyCluster.GetObjects<VentilationSystem>())
            {
                PartOAuthoredPlantDuty partOAuthoredPlantDuty = adjacencyCluster.PartOAuthoredPlantDuty(ventilationSystem_Template);
                Assert.True(partOAuthoredPlantDuty.IsInert, partOAuthoredPlantDuty.ToString());
                Assert.Equal(ventilationSystem_Template.FullName, partOAuthoredPlantDuty.Name);
            }

            Assert.Equal([Unit_Legacy], adjacencyCluster.PartOAuthoredPlantDuty(ventilationSystem).UnitNames);
            Assert.Empty(adjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(adjacencyCluster, "NV 1")).UnitNames);
            Assert.True(adjacencyCluster.PartOAuthoredPlantDuty(UnitNamed(adjacencyCluster, Unit_Legacy)).IsInert);
        }

        // =================================================================================================
        // 1. The inert shared scaffold passes
        // =================================================================================================

        /// <summary>
        /// <b>The owner's refusal, fixed.</b> Inert <c>MV 1</c>/<c>AHU1</c> across Flats 2 and 3 is scaffolding, not
        /// shared plant: the owner's selection materialises with an inert note, the scaffold stays on the model as
        /// authored, and the PR-1 Systems scope leaves out <c>NV 1</c>, <c>UV 1</c> and <c>MV 1</c>.
        /// </summary>
        [Fact]
        public void EffectiveDuty_InertSharedScaffold_Passes_WithAnInertNote_AndNoSharedSystem()
        {
            AnalyticalModel baseline = OwnerSelection(LegacyScaffoldBaseline());
            string json_Baseline = baseline.ToJsonObject().ToJsonString();

            PartOMaterialisation materialisation = MaterialiseOwner(baseline);

            Assert.True(materialisation.IsMaterialised, materialisation.Refusal);
            AssertNoSharedSystem(materialisation);
            Assert.Equal(PartOSimulationRoute.Systems, materialisation.Route);

            Assert.Contains(materialisation.Notes, x => x.Contains("'MV 1'") && x.Contains("'AHU1'") && x.Contains("inert") && x.Contains("not part of this assessment"));
            Assert.Contains(materialisation.Notes, x => x.Contains("'NV 1'") && x.Contains("template metadata"));

            //Left exactly as authored: the system keeps every room relation, the unit is not touched or reused.
            AdjacencyCluster adjacencyCluster_Baseline = baseline.AdjacencyCluster;
            AdjacencyCluster adjacencyCluster = materialisation.AnalyticalModel.AdjacencyCluster;
            VentilationSystem ventilationSystem_Baseline = SystemNamed(adjacencyCluster_Baseline, System_Legacy);
            VentilationSystem ventilationSystem = adjacencyCluster.GetObject<VentilationSystem>(ventilationSystem_Baseline.Guid);
            Assert.NotNull(ventilationSystem);
            Assert.Equal(
                adjacencyCluster_Baseline.GetRelatedObjects<Space>(ventilationSystem_Baseline).Select(x => x.Guid).OrderBy(x => x),
                adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem).Select(x => x.Guid).OrderBy(x => x));
            AirHandlingUnit airHandlingUnit = UnitNamed(adjacencyCluster, Unit_Legacy);
            Assert.Equal(UnitNamed(adjacencyCluster_Baseline, Unit_Legacy).Guid, airHandlingUnit.Guid);
            Assert.Null(airHandlingUnit.SelectedVentilationUnitReference());
            Assert.Empty(adjacencyCluster.GetRelatedObjects(airHandlingUnit) ?? []);

            //Each MVHR dwelling got its own Part O unit, neither of which is AHU1.
            Assert.Equal(2, materialisation.Record.VentilationSystemGuids.Count);
            Assert.DoesNotContain(materialisation.AirHandlingUnits, x => x.Name == Unit_Legacy);

            //PR-1's scope, unchanged: only the Part O systems go to SAM_Systems.
            PartOSystemsMaterialisationScope scope = SystemsScope(materialisation);
            Assert.True(scope.IsScoped, scope.Refusal);
            Assert.Equal(materialisation.Record.VentilationSystemGuids.Values.OrderBy(x => x), scope.Guids_Retained);
            Assert.Equal(["MV 1", "NV 1", "UV 1"], scope.Exclusions.Select(x => x.FullName_VentilationSystem).OrderBy(x => x, StringComparer.Ordinal));

            //The design model is not modified.
            Assert.Equal(json_Baseline, baseline.ToJsonObject().ToJsonString());
        }

        // =================================================================================================
        // 2-5. Any genuine duty makes it active, and the shared refusal stands
        // =================================================================================================

        [Fact]
        public void EffectiveDuty_TerminalDuty_OnTheSharedScaffold_StillRefusesSharedSystem()
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                VentilationTerminal ventilationTerminal = new("Flat 2 legacy supply", FlowClassification.Supply, 8.0);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, SpaceOf(adjacencyCluster, Flat2));
                adjacencyCluster.AddRelation(SystemNamed(adjacencyCluster, System_Legacy), ventilationTerminal);
            });

            PartOAuthoredPlantDuty partOAuthoredPlantDuty = baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy));
            PartOMechanicalDutyEvidence evidence = Assert.Single(partOAuthoredPlantDuty.Evidence);
            Assert.Equal(PartOMechanicalDutyEvidenceKind.TerminalDesignAirFlow, evidence.Kind);
            Assert.Equal(8.0, evidence.Value);
            Assert.Equal(System_Legacy, evidence.Name_Owner);

            //The unit is duty-bearing through the system that names it.
            Assert.False(baseline.AdjacencyCluster.PartOAuthoredPlantDuty(UnitNamed(baseline.AdjacencyCluster, Unit_Legacy)).IsInert);

            AssertSharedLegacyPlantRefused(MaterialiseOwner(OwnerSelection(baseline)));
        }

        [Theory]
        [InlineData("unit to room")]
        [InlineData("unit exhaust")]
        [InlineData("unit endpoint only")]
        [InlineData("system")]
        public void EffectiveDuty_AirMovement_OnTheSharedScaffold_IsActive_AndRefused(string shape)
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                AirHandlingUnit airHandlingUnit = UnitNamed(adjacencyCluster, Unit_Legacy);
                Space space = SpaceOf(adjacencyCluster, Flat3);
                string reference_Unit = new Core.ObjectReference(airHandlingUnit).ToString();
                string reference_Space = new Core.ObjectReference(space).ToString();

                SpaceAirMovement spaceAirMovement = shape switch
                {
                    "unit exhaust" => new SpaceAirMovement("AHU1 exhaust", 0.012, reference_Unit, null),
                    "system" => new SpaceAirMovement("MV 1 transfer", 0.004, reference_Space, new Core.ObjectReference(SpaceOf(adjacencyCluster, Flat2)).ToString()),
                    _ => new SpaceAirMovement("AHU1 supply", 0.006, reference_Unit, reference_Space),
                };

                adjacencyCluster.AddObject(spaceAirMovement);

                if (shape == "system")
                {
                    adjacencyCluster.AddRelation(spaceAirMovement, SystemNamed(adjacencyCluster, System_Legacy));
                }
                else if (shape != "unit endpoint only")
                {
                    adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                }

                if (shape != "unit exhaust")
                {
                    adjacencyCluster.AddRelation(spaceAirMovement, space);
                }
            });

            PartOAuthoredPlantDuty partOAuthoredPlantDuty = baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy));
            output.WriteLine(partOAuthoredPlantDuty.ToString());
            PartOMechanicalDutyEvidence evidence = Assert.Single(partOAuthoredPlantDuty.Evidence);
            Assert.Equal(PartOMechanicalDutyEvidenceKind.SpaceAirMovement, evidence.Kind);
            Assert.Equal(shape == "system" ? System_Legacy : Unit_Legacy, evidence.Name_Owner);

            AssertSharedLegacyPlantRefused(MaterialiseOwner(OwnerSelection(baseline)));
        }

        /// <summary>
        /// The unit's airflow. SAM stores none on an <see cref="AirHandlingUnit"/>: it is derived - from the systems'
        /// terminals (<c>AirHandlingUnitDesignDuty</c>, case 2) and from the unit's own movements (<c>Query.AirFlow</c>).
        /// The unit's supply condition (<see cref="AirHandlingUnitAirMovement"/>) states conditions, not an airflow, so
        /// on its own it is NOT duty (owner, PR-2): the scaffold is not shared plant, though the unchanged movement rule
        /// still refuses the condition itself beside an MVHR dwelling. Once a movement gives the unit a finite
        /// <c>Query.AirFlow</c>, that movement is the duty, and the shared refusal stands.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void EffectiveDuty_UnitSupplyCondition_IsDutyOnlyThroughAFiniteUnitAirflow(bool withFlow)
        {
            Guid guid_AirHandlingUnitAirMovement = Guid.Empty;
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                AirHandlingUnit airHandlingUnit = UnitNamed(adjacencyCluster, Unit_Legacy);
                AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(Unit_Legacy, new Profile("AHU1 Heating", ProfileType.Heating, [16.0]), null, null, null, null);
                adjacencyCluster.AddObject(airHandlingUnitAirMovement);
                adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);
                guid_AirHandlingUnitAirMovement = airHandlingUnitAirMovement.Guid;

                if (withFlow)
                {
                    Space space = SpaceOf(adjacencyCluster, Flat3);
                    SpaceAirMovement spaceAirMovement = new("AHU1 supply", 0.006, new Profile("AHU1 supply", ProfileType.Other, [1.0]), new Core.ObjectReference(airHandlingUnit).ToString(), new Core.ObjectReference(space).ToString());
                    adjacencyCluster.AddObject(spaceAirMovement);
                    adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
                    adjacencyCluster.AddRelation(spaceAirMovement, space);
                }
            });

            AdjacencyCluster adjacencyCluster_Baseline = baseline.AdjacencyCluster;
            double airFlow = adjacencyCluster_Baseline.AirFlow(adjacencyCluster_Baseline.GetObject<AirHandlingUnitAirMovement>(guid_AirHandlingUnitAirMovement), out _);
            Assert.Equal(withFlow, !double.IsNaN(airFlow));

            PartOAuthoredPlantDuty partOAuthoredPlantDuty = adjacencyCluster_Baseline.PartOAuthoredPlantDuty(UnitNamed(adjacencyCluster_Baseline, Unit_Legacy));
            output.WriteLine(partOAuthoredPlantDuty.ToString());
            Assert.Equal(!withFlow, partOAuthoredPlantDuty.IsInert);
            Assert.All(partOAuthoredPlantDuty.Evidence, x => Assert.Equal(PartOMechanicalDutyEvidenceKind.SpaceAirMovement, x.Kind));
            if (withFlow)
            {
                Assert.Equal(airFlow * 1000.0, Assert.Single(partOAuthoredPlantDuty.Evidence).Value, 9);
            }

            PartOMaterialisation materialisation = MaterialiseOwner(OwnerSelection(baseline));
            if (withFlow)
            {
                AssertSharedLegacyPlantRefused(materialisation);
            }
            else
            {
                AssertNoSharedSystem(materialisation);
                Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.AuthoredAirMovementConflict && x.Subject == Unit_Legacy);
            }
        }

        [Fact]
        public void EffectiveDuty_SelectedProduct_OnTheSharedUnit_IsActive_AndRefused()
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                AirHandlingUnit airHandlingUnit = new(UnitNamed(adjacencyCluster, Unit_Legacy));
                airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, Small.VentilationUnitReference);
                adjacencyCluster.AddObject(airHandlingUnit);
            });

            PartOAuthoredPlantDuty partOAuthoredPlantDuty = baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy));
            PartOMechanicalDutyEvidence evidence = Assert.Single(partOAuthoredPlantDuty.Evidence);
            Assert.Equal(PartOMechanicalDutyEvidenceKind.SelectedProduct, evidence.Kind);
            Assert.Equal(Unit_Legacy, evidence.Name_Owner);
            Assert.Contains("Small", evidence.Message);

            AssertSharedLegacyPlantRefused(MaterialiseOwner(OwnerSelection(baseline)));
        }

        // =================================================================================================
        // 6. NaN, zero and missing are not duty
        // =================================================================================================

        [Theory]
        [InlineData(0.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(null)]
        public void EffectiveDuty_TerminalWithoutAFiniteNonZeroAirflow_IsNotDuty_AndTheScaffoldPasses(double? designFlowRate_Lps)
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                VentilationTerminal ventilationTerminal = new("Flat 2 legacy supply", FlowClassification.Supply, designFlowRate_Lps);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, SpaceOf(adjacencyCluster, Flat2));
                adjacencyCluster.AddRelation(SystemNamed(adjacencyCluster, System_Legacy), ventilationTerminal);
            });

            Assert.True(baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy)).IsInert);

            //Under the owner's selection the stray terminal is not plant. It is still a terminal in MVHR Flat 2 that
            //realises no Part F requirement, and the unchanged design-basis rule refuses THAT.
            PartOMaterialisation materialisation = MaterialiseOwner(OwnerSelection(baseline));
            AssertNoSharedSystem(materialisation);
            Assert.All(materialisation.Refusals, x => Assert.Equal(PartOMaterialisationRefusalReason.DesignDiffersFromRequirement, x.Reason));

            //With Flat 2 natural the terminal is copied through inert, and the scaffold passes. Not for +-infinity: the
            //baseline fingerprint is its JSON, which cannot hold an infinite number, so such a model cannot even be
            //saved - a pre-existing limit PR-2 does not touch.
            if (designFlowRate_Lps.HasValue && double.IsInfinity(designFlowRate_Lps.Value))
            {
                return;
            }

            materialisation = MaterialiseOwner(WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Cooled(Flat3, Large.VentilationUnitReference)));
            Assert.True(materialisation.IsMaterialised, materialisation.Refusal);
            Assert.Contains(materialisation.Notes, x => x.Contains("'MV 1'") && x.Contains("inert"));
        }

        /// <summary>Same test as the Systems scope: a negative airflow is stated, so it is duty (PR-1 parity).</summary>
        [Fact]
        public void EffectiveDuty_TerminalWithANegativeAirflow_IsDuty_AsTheSystemsScopeReadsIt()
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                VentilationTerminal ventilationTerminal = new("Flat 2 legacy extract", FlowClassification.Extract, -6.0);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, SpaceOf(adjacencyCluster, Flat2));
                adjacencyCluster.AddRelation(SystemNamed(adjacencyCluster, System_Legacy), ventilationTerminal);
            });

            Assert.False(baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy)).IsInert);
            AssertSharedLegacyPlantRefused(MaterialiseOwner(OwnerSelection(baseline)));
        }

        /// <summary>
        /// A movement that moves nothing is not duty, so it does not make the scaffold shared plant. It is still an
        /// authored movement on a unit reaching an MVHR dwelling, and the unchanged movement rule refuses THAT.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(double.NaN)]
        public void EffectiveDuty_AirMovementWithoutAFiniteNonZeroAirflow_IsNotDuty(double airFlow)
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                AirHandlingUnit airHandlingUnit = UnitNamed(adjacencyCluster, Unit_Legacy);
                SpaceAirMovement spaceAirMovement = new("AHU1 exhaust", airFlow, new Core.ObjectReference(airHandlingUnit).ToString(), null);
                adjacencyCluster.AddObject(spaceAirMovement);
                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
            });

            Assert.True(baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy)).IsInert);
            Assert.True(baseline.AdjacencyCluster.PartOAuthoredPlantDuty(UnitNamed(baseline.AdjacencyCluster, Unit_Legacy)).IsInert);

            PartOMaterialisation materialisation = MaterialiseOwner(OwnerSelection(baseline));
            AssertNoSharedSystem(materialisation);
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.AuthoredAirMovementConflict && x.Subject == "AHU1 exhaust");
        }

        // =================================================================================================
        // 7. The normal topology: one unit per dwelling
        // =================================================================================================

        /// <summary>
        /// Flat 1 natural and the corridor uncontrolled as before; Flats 2 and 3 each get their OWN <c>MV</c> system and
        /// unit from <c>AddMechanicalSystems</c>, each connected to that flat's design terminals, each with its own
        /// selected product and no authored supply temperature.
        /// </summary>
        private static AnalyticalModel OneUnitPerDwellingBaseline(bool connectTerminals = true)
        {
            AnalyticalModel analyticalModel = Baseline();
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            Core.SystemTypeLibrary systemTypeLibrary = Analytical.Query.DefaultSystemTypeLibrary();

            Dictionary<string, List<Space>> dictionary_Spaces = [];
            Dictionary<Guid, string> dictionary_Template = [];
            foreach (Zone zone in adjacencyCluster.GetObjects<Zone>())
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(zone) ?? [];
                dictionary_Spaces[zone.Name] = spaces;
                spaces.ForEach(x => dictionary_Template[x.Guid] = zone.Name == Flat1 ? "NV" : zone.Name == Corridor ? "UV" : "MV");
            }

            SetTemplates(adjacencyCluster, dictionary_Template);

            Assert.NotEmpty(adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, [.. dictionary_Spaces[Flat1], .. dictionary_Spaces[Corridor]]));

            foreach ((string name_Zone, VentilationUnitCapacityDescriptor descriptor) in new[] { (Flat2, Small), (Flat3, Large) })
            {
                string name_Unit = "AHU " + name_Zone;
                Assert.NotEmpty(adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, dictionary_Spaces[name_Zone], name_Unit, name_Unit));

                AirHandlingUnit airHandlingUnit = new(UnitNamed(adjacencyCluster, name_Unit)) { SummerSupplyTemperature = double.NaN, WinterSupplyTemperature = double.NaN };
                airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, descriptor.VentilationUnitReference);
                adjacencyCluster.AddObject(airHandlingUnit);

                if (!connectTerminals)
                {
                    continue;
                }

                VentilationSystem ventilationSystem = adjacencyCluster.GetObjects<VentilationSystem>().Single(x => x.GetValue<string>(VentilationSystemParameter.SupplyUnitName) == name_Unit);
                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.RealizePartFVentilationTerminals(dictionary_Spaces[name_Zone], out _, out List<string> refusals);
                Assert.Empty(refusals);
                ventilationTerminals.ForEach(x => adjacencyCluster.AddRelation(ventilationSystem, x));
            }

            AnalyticalModel result = new(analyticalModel, adjacencyCluster);
            Assert.True(result.IsPartOCleanBaseline(out List<PartOMaterialisationRefusal> findings), string.Join("\n", findings));

            return result;
        }

        [Fact]
        public void EffectiveDuty_OneUnitPerDwelling_EachCarriesItsOwnDuty_AndNoneIsShared()
        {
            AnalyticalModel baseline = OneUnitPerDwellingBaseline();
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            Assert.Equal(["MV 1", "MV 2", "NV 1", "UV 1"], adjacencyCluster.GetObjects<VentilationSystem>().ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));

            //Each system is active on its own terminals and its own unit's product, and on nothing of the other flat's.
            foreach ((string name_System, string name_Unit, string model) in new[] { ("MV 1", "AHU Flat 2", "Small"), ("MV 2", "AHU Flat 3", "Large") })
            {
                PartOAuthoredPlantDuty partOAuthoredPlantDuty = adjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(adjacencyCluster, name_System));
                output.WriteLine(partOAuthoredPlantDuty.ToString());

                Assert.False(partOAuthoredPlantDuty.IsInert);
                Assert.Equal([name_Unit], partOAuthoredPlantDuty.UnitNames);
                Assert.All(partOAuthoredPlantDuty.Evidence, x => Assert.Contains(x.Name_Owner, new[] { name_System, name_Unit }));
                Assert.Contains(partOAuthoredPlantDuty.Evidence, x => x.Kind == PartOMechanicalDutyEvidenceKind.TerminalDesignAirFlow);
                Assert.Contains(partOAuthoredPlantDuty.Evidence, x => x.Kind == PartOMechanicalDutyEvidenceKind.SelectedProduct && x.Message.Contains(model));
            }

            PartOMaterialisation materialisation = MaterialiseOwner(OwnerSelection(baseline));

            AssertNoSharedSystem(materialisation);
            Assert.True(materialisation.IsMaterialised, materialisation.Refusal);

            //Each dwelling is designed on its OWN authored unit, which carries its own product.
            AnalyticalModel model_Materialised = materialisation.AnalyticalModel;
            Assert.Equal("AHU Flat 2", UnitOf(model_Materialised, Flat2).Name);
            Assert.Equal("AHU Flat 3", UnitOf(model_Materialised, Flat3).Name);
            Assert.Equal("Small", UnitOf(model_Materialised, Flat2).SelectedVentilationUnitReference()?.Model);
            Assert.Equal("Large", UnitOf(model_Materialised, Flat3).SelectedVentilationUnitReference()?.Model);
        }

        /// <summary>
        /// Two dwellings' systems on ONE unit: the unit is shared plant as soon as either system states duty, and the
        /// other system is then duty-bearing through it. Inert on both, it is scaffolding like <c>AHU1</c>.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EffectiveDuty_TwoDwellingSystemsOnOneUnit_AreSharedPlant_OnlyWhenEitherStatesDuty(bool duty)
        {
            const string name_Unit = "AHU shared";

            AnalyticalModel analyticalModel = Baseline();
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            Core.SystemTypeLibrary systemTypeLibrary = Analytical.Query.DefaultSystemTypeLibrary();

            Dictionary<Guid, string> dictionary_Template = [];
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                dictionary_Template[space.Guid] = "MV";
            }

            SetTemplates(adjacencyCluster, dictionary_Template);

            foreach (string name_Zone in new[] { Flat2, Flat3 })
            {
                Assert.NotEmpty(adjacencyCluster.AddMechanicalSystems(systemTypeLibrary, adjacencyCluster.GetRelatedObjects<Space>(adjacencyCluster.GetObjects<Zone>().Single(x => x.Name == name_Zone)), name_Unit, name_Unit));
            }

            if (duty)
            {
                VentilationTerminal ventilationTerminal = new("Flat 2 authored supply", FlowClassification.Supply, 8.0);
                adjacencyCluster.AddObject(ventilationTerminal);
                adjacencyCluster.AddRelation(ventilationTerminal, SpaceOf(adjacencyCluster, Flat2));
                adjacencyCluster.AddRelation(SystemNamed(adjacencyCluster, "MV 1"), ventilationTerminal);
            }

            AnalyticalModel baseline = new(analyticalModel, adjacencyCluster);

            //Flat 3's system states nothing itself; with duty it is active through the shared unit, named as the owner.
            PartOAuthoredPlantDuty partOAuthoredPlantDuty = baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, "MV 2"));
            output.WriteLine(partOAuthoredPlantDuty.ToString());
            Assert.Equal(!duty, partOAuthoredPlantDuty.IsInert);
            Assert.All(partOAuthoredPlantDuty.Evidence, x => Assert.Equal("MV 1", x.Name_Owner));

            PartOMaterialisation materialisation = MaterialiseOwner(WithStrategies(baseline, Mvhr(Flat1), Mvhr(Flat2, Small.VentilationUnitReference), Cooled(Flat3, Large.VentilationUnitReference)));

            if (duty)
            {
                Assert.Null(materialisation.AnalyticalModel);
                Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.SharedSystem && x.Subject == name_Unit && x.Message.Contains("'Flat 2'") && x.Message.Contains("'Flat 3'"));
            }
            else
            {
                AssertNoSharedSystem(materialisation);
                Assert.True(materialisation.IsMaterialised, materialisation.Refusal);
                Assert.Equal(2, materialisation.Notes.Count(x => x.Contains("'AHU shared'") && x.Contains("inert")));
            }
        }

        // =================================================================================================
        // 8. Single-dwelling authored plant keeps the active-plant refusals
        // =================================================================================================

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EffectiveDuty_SingleDwellingPlantWithAProduct_NotConnectedToTheDesign_StillRefusesAsUnconnected_OrAsNaturalOverMechanical(bool mvhr)
        {
            AnalyticalModel baseline = OneUnitPerDwellingBaseline(connectTerminals: false);

            AnalyticalModel analyticalModel = mvhr
                ? WithStrategies(baseline, Natural(Flat1), Mvhr(Flat2, Small.VentilationUnitReference), Natural(Flat3))
                : WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Natural(Flat3));

            PartOMaterialisation materialisation = MaterialiseOwner(analyticalModel);

            Assert.Null(materialisation.AnalyticalModel);
            AssertNoSharedSystem(materialisation);
            Assert.Contains(materialisation.Refusals, x => x.Reason == (mvhr ? PartOMaterialisationRefusalReason.UnconnectedAuthoredPlant : PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty) && x.Subject == "MV 1");
            Assert.Contains(materialisation.Refusals, x => x.Reason == PartOMaterialisationRefusalReason.NaturalOverMechanicalDuty && x.Subject == "MV 2");
        }

        /// <summary>
        /// The same single-dwelling plant WITHOUT a product states nothing: before PR-2 an existing unit alone refused
        /// it; now it is inert scaffolding, noted and left alone - whichever strategy the dwelling has.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EffectiveDuty_SingleDwellingPlant_Inert_IsNoted_NotRefused(bool mvhr)
        {
            AnalyticalModel baseline = Edited(OneUnitPerDwellingBaseline(connectTerminals: false), adjacencyCluster =>
            {
                foreach (AirHandlingUnit airHandlingUnit in adjacencyCluster.GetObjects<AirHandlingUnit>())
                {
                    AirHandlingUnit airHandlingUnit_Bare = new(airHandlingUnit);
                    airHandlingUnit_Bare.RemoveValue(AirHandlingUnitParameter.VentilationUnitReference);
                    adjacencyCluster.AddObject(airHandlingUnit_Bare);
                }
            });

            AnalyticalModel analyticalModel = mvhr
                ? WithStrategies(baseline, Natural(Flat1), Mvhr(Flat2, Small.VentilationUnitReference), Mvhr(Flat3, Large.VentilationUnitReference))
                : WithStrategies(baseline, Natural(Flat1), Natural(Flat2), Natural(Flat3));

            PartOMaterialisation materialisation = MaterialiseOwner(analyticalModel);

            Assert.True(materialisation.IsMaterialised, materialisation.Refusal);
            Assert.Contains(materialisation.Notes, x => x.Contains("'MV 1'") && x.Contains("'AHU Flat 2'") && x.Contains("inert"));
            Assert.Contains(materialisation.Notes, x => x.Contains("'MV 2'") && x.Contains("'AHU Flat 3'") && x.Contains("inert"));
        }

        // =================================================================================================
        // 9. NV / UV stay non-mechanical
        // =================================================================================================

        [Fact]
        public void EffectiveDuty_NaturalAndUncontrolledTemplates_NameNoUnit_AndStayTemplateMetadata()
        {
            AnalyticalModel baseline = OwnerSelection(LegacyScaffoldBaseline());
            AdjacencyCluster adjacencyCluster = baseline.AdjacencyCluster;

            foreach (string name_System in new[] { "NV 1", "UV 1" })
            {
                VentilationSystem ventilationSystem = SystemNamed(adjacencyCluster, name_System);
                Assert.False(ventilationSystem.IsMechanicalVentilation());

                PartOAuthoredPlantDuty partOAuthoredPlantDuty = adjacencyCluster.PartOAuthoredPlantDuty(ventilationSystem);
                Assert.True(partOAuthoredPlantDuty.IsInert);
                Assert.Empty(partOAuthoredPlantDuty.UnitNames);
            }

            PartOMaterialisation materialisation = MaterialiseOwner(baseline);
            Assert.True(materialisation.IsMaterialised, materialisation.Refusal);

            //NV 1 serves the assessed natural Flat 1: the pre-PR-2 note, word for word. UV 1 serves only the corridor,
            //which is no dwelling, so it is not judged at all.
            Assert.Contains("Ventilation system 'NV 1' (NV) is related to assessed spaces but carries no design duty and no unit, so it is template metadata: left exactly as authored.", materialisation.Notes);
            Assert.DoesNotContain(materialisation.Notes, x => x.Contains("'UV 1'"));
            Assert.Equal(PartOIteration.BaseNaturalVentilation, materialisation.OverheatingScenarios.Single(x => x.ZoneGuid == Zone(baseline, Flat1).Guid).Iteration);
        }

        // =================================================================================================
        // The query itself
        // =================================================================================================

        [Fact]
        public void EffectiveDuty_Query_NullArguments_ReturnNull()
        {
            AdjacencyCluster adjacencyCluster = LegacyScaffoldBaseline().AdjacencyCluster;

            Assert.Null(Analytical.Query.PartOAuthoredPlantDuty(null, SystemNamed(adjacencyCluster, System_Legacy)));
            Assert.Null(Analytical.Query.PartOAuthoredPlantDuty(null, UnitNamed(adjacencyCluster, Unit_Legacy)));
            Assert.Null(adjacencyCluster.PartOAuthoredPlantDuty((VentilationSystem)null));
            Assert.Null(adjacencyCluster.PartOAuthoredPlantDuty((AirHandlingUnit)null));
        }

        [Fact]
        public void EffectiveDuty_Query_EveryKindOfEvidence_IsReportedOnce_InAStableOrder()
        {
            AnalyticalModel baseline = Edited(LegacyScaffoldBaseline(), adjacencyCluster =>
            {
                AirHandlingUnit airHandlingUnit = new(UnitNamed(adjacencyCluster, Unit_Legacy));
                airHandlingUnit.SetValue(AirHandlingUnitParameter.VentilationUnitReference, Large.VentilationUnitReference);
                adjacencyCluster.AddObject(airHandlingUnit);

                VentilationSystem ventilationSystem = SystemNamed(adjacencyCluster, System_Legacy);
                foreach ((string name, double flow) in new[] { ("b terminal", 4.0), ("a terminal", 3.0) })
                {
                    VentilationTerminal ventilationTerminal = new(name, FlowClassification.Supply, flow);
                    adjacencyCluster.AddObject(ventilationTerminal);
                    adjacencyCluster.AddRelation(ventilationTerminal, SpaceOf(adjacencyCluster, Flat2));
                    adjacencyCluster.AddRelation(ventilationSystem, ventilationTerminal);
                }

                AirHandlingUnitAirMovement airHandlingUnitAirMovement = new(Unit_Legacy);
                adjacencyCluster.AddObject(airHandlingUnitAirMovement);
                adjacencyCluster.AddRelation(airHandlingUnit, airHandlingUnitAirMovement);

                //Related to the unit AND naming it as an endpoint: one movement, one piece of evidence.
                SpaceAirMovement spaceAirMovement = new("AHU1 exhaust", 0.01, new Core.ObjectReference(airHandlingUnit).ToString(), null);
                adjacencyCluster.AddObject(spaceAirMovement);
                adjacencyCluster.AddRelation(spaceAirMovement, airHandlingUnit);
            });

            PartOAuthoredPlantDuty partOAuthoredPlantDuty = baseline.AdjacencyCluster.PartOAuthoredPlantDuty(SystemNamed(baseline.AdjacencyCluster, System_Legacy));
            output.WriteLine(partOAuthoredPlantDuty.ToString());

            Assert.Equal(
                [PartOMechanicalDutyEvidenceKind.TerminalDesignAirFlow, PartOMechanicalDutyEvidenceKind.TerminalDesignAirFlow, PartOMechanicalDutyEvidenceKind.SelectedProduct, PartOMechanicalDutyEvidenceKind.SpaceAirMovement],
                partOAuthoredPlantDuty.Evidence.Select(x => x.Kind));
            Assert.Equal(["a terminal", "b terminal"], partOAuthoredPlantDuty.Evidence.Take(2).Select(x => x.Name));
            Assert.Equal(10.0, partOAuthoredPlantDuty.Evidence.Last().Value, 9);
            Assert.Contains("active", partOAuthoredPlantDuty.ToString());
        }
    }
}
