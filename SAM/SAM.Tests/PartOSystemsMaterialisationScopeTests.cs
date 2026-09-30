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
    /// <c>Query.PartOSystemsMaterialisationScope</c> (SAM_UI PR-1): the one identity-based rule deciding which
    /// ventilation systems a Part O Systems-route materialisation is handed. It was Iteration 3's rule in SAM_UI; these
    /// tests pin its semantics in SAM, where Iteration 3 and Mixed Design now both read it.
    /// <para>
    /// The model here is the <c>Modify.AddMechanicalSystems</c> shape a real project reaches Part O with: natural and
    /// uncontrolled systems that name no air handling unit, a template mechanical system with its unit, and cooling
    /// and heating template systems - beside the systems Part O built.
    /// </para>
    /// </summary>
    public class PartOSystemsMaterialisationScopeTests
    {
        private sealed class Fixture
        {
            public AdjacencyCluster AdjacencyCluster { get; } = new();

            public List<Guid> Guids_Built { get; } = [];

            public List<Space> Spaces_Dwelling { get; } = [];

            public Space Space_Corridor { get; set; }

            public VentilationSystem VentilationSystem_NV { get; set; }

            public VentilationSystem VentilationSystem_UV { get; set; }

            public VentilationSystem VentilationSystem_MV { get; set; }

            public List<Guid> Guids_Space_Dwelling => Spaces_Dwelling.ConvertAll(x => x.Guid);
        }

        private static Space Space(AdjacencyCluster adjacencyCluster, string name)
        {
            Space result = new(name);
            result.SetValue(SpaceParameter.Area, 10.0);
            result.SetValue(SpaceParameter.Volume, 25.0);
            adjacencyCluster.AddObject(result);

            return result;
        }

        private static VentilationSystem VentilationSystem_(AdjacencyCluster adjacencyCluster, string type, int index, string name_Unit)
        {
            VentilationSystem result = Create.MechanicalSystem(Create.VentilationSystemType(Guid.NewGuid(), type, type), null, index) as VentilationSystem;

            if (name_Unit is not null)
            {
                if ((adjacencyCluster.GetObjects<AirHandlingUnit>() ?? []).Find(x => x.Name == name_Unit) is null)
                {
                    adjacencyCluster.AddObject(Create.AirHandlingUnit(name_Unit));
                }

                result.SetValue(VentilationSystemParameter.SupplyUnitName, name_Unit);
                result.SetValue(VentilationSystemParameter.ExhaustUnitName, name_Unit);
            }

            adjacencyCluster.AddObject(result);

            return result;
        }

        private static VentilationTerminal Terminal(AdjacencyCluster adjacencyCluster, VentilationSystem ventilationSystem, Space space, FlowClassification flowClassification, double? designFlowRate_Lps)
        {
            VentilationTerminal result = new(string.Format("{0} {1}", space?.Name ?? "orphan", flowClassification), flowClassification, designFlowRate_Lps);
            adjacencyCluster.AddObject(result);
            adjacencyCluster.AddRelation(ventilationSystem, result);

            if (space is not null)
            {
                adjacencyCluster.AddRelation(result, space);
            }

            return result;
        }

        /// <summary>
        /// Two dwellings with a Part O MVHR system each (<paramref name="type_Built"/>, one unit each), and the
        /// <c>AddMechanicalSystems</c> scaffolding: <c>NV 1</c> over the dwellings and <c>UV 1</c> over the corridor
        /// with no unit, <c>MV 1</c> naming <c>AHU1</c> over the corridor, and cooling <c>AHU 1</c> / <c>FCU 1</c> and
        /// heating <c>RAD 1</c> templates. The scaffolding carries no design terminal, as the template builds it.
        /// </summary>
        private static Fixture Scaffolded(string type_Built = "MVHR", string type_NV = "NV", string type_UV = "UV", string type_MV = "MV")
        {
            Fixture result = new();
            AdjacencyCluster adjacencyCluster = result.AdjacencyCluster;

            for (int i = 1; i <= 2; i++)
            {
                VentilationSystem ventilationSystem = VentilationSystem_(adjacencyCluster, type_Built, i, string.Format("MVHR Flat {0}", i));

                Space space_Bedroom = Space(adjacencyCluster, "Bedroom");
                Space space_Bathroom = Space(adjacencyCluster, "Bathroom");
                Terminal(adjacencyCluster, ventilationSystem, space_Bedroom, FlowClassification.Supply, 13.0);
                Terminal(adjacencyCluster, ventilationSystem, space_Bathroom, FlowClassification.Extract, 13.0);
                adjacencyCluster.AddRelation(ventilationSystem, space_Bedroom);
                adjacencyCluster.AddRelation(ventilationSystem, space_Bathroom);

                Zone zone = new(string.Format("Flat {0}", i));
                adjacencyCluster.AddObject(zone);
                adjacencyCluster.AddRelation(zone, space_Bedroom);
                adjacencyCluster.AddRelation(zone, space_Bathroom);

                result.Spaces_Dwelling.Add(space_Bedroom);
                result.Spaces_Dwelling.Add(space_Bathroom);
                result.Guids_Built.Add(ventilationSystem.Guid);
            }

            result.Space_Corridor = Space(adjacencyCluster, "Corridor");

            result.VentilationSystem_NV = VentilationSystem_(adjacencyCluster, type_NV, 1, null);
            result.Spaces_Dwelling.ForEach(x => adjacencyCluster.AddRelation(result.VentilationSystem_NV, x));

            result.VentilationSystem_UV = VentilationSystem_(adjacencyCluster, type_UV, 1, null);
            adjacencyCluster.AddRelation(result.VentilationSystem_UV, result.Space_Corridor);

            result.VentilationSystem_MV = VentilationSystem_(adjacencyCluster, type_MV, 1, "AHU1");
            adjacencyCluster.AddRelation(result.VentilationSystem_MV, result.Space_Corridor);

            foreach (MechanicalSystem mechanicalSystem in new MechanicalSystem[]
            {
                Create.MechanicalSystem(new CoolingSystemType("AHU", "AHU"), null, 1),
                Create.MechanicalSystem(new CoolingSystemType("FCU", "FCU"), null, 1),
                Create.MechanicalSystem(new HeatingSystemType("RAD", "RAD"), null, 1),
            })
            {
                adjacencyCluster.AddObject(mechanicalSystem);
                result.Spaces_Dwelling.ForEach(x => adjacencyCluster.AddRelation(mechanicalSystem, x));
            }

            return result;
        }

        private static string Json(AdjacencyCluster adjacencyCluster)
        {
            return adjacencyCluster.ToJsonObject().ToJsonString(new System.Text.Json.JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
                TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
            });
        }

        private static List<Guid> Sorted(IEnumerable<Guid> guids)
        {
            List<Guid> result = [.. guids];
            result.Sort();
            return result;
        }

        // =================================================================================================
        // The production shape
        // =================================================================================================

        [Fact]
        public void NV_and_UV_without_a_unit_are_left_out_and_every_retained_system_names_a_unit()
        {
            Fixture fixture = Scaffolded();

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped, scope.Refusal);
            Assert.Empty(scope.Refusals);
            Assert.Null(scope.Refusal);

            Assert.Equal(Sorted(fixture.Guids_Built), scope.Guids_Retained);
            Assert.Equal(Sorted([fixture.VentilationSystem_NV.Guid, fixture.VentilationSystem_UV.Guid, fixture.VentilationSystem_MV.Guid]), scope.Guids_Removed);

            //The working copy is exactly the Part O design, and each of its systems resolves its unit - the shape
            //SAM_Systems' unit resolution requires of every ventilation system it is handed.
            List<VentilationSystem> ventilationSystems = scope.AdjacencyCluster.GetObjects<VentilationSystem>();
            Assert.Equal(Sorted(fixture.Guids_Built), Sorted(ventilationSystems.ConvertAll(x => x.Guid)));
            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                string name_Unit = ventilationSystem.GetValue<string>(VentilationSystemParameter.SupplyUnitName);
                Assert.Contains(scope.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => x.Name == name_Unit);
            }

            Assert.DoesNotContain(ventilationSystems, x => x.Guid == fixture.VentilationSystem_NV.Guid || x.Guid == fixture.VentilationSystem_UV.Guid);

            //One structured exclusion per system left out, each saying why, then a summary note.
            Assert.Equal(3, scope.Exclusions.Count);
            Assert.All(scope.Exclusions, x => Assert.Equal(0, x.Count_VentilationTerminal));
            Assert.All(scope.Exclusions, x => Assert.Contains("no design ventilation terminal", x.Message));
            Assert.Equal(4, scope.Notes.Count);
            Assert.Contains("2 ventilation system(s) built by Part O", scope.Notes[^1]);
        }

        /// <summary>The engineer's model is never altered, including its NV/UV/MV scaffolding and its unit.</summary>
        [Fact]
        public void The_model_supplied_is_not_modified()
        {
            Fixture fixture = Scaffolded();
            string json = Json(fixture.AdjacencyCluster);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped);
            Assert.NotSame(fixture.AdjacencyCluster, scope.AdjacencyCluster);
            Assert.Equal(json, Json(fixture.AdjacencyCluster));
            Assert.Equal(5, fixture.AdjacencyCluster.GetObjects<VentilationSystem>().Count);
            Assert.Contains(fixture.AdjacencyCluster.GetObjects<AirHandlingUnit>(), x => x.Name == "AHU1");
        }

        /// <summary>
        /// Cooling and heating templates (<c>AHU 1</c>, <c>FCU 1</c>, <c>RAD 1</c>) are not ventilation systems: the scope
        /// neither consumes nor removes them, and they stay in the working copy exactly as authored.
        /// </summary>
        [Fact]
        public void Cooling_and_heating_templates_are_neither_consumed_nor_removed()
        {
            Fixture fixture = Scaffolded();
            List<Guid> guids_Template = [.. fixture.AdjacencyCluster.GetObjects<CoolingSystem>().ConvertAll(x => x.Guid), .. fixture.AdjacencyCluster.GetObjects<HeatingSystem>().ConvertAll(x => x.Guid)];
            Assert.Equal(3, guids_Template.Count);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped);
            Assert.DoesNotContain(scope.Guids_Retained, guids_Template.Contains);
            Assert.DoesNotContain(scope.Guids_Removed, guids_Template.Contains);
            Assert.DoesNotContain(scope.Exclusions, x => guids_Template.Contains(x.Guid_VentilationSystem));

            List<Guid> guids_Working = [.. scope.AdjacencyCluster.GetObjects<CoolingSystem>().ConvertAll(x => x.Guid), .. scope.AdjacencyCluster.GetObjects<HeatingSystem>().ConvertAll(x => x.Guid)];
            Assert.Equal(Sorted(guids_Template), Sorted(guids_Working));
            Assert.Equal(["AHU 1", "FCU 1"], scope.AdjacencyCluster.GetObjects<CoolingSystem>().ConvertAll(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal));
        }

        // =================================================================================================
        // Identity, never names
        // =================================================================================================

        /// <summary>
        /// The Part O systems are given the labels the scaffolding normally has and the scaffolding the label Part O's
        /// normally has. Nothing changes: membership follows the guids supplied.
        /// </summary>
        [Fact]
        public void Scope_follows_identity_not_familiar_labels()
        {
            Fixture fixture = Scaffolded(type_Built: "NV", type_NV: "MVHR", type_UV: "MVHR", type_MV: "MVHR");

            Assert.Contains(fixture.AdjacencyCluster.GetObjects<VentilationSystem>(), x => x.FullName == "NV 1" && fixture.Guids_Built.Contains(x.Guid));
            Assert.Contains(fixture.AdjacencyCluster.GetObjects<VentilationSystem>(), x => x.FullName == "MVHR 1" && !fixture.Guids_Built.Contains(x.Guid));

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped, scope.Refusal);
            Assert.Equal(Sorted(fixture.Guids_Built), scope.Guids_Retained);
            Assert.Equal(Sorted([fixture.VentilationSystem_NV.Guid, fixture.VentilationSystem_UV.Guid, fixture.VentilationSystem_MV.Guid]), scope.Guids_Removed);
        }

        [Fact]
        public void Two_systems_with_the_same_name_are_told_apart_by_identity()
        {
            Fixture fixture = Scaffolded();
            VentilationSystem ventilationSystem_Twin = VentilationSystem_(fixture.AdjacencyCluster, "MVHR", 1, "MVHR Flat 1");
            Assert.Equal(2, fixture.AdjacencyCluster.GetObjects<VentilationSystem>().Count(x => x.FullName == ventilationSystem_Twin.FullName));

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped);
            Assert.Contains(ventilationSystem_Twin.Guid, scope.Guids_Removed);
            Assert.DoesNotContain(ventilationSystem_Twin.Guid, scope.Guids_Retained);
        }

        // =================================================================================================
        // Authored effective duty keeps refusing
        // =================================================================================================

        [Fact]
        public void An_authored_mechanical_duty_outside_the_dwellings_refuses_with_its_identities()
        {
            Fixture fixture = Scaffolded();
            VentilationTerminal ventilationTerminal = Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, fixture.Space_Corridor, FlowClassification.Supply, 30.0);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.False(scope.IsScoped);
            Assert.Null(scope.AdjacencyCluster);
            Assert.Empty(scope.Guids_Retained);
            Assert.Empty(scope.Guids_Removed);
            Assert.Empty(scope.Exclusions);
            Assert.Empty(scope.Notes);

            PartOSystemsScopeRefusal refusal = Assert.Single(scope.Refusals);
            Assert.Equal(PartOSystemsScopeRefusalReason.DutyOutsideDwellingScope, refusal.Reason);
            Assert.Equal(fixture.VentilationSystem_MV.Guid, refusal.Guid_VentilationSystem);
            Assert.Equal("MV", refusal.Name_VentilationSystem);
            Assert.Equal("MV 1", refusal.FullName_VentilationSystem);
            Assert.Equal(ventilationTerminal.Guid, refusal.Guid_VentilationTerminal);
            Assert.Equal(FlowClassification.Supply, refusal.FlowClassification);
            Assert.Equal(30.0, refusal.DesignFlowRate_Lps);
            Assert.Equal(fixture.Space_Corridor.Guid, refusal.Guid_Space);
            Assert.Contains("outside the assessed dwellings", refusal.Message);
            Assert.Contains("30 l/s", refusal.Message);
            Assert.Contains(fixture.Space_Corridor.Guid.ToString(), refusal.Message);
            Assert.Equal(refusal.Message, scope.Refusal);
        }

        [Fact]
        public void An_authored_mechanical_duty_inside_the_dwellings_refuses_as_a_second_design()
        {
            Fixture fixture = Scaffolded();
            Space space = fixture.Spaces_Dwelling[1];
            Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, space, FlowClassification.Extract, 9.0);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            PartOSystemsScopeRefusal refusal = Assert.Single(scope.Refusals);
            Assert.Equal(PartOSystemsScopeRefusalReason.DutyInsideDwellingScope, refusal.Reason);
            Assert.Equal(space.Guid, refusal.Guid_Space);
            Assert.Contains("second mechanical ventilation design", refusal.Message);
        }

        /// <summary>The duty rule does not read the system type: a stated airflow on an NV system still refuses.</summary>
        [Fact]
        public void A_stated_airflow_on_a_natural_system_still_counts_as_duty()
        {
            Fixture fixture = Scaffolded();
            Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_NV, fixture.Space_Corridor, FlowClassification.Supply, 5.0);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.Equal(fixture.VentilationSystem_NV.Guid, Assert.Single(scope.Refusals).Guid_VentilationSystem);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void A_terminal_with_no_effective_airflow_is_not_duty(double? designFlowRate_Lps)
        {
            Fixture fixture = Scaffolded();
            Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, fixture.Space_Corridor, FlowClassification.Supply, designFlowRate_Lps);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.True(scope.IsScoped);
            PartOSystemsScopeExclusion exclusion = Assert.Single(scope.Exclusions, x => x.Guid_VentilationSystem == fixture.VentilationSystem_MV.Guid);
            Assert.Equal(1, exclusion.Count_VentilationTerminal);
            Assert.Contains("none of which states an effective design airflow", exclusion.Message);
        }

        [Theory]
        [InlineData(FlowClassification.Undefined, 5.0)]
        [InlineData(FlowClassification.Supply, -4.0)]
        [InlineData(FlowClassification.Extract, 0.001)]
        public void Any_stated_finite_non_zero_airflow_is_duty_whatever_its_classification(FlowClassification flowClassification, double designFlowRate_Lps)
        {
            Fixture fixture = Scaffolded();
            Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, fixture.Space_Corridor, flowClassification, designFlowRate_Lps);

            Assert.False(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling).IsScoped);
        }

        [Fact]
        public void A_duty_serving_no_room_refuses()
        {
            Fixture fixture = Scaffolded();
            Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, null, FlowClassification.Supply, 15.0);

            PartOSystemsScopeRefusal refusal = Assert.Single(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling).Refusals);

            Assert.Equal(PartOSystemsScopeRefusalReason.DutyServesNoSpace, refusal.Reason);
            Assert.Equal(Guid.Empty, refusal.Guid_Space);
        }

        /// <summary>A terminal serving two rooms refuses once per room, in the model's order, and the refusal names each.</summary>
        [Fact]
        public void A_duty_serving_two_rooms_refuses_once_per_room()
        {
            Fixture fixture = Scaffolded();
            VentilationTerminal ventilationTerminal = Terminal(fixture.AdjacencyCluster, fixture.VentilationSystem_MV, fixture.Space_Corridor, FlowClassification.Supply, 15.0);
            fixture.AdjacencyCluster.AddRelation(ventilationTerminal, fixture.Spaces_Dwelling[0]);

            List<PartOSystemsScopeRefusal> refusals = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling).Refusals;

            Assert.Equal(2, refusals.Count);
            Assert.Contains(refusals, x => x.Reason == PartOSystemsScopeRefusalReason.DutyOutsideDwellingScope && x.Guid_Space == fixture.Space_Corridor.Guid);
            Assert.Contains(refusals, x => x.Reason == PartOSystemsScopeRefusalReason.DutyInsideDwellingScope && x.Guid_Space == fixture.Spaces_Dwelling[0].Guid);
        }

        // =================================================================================================
        // Inputs
        // =================================================================================================

        [Fact]
        public void No_model_no_identities_and_an_unknown_identity_refuse()
        {
            Fixture fixture = Scaffolded();

            Assert.Equal(PartOSystemsScopeRefusalReason.NoModel, Assert.Single(Query.PartOSystemsMaterialisationScope(null, fixture.Guids_Built, fixture.Guids_Space_Dwelling).Refusals).Reason);
            Assert.Equal(PartOSystemsScopeRefusalReason.NoIdentities, Assert.Single(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, null, fixture.Guids_Space_Dwelling).Refusals).Reason);
            Assert.Equal(PartOSystemsScopeRefusalReason.NoIdentities, Assert.Single(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, [Guid.Empty], fixture.Guids_Space_Dwelling).Refusals).Reason);

            Guid guid = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
            PartOSystemsScopeRefusal refusal = Assert.Single(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, [.. fixture.Guids_Built, guid], fixture.Guids_Space_Dwelling).Refusals);
            Assert.Equal(PartOSystemsScopeRefusalReason.IdentityNotOnModel, refusal.Reason);
            Assert.Equal(guid, refusal.Guid_VentilationSystem);

            //A cooling system's identity is not a ventilation system's.
            Guid guid_Cooling = fixture.AdjacencyCluster.GetObjects<CoolingSystem>()[0].Guid;
            Assert.Equal(PartOSystemsScopeRefusalReason.IdentityNotOnModel, Assert.Single(Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, [.. fixture.Guids_Built, guid_Cooling], fixture.Guids_Space_Dwelling).Refusals).Reason);
        }

        /// <summary>
        /// Fail closed, structurally: a result that carries a refusal never hands out a working copy or any identity, even
        /// if a future caller builds it with one.
        /// </summary>
        [Fact]
        public void A_refused_result_never_carries_a_working_copy()
        {
            Fixture fixture = Scaffolded();

            PartOSystemsMaterialisationScope scope = new(
                fixture.AdjacencyCluster,
                fixture.Guids_Built,
                [fixture.VentilationSystem_NV.Guid],
                [new PartOSystemsScopeExclusion(fixture.VentilationSystem_NV, 0, "note")],
                "summary",
                [new PartOSystemsScopeRefusal(PartOSystemsScopeRefusalReason.NotRemovable, "refused")]);

            Assert.False(scope.IsScoped);
            Assert.Null(scope.AdjacencyCluster);
            Assert.Empty(scope.Guids_Retained);
            Assert.Empty(scope.Guids_Removed);
            Assert.Empty(scope.Exclusions);
            Assert.Empty(scope.Notes);
            Assert.Equal("refused", scope.Refusal);
        }

        /// <summary>The dwelling scope only words a refusal; it never changes what is kept or left out.</summary>
        [Fact]
        public void The_dwelling_scope_does_not_change_the_scope_taken()
        {
            Fixture fixture = Scaffolded();

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);
            PartOSystemsMaterialisationScope scope_None = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, null);

            Assert.Equal(scope.Guids_Retained, scope_None.Guids_Retained);
            Assert.Equal(scope.Guids_Removed, scope_None.Guids_Removed);
            Assert.Equal(scope.Notes, scope_None.Notes);
        }

        // =================================================================================================
        // Legacy and simple models
        // =================================================================================================

        /// <summary>The system-free / MVHR-only shape every earlier fixture had: nothing to leave out.</summary>
        [Fact]
        public void An_MVHR_only_model_retains_everything_and_leaves_nothing_out()
        {
            Fixture fixture = new();
            VentilationSystem ventilationSystem = VentilationSystem_(fixture.AdjacencyCluster, "MVHR", 1, "MVHR Flat 1");
            Space space = Space(fixture.AdjacencyCluster, "Bedroom");
            Terminal(fixture.AdjacencyCluster, ventilationSystem, space, FlowClassification.Supply, 13.0);

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, [ventilationSystem.Guid], [space.Guid]);

            Assert.True(scope.IsScoped);
            Assert.Equal([ventilationSystem.Guid], scope.Guids_Retained);
            Assert.Empty(scope.Guids_Removed);
            Assert.Empty(scope.Exclusions);
            Assert.Equal(Json(fixture.AdjacencyCluster), Json(scope.AdjacencyCluster));
        }

        [Fact]
        public void The_retained_and_removed_identities_are_in_guid_order()
        {
            Fixture fixture = Scaffolded();

            PartOSystemsMaterialisationScope scope = Query.PartOSystemsMaterialisationScope(fixture.AdjacencyCluster, fixture.Guids_Built, fixture.Guids_Space_Dwelling);

            Assert.Equal(Sorted(scope.Guids_Retained), scope.Guids_Retained);
            Assert.Equal(Sorted(scope.Guids_Removed), scope.Guids_Removed);
        }
    }
}
