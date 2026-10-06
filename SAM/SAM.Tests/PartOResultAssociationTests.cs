// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using SAM.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Iteration 0 step 8: design intent and TM59 results are tied together by identity, never by name.</b>
    /// <para>
    /// <b>The fixture is the shape that makes name matching wrong rather than merely weak.</b> Three flats, and
    /// the room in each is called <i>exactly</i> <c>"Bedroom 2"</c> - not "Flat 1 Bedroom 2", which is a name
    /// that happens to be unique. Plus a communal corridor. That is the real validation model's shape, and in it
    /// a name identifies nothing at all: three different rooms answer to it.
    /// </para>
    /// <para>
    /// <b>Why the internal condition is the important half.</b> Restoring the wrong flat's
    /// <c>InternalCondition</c> drives the calculation with the wrong occupancy profile and the wrong ventilation
    /// system, and then a correct result association reports that wrong number against the right room. It would
    /// pass every check downstream. So these tests prove the identity holds on the way IN, not only on the way
    /// out.
    /// </para>
    /// <para>
    /// Every flat's room is deliberately given <b>different design intent and different hourly data</b>, so a
    /// cross-wiring changes an observable answer rather than merely a reference.
    /// </para>
    /// </summary>
    public class PartOResultAssociationTests
    {
        //The TAS spelling, which is what the TSD conversion writes.
        private const string key_Tas_OccupantSensibleGain = "Occupant Sensible Gain";

        /// <summary>
        /// The parameter the fixture stamps its stable identity under.
        /// <para>
        /// <b>An arbitrary name, and that is the point.</b> <c>SAM.Analytical</c> defines no such parameter and
        /// must not: which value survives a simulation is engine-specific, so <c>SimulationSpaceMap</c> takes a
        /// <c>Func&lt;Space, string&gt;</c> and the caller says what it reads. For TAS the value is the zone guid,
        /// and <c>SpaceParameter.ZoneGuid</c> is a <c>SAM.Analytical.Tas</c> enum this assembly cannot even see.
        /// </para>
        /// </summary>
        private const string key_StableIdentity = "SomeEngineStableIdentity";

        // ---------------------------------------------------------------------------------------------
        // 1-3: each flat receives only its own design intent and its own result
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Proofs 1, 2 and 3 together.</b> Each flat's "Bedroom 2" is restored with <i>its own</i> internal
        /// condition, assessed against <i>its own</i> scenario's criterion, and its result is associated back to
        /// <i>its own</i> scenario - with all three rooms sharing one name throughout.
        /// </summary>
        [Fact]
        public void EachFlat_ReceivesOnlyItsOwnDesignIntentAndItsOwnResult()
        {
            Fixture fixture = new();

            //--- on the way in: the internal condition came from the right flat ---
            Assert.True(fixture.Calculator.RestoreDesignInternalConditions());
            Assert.Empty(fixture.Calculator.AssociationRefusals);

            foreach (string flat in Fixture.Flats)
            {
                Space space_Simulation = fixture.SimulationSpace(flat);

                //The internal condition is named after the FLAT, while the space is named "Bedroom 2" - so this
                //assertion can only pass if identity, not name, decided which one was copied.
                Assert.Equal("Bedroom 2", space_Simulation.Name);
                Assert.Equal(flat + " IC", space_Simulation.InternalCondition.Name);
            }

            //--- and on the way out: each result is associated back to its own scenario ---
            TM59AssessmentResult tM59AssessmentResult = fixture.Assess();

            Dictionary<OverheatingScenario, List<TMResult>> dictionary = fixture.ScenarioMap.Associate(tM59AssessmentResult, out List<TMResult> tMResults_Unassociated);

            Assert.Empty(tMResults_Unassociated);

            foreach (string flat in Fixture.Flats)
            {
                OverheatingScenario overheatingScenario = fixture.Scenario(flat);

                TMResult tMResult = Assert.Single(dictionary[overheatingScenario]);

                //The result carries the SIMULATED space's guid, and it is this flat's.
                Assert.Equal(fixture.SimulationSpace(flat).Guid.ToString(), tMResult.Reference);
            }

            //Three flats plus the corridor, each with exactly one result and no cross-attribution.
            Assert.Equal(4, dictionary.Count);
        }

        /// <summary>
        /// <b>Proof 4: the corridor stays outside the three dwelling associations.</b> It is assessed - TM59 has
        /// a corridor criterion and dropping it would lose an assessment the building needs - but it belongs to
        /// its own <c>CommonSpace</c> scenario and to no flat.
        /// </summary>
        [Fact]
        public void TheCorridor_IsAssessedAndBelongsToNoDwelling()
        {
            Fixture fixture = new();

            fixture.Calculator.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult = fixture.Assess();

            Dictionary<OverheatingScenario, List<TMResult>> dictionary = fixture.ScenarioMap.Associate(tM59AssessmentResult, out _);

            OverheatingScenario overheatingScenario_Corridor = fixture.Scenario(Fixture.Corridor);

            //Assessed, and under the common-space scope.
            Assert.Equal(PartOAssessmentScope.CommonSpace, overheatingScenario_Corridor.Scope);
            Assert.Single(dictionary[overheatingScenario_Corridor]);

            //It is a corridor RESULT, not a dwelling one.
            Assert.Single(tM59AssessmentResult.CorridorResults);
            Assert.Equal(fixture.SimulationSpace(Fixture.Corridor).Guid.ToString(), tM59AssessmentResult.CorridorResults[0].Reference);

            //And no dwelling scenario claims it.
            foreach (string flat in Fixture.Flats)
            {
                Assert.Equal(PartOAssessmentScope.Dwelling, fixture.Scenario(flat).Scope);
                Assert.DoesNotContain(fixture.SimulationSpace(Fixture.Corridor).Guid.ToString(), dictionary[fixture.Scenario(flat)].ConvertAll(x => x.Reference));
            }

            //Read the other way round: the corridor's space maps to the corridor scenario and nothing else.
            Assert.Equal(overheatingScenario_Corridor, fixture.ScenarioMap.Scenario(fixture.SimulationSpace(Fixture.Corridor)));
        }

        // ---------------------------------------------------------------------------------------------
        // 5-6: names are irrelevant, duplicates are not ambiguous
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Proof 5: changing names does not change association.</b> Every space in both models is renamed to
        /// something meaningless and mutually inconsistent - the design side and the simulated side no longer even
        /// agree - and every association is identical, because none of it was ever reading a name.
        /// </summary>
        [Fact]
        public void ChangingNames_DoesNotChangeAssociation()
        {
            Fixture fixture_Before = new();
            fixture_Before.Calculator.RestoreDesignInternalConditions();

            Dictionary<string, string> dictionary_Before = fixture_Before.AssociationByZoneGuid();

            Fixture fixture_After = new(rename: true);
            fixture_After.Calculator.RestoreDesignInternalConditions();

            Assert.Empty(fixture_After.Calculator.AssociationRefusals);

            //The two sides disagree about every name, and the association is unchanged.
            Assert.Equal(dictionary_Before, fixture_After.AssociationByZoneGuid());

            //Not vacuous: the names really did change, and really do differ between the two models.
            Space space_Design = fixture_After.DesignSpace("Flat 1");
            Space space_Simulation = fixture_After.SimulationSpace("Flat 1");

            Assert.NotEqual(space_Design.Name, space_Simulation.Name);
            Assert.DoesNotContain("Bedroom", space_Simulation.Name);

            //And the right internal condition still arrived.
            Assert.Equal("Flat 1 IC", space_Simulation.InternalCondition.Name);
        }

        /// <summary>
        /// <b>Proof 6: duplicate names create no ambiguity while the guid mapping is valid.</b> All three rooms
        /// share one name and nothing is refused, nothing is reported ambiguous, and the map is complete in both
        /// directions.
        /// </summary>
        [Fact]
        public void DuplicateNames_AreNotAmbiguousWhenTheMappingIsValid()
        {
            Fixture fixture = new();

            //One name, three rooms.
            Assert.Equal(3, fixture.AnalyticalModel_Simulation.GetSpaces().FindAll(x => x.Name == "Bedroom 2").Count);

            Assert.True(fixture.SimulationSpaceMap.IsComplete);
            Assert.Empty(fixture.SimulationSpaceMap.Ambiguous);
            Assert.Empty(fixture.SimulationSpaceMap.AmbiguousDesign);
            Assert.Empty(fixture.SimulationSpaceMap.Unresolved);

            Assert.True(fixture.ScenarioMap.IsComplete);
            Assert.Empty(fixture.ScenarioMap.Refusals);

            //Every space resolves both ways, and to a different partner each time.
            List<Guid> guids = [];

            foreach (string flat in Fixture.Flats)
            {
                Space space_Design = fixture.DesignSpace(flat);
                Space space_Simulation = fixture.SimulationSpace(flat);

                Assert.Equal(space_Simulation.Guid, fixture.SimulationSpaceMap.Simulation(space_Design).Guid);
                Assert.Equal(space_Design.Guid, fixture.SimulationSpaceMap.Design(space_Simulation).Guid);

                guids.Add(space_Simulation.Guid);
            }

            Assert.Equal(3, guids.Distinct().Count());
        }

        // ---------------------------------------------------------------------------------------------
        // 7-8: refusals
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Proof 7: a missing mapping refuses explicitly.</b> Flat 2's simulated room loses its stable
        /// identity, so with three rooms sharing one name it can no longer be resolved - and it is <b>not</b>
        /// paired with Flat 1's or Flat 3's design space. Its internal condition is left as the simulation
        /// produced it, its scenario is refused, and both are reported.
        /// </summary>
        [Fact]
        public void AMissingMapping_RefusesExplicitly()
        {
            Fixture fixture = new(drop_StableKey: "Flat 2");

            Assert.False(fixture.SimulationSpaceMap.IsComplete);

            //Refused on the way in, and the refusal names the space.
            fixture.Calculator.RestoreDesignInternalConditions();

            string refusal = Assert.Single(fixture.Calculator.AssociationRefusals);
            Assert.Contains("does not resolve to exactly one design space", refusal);

            //The critical part: it was NOT given another flat's internal condition.
            Assert.Null(fixture.SimulationSpace("Flat 2").InternalCondition);

            //The other two flats are untouched by their neighbour's problem.
            Assert.Equal("Flat 1 IC", fixture.SimulationSpace("Flat 1").InternalCondition.Name);
            Assert.Equal("Flat 3 IC", fixture.SimulationSpace("Flat 3").InternalCondition.Name);

            //And the scenario for Flat 2 is refused whole rather than tied to a partial dwelling.
            Assert.False(fixture.ScenarioMap.IsComplete);
            Assert.Contains(fixture.ScenarioMap.Refusals, x => x.Contains("does not resolve to exactly one simulated space"));

            Assert.Empty(fixture.ScenarioMap.Spaces(fixture.Scenario("Flat 2")));
            Assert.Single(fixture.ScenarioMap.Spaces(fixture.Scenario("Flat 1")));
        }

        /// <summary>
        /// <b>Proof 8: two candidate mappings for one design identity refuse explicitly.</b> Two simulated spaces
        /// carry Flat 1's stable identity. Neither direction remains usable: restoring design intent onto both
        /// candidates would be just as unsafe as choosing one of their results for Flat 1.
        /// </summary>
        [Fact]
        public void TwoCandidateMappingsForOneDesignIdentity_RefuseExplicitly()
        {
            Fixture fixture = new(duplicate_StableKey: "Flat 1");

            //Reported, and struck out of the reverse direction.
            Assert.Contains(fixture.DesignSpace("Flat 1").Guid, fixture.SimulationSpaceMap.AmbiguousDesign.ConvertAll(x => x.Guid));
            Assert.Null(fixture.SimulationSpaceMap.Simulation(fixture.DesignSpace("Flat 1")));
            Assert.False(fixture.SimulationSpaceMap.IsComplete);

            //The forward direction is struck out too, so neither candidate can receive Flat 1's design intent.
            Assert.Null(fixture.SimulationSpaceMap.Design(fixture.SimulationSpace("Flat 1")));

            //So Flat 1's scenario is refused, and the other flats are not.
            Assert.Contains(fixture.ScenarioMap.Refusals, x => x.Contains("does not resolve to exactly one simulated space"));
            Assert.Empty(fixture.ScenarioMap.Spaces(fixture.Scenario("Flat 1")));
            Assert.Single(fixture.ScenarioMap.Spaces(fixture.Scenario("Flat 3")));

            //A requested design space is refused for the same reason rather than resolved to either candidate.
            Assert.Empty(fixture.Calculator.Spaces([fixture.DesignSpace("Flat 1")], null));
            Assert.Single(fixture.Calculator.AssociationRefusals);

            //And the whole-model path cannot put the ambiguous candidate back into the assessment.
            Assert.DoesNotContain(fixture.Calculator.Spaces(null, null), x => x.Guid == fixture.SimulationSpace("Flat 1").Guid);
            Assert.Contains(fixture.Calculator.AssociationRefusals, x => x.Contains("does not resolve to exactly one design space"));
        }

        /// <summary>
        /// Two scenarios over one simulated space refuse too - the ambiguity on the scenario side rather than the
        /// mapping side.
        /// </summary>
        [Fact]
        public void TwoScenariosOverOneSpace_RefuseExplicitly()
        {
            Fixture fixture = new();

            //A second dwelling scenario over Flat 1's zone, stated separately.
            OverheatingScenario overheatingScenario_Duplicate = new(PartOAssessmentScope.Dwelling, fixture.DesignZone("Flat 1").Guid, PartOIteration.BasePassive, new SystemTemplate("NV", null, null, null, null, null));

            OverheatingScenarioMap overheatingScenarioMap = new([.. fixture.Scenarios, overheatingScenario_Duplicate], fixture.AnalyticalModel_Design, fixture.SimulationSpaceMap);

            Assert.False(overheatingScenarioMap.IsComplete);
            Assert.Contains(overheatingScenarioMap.Refusals, x => x.Contains("claimed by more than one scenario"));

            //Neither wins: the space maps to no scenario at all.
            Assert.Null(overheatingScenarioMap.Scenario(fixture.SimulationSpace("Flat 1")));

            //The scenario accepted first is rolled back whole, including its strategy claim.
            Assert.Empty(overheatingScenarioMap.Spaces(fixture.Scenario("Flat 1")));
            Assert.DoesNotContain(overheatingScenarioMap.OverheatingScenarios, x => x.Key == fixture.Scenario("Flat 1").Key);
            Assert.False(overheatingScenarioMap.VentilationStrategyMap.Selection(fixture.SimulationSpace("Flat 1")).IsSelected);

            //And the untouched flats still resolve.
            Assert.NotNull(overheatingScenarioMap.Scenario(fixture.SimulationSpace("Flat 3")));
            Assert.True(overheatingScenarioMap.VentilationStrategyMap.Selection(fixture.SimulationSpace("Flat 3")).IsSelected);
        }

        /// <summary>
        /// <b>The same assessment stated twice is one answer, not a collision</b> - the rule
        /// <c>VentilationStrategyMap</c> and <c>Query.SelectPreferredCapableSystem</c> already follow. Two
        /// scenarios are the same assessment exactly when they derive the same <c>Key</c>, so a caller assembling
        /// its scenario list from two overlapping sources is not punished for it.
        /// </summary>
        [Fact]
        public void TheSameScenarioTwice_IsNotACollision()
        {
            Fixture fixture = new();

            List<OverheatingScenario> overheatingScenarios = [.. fixture.Scenarios, new OverheatingScenario(fixture.Scenario("Flat 1"))];

            //The duplicate really is the same assessment, and really is a different object.
            Assert.Equal(fixture.Scenario("Flat 1").Key, overheatingScenarios[^1].Key);
            Assert.NotSame(fixture.Scenario("Flat 1"), overheatingScenarios[^1]);

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, fixture.AnalyticalModel_Design, fixture.SimulationSpaceMap);

            Assert.True(overheatingScenarioMap.IsComplete);
            Assert.Empty(overheatingScenarioMap.Refusals);

            //Recorded once, with its spaces intact - the second walk must not overwrite the first's record.
            Assert.Single(overheatingScenarioMap.Spaces(fixture.Scenario("Flat 1")));
            Assert.Equal(4, overheatingScenarioMap.OverheatingScenarios.Count);

            Assert.NotNull(overheatingScenarioMap.Scenario(fixture.SimulationSpace("Flat 1")));
            Assert.True(overheatingScenarioMap.VentilationStrategyMap.Selection(fixture.SimulationSpace("Flat 1")).IsSelected);
        }

        /// <summary>
        /// Three <i>different</i> scenarios over one space all refuse, and the third does not resurrect the first
        /// by finding its claim already struck out.
        /// </summary>
        [Fact]
        public void ThreeScenariosOverOneSpace_AllRefuse()
        {
            Fixture fixture = new();

            Guid guid_Zone = fixture.DesignZone("Flat 1").Guid;

            List<OverheatingScenario> overheatingScenarios =
            [
                fixture.Scenario("Flat 1"),
                new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Zone, PartOIteration.BasePassive, new SystemTemplate("NV", null, null, null, null, null)),
                new OverheatingScenario(PartOAssessmentScope.Dwelling, guid_Zone, PartOIteration.AcousticRestricted, new SystemTemplate("MV", null, null, null, null, null))
            ];

            OverheatingScenarioMap overheatingScenarioMap = new(overheatingScenarios, fixture.AnalyticalModel_Design, fixture.SimulationSpaceMap);

            Assert.False(overheatingScenarioMap.IsComplete);

            //None of the three owns it, and none of the three has a strategy for it.
            Assert.Null(overheatingScenarioMap.Scenario(fixture.SimulationSpace("Flat 1")));
            Assert.False(overheatingScenarioMap.VentilationStrategyMap.Selection(fixture.SimulationSpace("Flat 1")).IsSelected);

            foreach (OverheatingScenario overheatingScenario in overheatingScenarios)
            {
                Assert.Empty(overheatingScenarioMap.Spaces(overheatingScenario));
            }
        }

        // ---------------------------------------------------------------------------------------------
        // 9-10: the step 7 guarantee survives, and identity never reads provenance
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>Proof 9: the scenario's ventilation strategy remains authoritative after mapping.</b> The
        /// <c>VentilationStrategyMap</c> the association builds is keyed on the resolved <i>simulated</i>
        /// identities, so each flat is assessed against the criterion its own scenario states - even though every
        /// flat's design data says something different and all three rooms share a name.
        /// </summary>
        [Fact]
        public void TheScenarioStrategy_RemainsAuthoritativeAfterMapping()
        {
            Fixture fixture = new();

            fixture.Calculator.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult = fixture.Assess();

            Dictionary<OverheatingScenario, List<TMResult>> dictionary = fixture.ScenarioMap.Associate(tM59AssessmentResult, out _);

            //Flat 1 states MVRE, Flat 2 states NV, Flat 3 states MVRE - see the fixture. Their design internal
            //conditions say the OPPOSITE in each case, so the criterion can only track the scenario.
            Assert.Equal("MVRE", fixture.Scenario("Flat 1").VentilationStrategy);
            Assert.Equal("NV", fixture.Scenario("Flat 2").VentilationStrategy);

            Assert.Contains(dictionary[fixture.Scenario("Flat 1")][0], tM59AssessmentResult.MechanicalVentilationResults);
            Assert.Contains(dictionary[fixture.Scenario("Flat 2")][0], tM59AssessmentResult.NaturalVentilationResults);
            Assert.Contains(dictionary[fixture.Scenario("Flat 3")][0], tM59AssessmentResult.MechanicalVentilationResults);

            //Nothing was refused for want of a strategy - the map really was built from resolved identities.
            Assert.Empty(tM59AssessmentResult.VentilationStrategyRefusals);

            //And the design data does contradict the scenarios, so this is not passing by agreement.
            Assert.Equal("NV", fixture.DesignSpace("Flat 1").InternalCondition.GetSystemTypeName<VentilationSystemType>());
            Assert.Equal("MVRE", fixture.DesignSpace("Flat 2").InternalCondition.GetSystemTypeName<VentilationSystemType>());
        }

        /// <summary>
        /// <b>Proof 10: no TSD, TPD, file or provenance value participates in identity matching.</b>
        /// <para>
        /// <b>Asserted behaviourally first</b>, because the structural scan below is the weaker half: it only
        /// proves no public member is <i>named</i> after provenance, not that the implementation ignores it. So
        /// the model name - which is what a result reports as its <c>Source</c> - and the stamped provenance
        /// fallback are both changed, and every association comes out identical.
        /// </para>
        /// <para>
        /// The scan is kept as a guard against a future member reaching for a file name when an identity is
        /// missing, which is exactly how this class of defect returns.
        /// </para>
        /// </summary>
        [Fact]
        public void NoProvenanceOrEngineValue_ParticipatesInIdentityMatching()
        {
            //--- behavioural: change the provenance, keep the association ---
            Fixture fixture_Default = new();
            fixture_Default.Calculator.RestoreDesignInternalConditions();

            Fixture fixture_Provenance = new(source: "From a completely different file.tsd");
            fixture_Provenance.Calculator.SourceFallback = "SomeOtherAssembly";
            fixture_Provenance.Calculator.RestoreDesignInternalConditions();

            //Same association, expressed without a single name or reference to either model.
            Assert.Equal(fixture_Default.AssociationByZoneGuid(), fixture_Provenance.AssociationByZoneGuid());

            //Every flat still got its own design intent, under the other provenance.
            foreach (string flat in Fixture.Flats)
            {
                Assert.Equal(flat + " IC", fixture_Provenance.SimulationSpace(flat).InternalCondition.Name);
            }

            //And the results really do carry the different provenance, so this is not passing by the two being
            //identical models.
            TM59AssessmentResult tM59AssessmentResult_Provenance = fixture_Provenance.Assess();

            Assert.Equal("From a completely different file.tsd", tM59AssessmentResult_Provenance.NaturalVentilationResults[0].Source);
            Assert.NotEqual(fixture_Default.Assess().NaturalVentilationResults[0].Source, tM59AssessmentResult_Provenance.NaturalVentilationResults[0].Source);

            //Association is unaffected by it.
            Assert.Equal(4, fixture_Provenance.ScenarioMap.Associate(tM59AssessmentResult_Provenance, out List<TMResult> tMResults_Unassociated).Count);
            Assert.Empty(tMResults_Unassociated);

            //--- structural: the weaker guard ---
            string[] forbidden = ["tsd", "tpd", "tbd", "file", "path", "directory", "source", "provenance", "engine"];

            foreach (Type type in new[] { typeof(SimulationSpaceMap), typeof(OverheatingScenarioMap), typeof(VentilationStrategyMap) })
            {
                foreach (MemberInfo memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    foreach (string text in forbidden)
                    {
                        Assert.False(memberInfo.Name.ToLower().Contains(text), string.Format("{0}.{1} names '{2}'. Identity matching must not read provenance or an engine artefact.", type.Name, memberInfo.Name, text));
                    }
                }
            }

            //And the association really is driven by the guid: a result whose Reference is not one refuses.
            Fixture fixture = new();
            fixture.Calculator.RestoreDesignInternalConditions();

            TM59AssessmentResult tM59AssessmentResult = fixture.Assess();

            TMResult tMResult = tM59AssessmentResult.NaturalVentilationResults[0];

            //Same name, same Source, different Reference - and it no longer associates.
            Assert.NotNull(fixture.ScenarioMap.Scenario(tMResult));
            Assert.Null(fixture.ScenarioMap.Scenario(new TM59CorridorExtendedResult(tMResult.Name, tMResult.Source, Guid.NewGuid().ToString(), TM52BuildingCategory.CategoryII, [], new IndexedDoubles(), new IndexedDoubles(), new IndexedDoubles())));
        }

        /// <summary>
        /// The scenario map refuses rather than throwing when it is given nothing to work with, and says which
        /// half was missing.
        /// </summary>
        [Fact]
        public void AScenarioMapWithNothingToWorkWith_RefusesWithAReason()
        {
            Fixture fixture = new();

            Assert.Contains("no design model", new OverheatingScenarioMap(fixture.Scenarios, null, fixture.SimulationSpaceMap).Refusals[0]);
            Assert.Contains("no simulation space map", new OverheatingScenarioMap(fixture.Scenarios, fixture.AnalyticalModel_Design, null).Refusals[0]);
            Assert.Contains("names nothing assessable", new OverheatingScenarioMap([new OverheatingScenario()], fixture.AnalyticalModel_Design, fixture.SimulationSpaceMap).Refusals[0]);

            //A scenario over a zone the design model does not hold.
            OverheatingScenario overheatingScenario = new(PartOAssessmentScope.Dwelling, Guid.NewGuid(), PartOIteration.Undefined, new SystemTemplate("NV", null, null, null, null, null));

            Assert.Contains("the design model does not hold", new OverheatingScenarioMap([overheatingScenario], fixture.AnalyticalModel_Design, fixture.SimulationSpaceMap).Refusals[0]);
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture - three flats, one room name, one corridor
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// The three-flat validation shape, built twice: a design model and the simulated rebuild it produces,
        /// linked only by the stable identity a TAS round trip preserves.
        /// </summary>
        private class Fixture
        {
            internal static readonly string[] Flats = ["Flat 1", "Flat 2", "Flat 3"];

            internal const string Corridor = "Corridor";

            /// <summary>
            /// What each flat's SCENARIO states, and in every case the opposite of what its design data says -
            /// so a criterion that tracked the design instead of the scenario would be visible.
            /// </summary>
            private static readonly Dictionary<string, string> dictionary_Strategy_Scenario = new()
            {
                { "Flat 1", "MVRE" }, { "Flat 2", "NV" }, { "Flat 3", "MVRE" }, { Corridor, "UV" }
            };

            private static readonly Dictionary<string, string> dictionary_Strategy_Design = new()
            {
                { "Flat 1", "NV" }, { "Flat 2", "MVRE" }, { "Flat 3", "NV" }, { Corridor, "MVRE" }
            };

            private readonly Dictionary<string, Space> dictionary_Space_Design = [];
            private readonly Dictionary<string, Space> dictionary_Space_Simulation = [];
            private readonly Dictionary<string, Zone> dictionary_Zone_Design = [];
            private readonly Dictionary<string, OverheatingScenario> dictionary_Scenario = [];

            internal AnalyticalModel AnalyticalModel_Design { get; }

            internal AnalyticalModel AnalyticalModel_Simulation { get; }

            internal SimulationSpaceMap SimulationSpaceMap { get; }

            internal OverheatingScenarioMap ScenarioMap { get; }

            internal TM59AssessmentCalculator Calculator { get; }

            internal List<OverheatingScenario> Scenarios => [.. dictionary_Scenario.Values];

            /// <param name="rename">
            /// Rename every space on both sides, inconsistently, so the two models agree about no name at all.
            /// </param>
            /// <param name="drop_StableKey">A flat whose simulated space loses its stable identity.</param>
            /// <param name="duplicate_StableKey">
            /// A flat whose stable identity is stamped onto a second simulated space as well.
            /// </param>
            /// <param name="source">
            /// The simulated model's NAME, which is what a result reports as its <c>Source</c>. Varying it must
            /// change nothing about association.
            /// </param>
            internal Fixture(bool rename = false, string drop_StableKey = null, string duplicate_StableKey = null, string source = "Three Flats")
            {
                AdjacencyCluster adjacencyCluster_Design = new();
                AdjacencyCluster adjacencyCluster_Simulation = new();

                List<string> names = [.. Flats, Corridor];

                for (int i = 0; i < names.Count; i++)
                {
                    string name = names[i];

                    //THE POINT OF THE FIXTURE: one room name for all three flats.
                    string name_Space = name == Corridor ? "Corridor" : "Bedroom 2";
                    string key_Stable = "tas-zone-" + i;

                    //--- design side ---
                    Space space_Design = new(rename ? "design-" + i : name_Space)
                    {
                        InternalCondition = InternalCondition(name)
                    };

                    space_Design.SetValue(key_StableIdentity, key_Stable);

                    Zone zone_Design = new(name);

                    adjacencyCluster_Design.AddObject(space_Design);
                    adjacencyCluster_Design.AddObject(zone_Design);
                    adjacencyCluster_Design.AddRelation(zone_Design, space_Design);

                    dictionary_Space_Design[name] = space_Design;
                    dictionary_Zone_Design[name] = zone_Design;

                    //--- simulation side: a REBUILD, so a fresh guid and no internal condition ---
                    Space space_Simulation = Space(rename ? "sim-" + i : name_Space, name);

                    if (name != drop_StableKey)
                    {
                        space_Simulation.SetValue(key_StableIdentity, key_Stable);
                    }

                    adjacencyCluster_Simulation.AddObject(space_Simulation);

                    dictionary_Space_Simulation[name] = space_Simulation;

                    //--- the scenario, stated against the DESIGN zone ---
                    dictionary_Scenario[name] = new OverheatingScenario(
                        name == Corridor ? PartOAssessmentScope.CommonSpace : PartOAssessmentScope.Dwelling,
                        zone_Design.Guid,
                        PartOIteration.Undefined,
                        new SystemTemplate(dictionary_Strategy_Scenario[name], null, null, null, null, null));
                }

                if (duplicate_StableKey != null)
                {
                    //A second simulated space carrying an existing stable identity: two candidates for one
                    //design space.
                    Space space_Simulation_Duplicate = Space("Bedroom 2", duplicate_StableKey);

                    space_Simulation_Duplicate.SetValue(key_StableIdentity, StableKey(dictionary_Space_Design[duplicate_StableKey]));

                    adjacencyCluster_Simulation.AddObject(space_Simulation_Duplicate);
                }

                AnalyticalModel_Design = new AnalyticalModel("Three Flats", null, null, null, adjacencyCluster_Design);

                AnalyticalModel_Simulation = new AnalyticalModel(source, null, null, null, adjacencyCluster_Simulation);
                AnalyticalModel_Simulation.SetValue(AnalyticalModelParameter.WeatherData, new WeatherData("Test", "Test", 51.5, -0.1, 0, WeatherYear()));

                //The stable key is supplied by the caller, exactly as SAM_Tas supplies the TAS zone guid -
                //SAM.Analytical never names it.
                SimulationSpaceMap = new SimulationSpaceMap(AnalyticalModel_Design.GetSpaces(), AnalyticalModel_Simulation.GetSpaces(), StableKey);

                ScenarioMap = new OverheatingScenarioMap(Scenarios, AnalyticalModel_Design, SimulationSpaceMap);

                Calculator = new TM59AssessmentCalculator(AnalyticalModel_Simulation, AnalyticalModel_Design, SimulationSpaceMap)
                {
                    OccupancySensibleGainSeriesKey = key_Tas_OccupantSensibleGain,
                    VentilationStrategyMap = ScenarioMap.VentilationStrategyMap
                };
            }

            internal Space DesignSpace(string name) => dictionary_Space_Design[name];

            internal Zone DesignZone(string name) => dictionary_Zone_Design[name];

            internal OverheatingScenario Scenario(string name) => dictionary_Scenario[name];

            /// <summary>
            /// The simulated space, read back off the calculator's model so that a restore is visible - the
            /// calculator rebuilds its model, so the fixture's own instance would be a stale copy.
            /// </summary>
            internal Space SimulationSpace(string name)
            {
                Guid guid = dictionary_Space_Simulation[name].Guid;

                return Calculator.AnalyticalModel.GetSpaces().Find(x => x.Guid == guid);
            }

            internal TM59AssessmentResult Assess()
            {
                return Calculator.Calculate(Calculator.Spaces(null, null));
            }

            /// <summary>
            /// The whole association expressed without a single name: each design space's stable key mapped to
            /// its simulated space's stable key. Comparable across a rename.
            /// </summary>
            internal Dictionary<string, string> AssociationByZoneGuid()
            {
                Dictionary<string, string> result = [];

                foreach (Space space_Design in AnalyticalModel_Design.GetSpaces())
                {
                    Space space_Simulation = SimulationSpaceMap.Simulation(space_Design);

                    result[StableKey(space_Design)] = space_Simulation == null ? null : StableKey(space_Simulation);
                }

                return result;
            }

            private static string StableKey(Space space)
            {
                return space != null && space.TryGetValue(key_StableIdentity, out string result) ? result : null;
            }

            /// <summary>
            /// An internal condition named after the FLAT, not the room - so a cross-wiring is visible - and
            /// stating the opposite ventilation system from the flat's scenario.
            /// </summary>
            private static InternalCondition InternalCondition(string name)
            {
                InternalCondition result = new(name + " IC");

                result.SetValue(InternalConditionParameter.VentilationSystemTypeName, dictionary_Strategy_Design[name]);

                return result;
            }

            /// <summary>
            /// A simulated space carrying hourly series stored the way the TSD converter stores them, with a
            /// distinct temperature run per flat so a cross-wiring changes a number.
            /// </summary>
            private static Space Space(string name_Space, string name_Flat)
            {
                Space result = new(name_Space);

                double offset = System.Array.IndexOf(Flats, name_Flat) + 1;

                ParameterSet parameterSet = new("SAM.Analytical.Tas.dll");

                parameterSet.Add(Core.Query.Name(SpaceSimulationResultParameter.ResultantTemperature), Values([21.0 + offset, 24.5 + offset, 27.5 + offset, 29.0 + offset]));
                parameterSet.Add(key_Tas_OccupantSensibleGain, Values([0, 80.0, 80.0, 0]));

                result.Add(parameterSet);

                return result;
            }

            private static WeatherYear WeatherYear()
            {
                WeatherYear result = new(2018);

                for (int day = 0; day < 365; day++)
                {
                    for (int hour = 0; hour < 24; hour++)
                    {
                        result.Add(day, hour, new Dictionary<string, double> { { WeatherDataType.DryBulbTemperature.ToString(), 20.0 } });
                    }
                }

                return result;
            }

            private static JsonArray Values(IEnumerable<double> values)
            {
                JsonArray result = [];

                foreach (double value in values)
                {
                    result.Add(value);
                }

                return result;
            }
        }
    }
}
