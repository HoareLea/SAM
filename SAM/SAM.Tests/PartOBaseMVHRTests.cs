// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Approved Document O Iteration 1a - Base MVHR: the chain from a regulatory requirement to air the
    /// simulation actually moves, and the three separations that chain must not collapse.</b>
    /// <para>
    /// <c>PartOIteration.BasePassive</c> has always asserted <c>Mechanical Ventilation At Design Rate =
    /// True</c>, and that claim is inside the permanent <c>OverheatingScenario.Key</c> - but until this
    /// iteration nothing made a simulation obey it. The Approved Document F rate was written onto each
    /// space's internal condition and stopped there: the TAS export writes a <c>ticV</c> rate only where a
    /// SAM Ventilation profile is assigned, which the preparation does not do and must not start doing, so
    /// the rate reached the exported file as zone metadata and no air moved. Worse, clearing the
    /// per-person basis to stop the Part F rate being double counted drove the model's authored outside-air
    /// rate to zero, so a Base MVHR dwelling simulated with LESS ventilation than the same dwelling
    /// assessed naturally ventilated.
    /// </para>
    /// <para>
    /// The chain that closes it is: realize the requirement as design terminals, connect them to a generic
    /// system and unit, derive the duty and check it against the requirement, then realize directional air
    /// movements the existing inter-zone air movement export already carries into TAS. These tests pin every
    /// joint of it, and pin the three things it must never do - rewrite the requirement, impose one terminal
    /// per space, or make airflow data by itself switch ventilation on.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with the other readers of the default Part F rule set, so the two never run at
    /// the same time: the rule set is reached through the process-wide <c>ActiveSetting.Setting</c> and its
    /// stored <c>PartFData</c> is shared by reference between every <c>PartFCalculator</c> built from it.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartOBaseMVHRTests
    {
        private const string name_LivingRoom = "Living Room";

        private const string name_Bedroom = "Bedroom 1";

        private const string name_Kitchen = "Kitchen";

        private const string name_Bathroom = "Bathroom";

        private const string name_Zone = "Flat 1";

        // =================================================================================================
        // A. The route and the iteration
        // =================================================================================================

        /// <summary>
        /// The whole chain, on the stated MVHR route at the stage defined over it: terminals, a system, a
        /// unit, a duty, and air movements. Every later test reads one joint of this.
        /// </summary>
        [Fact]
        public void MVHRAndBasePassive_PreparesTheWholeBaseMVHRDesign()
        {
            PartOIterationPreparation preparation = Prepared();

            Assert.Null(preparation.Refusal);
            Assert.True(preparation.Successful);

            Assert.Equal(PartOVentilationMode.MVHR, preparation.VentilationMode);
            Assert.Equal(PartOPartFAirflowApplication.Apply, preparation.AirflowApplication);

            Assert.NotEmpty(preparation.VentilationTerminals);
            Assert.NotNull(preparation.VentilationSystem);
            Assert.NotNull(preparation.AirHandlingUnit);

            Assert.True(preparation.DesignSupplyDuty_Lps > 0);
            Assert.True(preparation.DesignExtractDuty_Lps > 0);

            Assert.NotEmpty(AirMovements(preparation.AnalyticalModel));
        }

        /// <summary>
        /// The natural ventilation route at the MVHR stage. Refused before anything is applied - the pairing
        /// would mint a permanent identity asserting design-rate mechanical ventilation for a dwelling that
        /// has none.
        /// </summary>
        [Fact]
        public void NaturalVentilationAndBasePassive_Refuses()
        {
            PartOIterationPreparation preparation = Prepare(Model(), PartOIteration.BasePassive, "NV");

            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Empty(preparation.VentilationTerminals);
            Assert.Null(preparation.VentilationSystem);
        }

        /// <summary>The same mismatch stated the other way round, and refused for the same reason.</summary>
        [Fact]
        public void MVHRAndBaseNaturalVentilation_Refuses()
        {
            PartOIterationPreparation preparation = Prepare(Model(), PartOIteration.BaseNaturalVentilation, "MVHR");

            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Empty(preparation.VentilationTerminals);
            Assert.Null(preparation.VentilationSystem);
        }

        /// <summary>
        /// The mirror of the Iteration 1b rule, pointing the other way: a model whose internal conditions
        /// still say <c>NV</c> is prepared as MVHR when the assessment states MVHR, <b>and the stale value
        /// is not rewritten</b>.
        /// <para>
        /// Both halves matter. Reading the metadata would put the route back into data that may predate the
        /// assessment; writing it would put the decision back into the metadata it was taken out of, and
        /// would make the model on disk a lie about the building.
        /// </para>
        /// </summary>
        [Fact]
        public void AStaleNVOnTheModel_DoesNotOverrideAnExplicitMVHRRoute_AndIsNotRewritten()
        {
            PartOIterationPreparation preparation = Prepare(Model("NV"), PartOIteration.BasePassive, "MVHR");

            Assert.Null(preparation.Refusal);
            Assert.Equal(PartOVentilationMode.MVHR, preparation.VentilationMode);
            Assert.NotEmpty(preparation.VentilationTerminals);

            foreach (Space space in preparation.AnalyticalModel.GetSpaces())
            {
                Assert.True(space.InternalCondition.TryGetValue(InternalConditionParameter.VentilationSystemTypeName, out string ventilationSystemTypeName));
                Assert.Equal("NV", ventilationSystemTypeName);
            }
        }

        /// <summary>
        /// <b>The word the Approved Document itself uses reaches the assessment.</b>
        /// <para>
        /// <c>Query.PartOVentilationMode</c> takes <c>MVHR</c> and <c>MVRE</c> as two spellings of the one
        /// route, so an assessment could state <c>MVHR</c>, prepare successfully, simulate for an hour and
        /// then produce no assessment at all - every space refused by <c>VentilationStrategyMap</c>, whose
        /// recognised vocabulary did not contain the word its own upstream had already accepted.
        /// </para>
        /// <para>
        /// The scenario keeps saying <c>MVHR</c> rather than being rewritten to <c>MVRE</c> on the way in:
        /// substituting one stated identity for another is the class of quiet change this whole map exists
        /// to stop.
        /// </para>
        /// </summary>
        [Fact]
        public void TheWordMVHR_IsSelectedByTheAssessmentWithNoRefusal()
        {
            PartOIterationPreparation preparation = Prepared("MVHR");

            OverheatingScenario overheatingScenario = Assert.Single(preparation.OverheatingScenarios);

            List<Space> spaces = preparation.AnalyticalModel.GetSpaces();

            VentilationStrategyMap ventilationStrategyMap = new VentilationStrategyMap();

            Assert.True(ventilationStrategyMap.Add(overheatingScenario, spaces));

            foreach (Space space in spaces)
            {
                VentilationStrategySelection ventilationStrategySelection = ventilationStrategyMap.Selection(space);

                Assert.True(ventilationStrategySelection.IsSelected, ventilationStrategySelection.Reason);
                Assert.Equal("MVHR", ventilationStrategySelection.VentilationStrategy);
            }

            //Nothing was refused: the per-space selections above are each IsSelected, which is the same
            //fact stated per space rather than in aggregate.
        }

        /// <summary><c>MVRE</c> keeps working, unchanged, so the fix is additive rather than a swap.</summary>
        [Fact]
        public void TheWordMVRE_StillSelects()
        {
            PartOIterationPreparation preparation = Prepared();

            OverheatingScenario overheatingScenario = Assert.Single(preparation.OverheatingScenarios);

            VentilationStrategyMap ventilationStrategyMap = new VentilationStrategyMap();

            ventilationStrategyMap.Add(overheatingScenario, preparation.AnalyticalModel.GetSpaces());

            Assert.Equal("MVRE", ventilationStrategyMap.Selection(Space(preparation.AnalyticalModel, name_Bedroom)).VentilationStrategy);
        }

        // =================================================================================================
        // B. The requirement is read, never written
        // =================================================================================================

        /// <summary>
        /// The Approved Document F requirement is <b>byte-identical</b> after the design realization.
        /// Compared as serialised JSON, so a changed rate, a changed compliance status and a changed
        /// terminal count all fail this.
        /// </summary>
        [Fact]
        public void Realization_LeavesEveryPartFRequirementUnchanged()
        {
            AnalyticalModel analyticalModel = Model();

            Dictionary<string, string> before = PartFJson(analyticalModel);

            PartOIterationPreparation preparation = Prepare(analyticalModel, PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation.Refusal);

            Dictionary<string, string> after = PartFJson(preparation.AnalyticalModel);

            Assert.Equal(before.Count, after.Count);

            foreach (KeyValuePair<string, string> keyValuePair in before)
            {
                Assert.Equal(keyValuePair.Value, after[keyValuePair.Key]);
            }
        }

        /// <summary>
        /// A design duty is the designer's to change; the regulatory rate is not. Editing one leaves the
        /// other exactly where it was - which is the whole reason these are two objects.
        /// </summary>
        [Fact]
        public void EditingADesignTerminalDuty_LeavesTheRequirementAlone()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = Space(preparation.AnalyticalModel, name_Bedroom);

            VentilationTerminal ventilationTerminal = Assert.Single(Analytical.Query.VentilationTerminals(adjacencyCluster, space));

            double requirement_Lps = Requirement(space, PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps.Value;

            Assert.Equal(requirement_Lps, ventilationTerminal.DesignFlowRate_Lps.Value, 6);

            ventilationTerminal.DesignFlowRate_Lps = requirement_Lps + 5;
            adjacencyCluster.AddObject(ventilationTerminal);

            Assert.Equal(requirement_Lps, Requirement(Space(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), name_Bedroom), PartFTerminalRole.Supply).ContinuousDesignFlowRate_Lps.Value, 6);
        }

        /// <summary>
        /// Commissioning evidence is recorded from site and never written over by a design step. Pinned
        /// here because the realization reads the same objects that carry it.
        /// </summary>
        [Fact]
        public void MeasuredCommissioningRates_SurviveRealization()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            partFSpaceData.Terminals.Find(x => x.TerminalRole == PartFTerminalRole.Supply).MeasuredContinuousFlowRate_Lps = 17.25;

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);
            adjacencyCluster.AddObject(space);

            PartOIterationPreparation preparation = Prepare(new AnalyticalModel(analyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation.Refusal);

            Assert.Equal(17.25, Requirement(Space(preparation.AnalyticalModel, name_Bedroom), PartFTerminalRole.Supply).MeasuredContinuousFlowRate_Lps.Value, 6);
        }

        // =================================================================================================
        // C. Cardinality - 0..N, never one
        // =================================================================================================

        /// <summary>One space, two supply terminals. The space's duty is their sum, not either of them.</summary>
        [Fact]
        public void ASpace_CanHoldManySupplyTerminals()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = Subdivided(preparation, name_Bedroom, FlowClassification.Supply, 3);

            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster, Space(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), name_Bedroom));

            Assert.Equal(3, Analytical.Query.VentilationTerminals(ventilationTerminals, FlowClassification.Supply).Count);
        }

        /// <summary>And the same for extract, which is the direction a wet room has.</summary>
        [Fact]
        public void ASpace_CanHoldManyExtractTerminals()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = Subdivided(preparation, name_Bathroom, FlowClassification.Extract, 2);

            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster, Space(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), name_Bathroom));

            Assert.Equal(2, Analytical.Query.VentilationTerminals(ventilationTerminals, FlowClassification.Extract).Count);
        }

        /// <summary>
        /// Both directions in one room. Nothing in the model forbids it - and Approved Document F needs it,
        /// because a studio or an open plan living kitchen takes a supply terminal under paragraph 1.67 and
        /// a kitchen extract terminal under paragraph 1.17a at the same time.
        /// </summary>
        [Fact]
        public void ASpace_CanHoldSupplyAndExtractTerminalsAtOnce()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            VentilationTerminal ventilationTerminal = new VentilationTerminal("Bedroom 1 - added extract terminal", FlowClassification.Extract, 4);

            adjacencyCluster.AddObject(ventilationTerminal);
            adjacencyCluster.AddRelation(ventilationTerminal, space);

            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(adjacencyCluster, space);

            Assert.Single(Analytical.Query.VentilationTerminals(ventilationTerminals, FlowClassification.Supply));
            Assert.Single(Analytical.Query.VentilationTerminals(ventilationTerminals, FlowClassification.Extract));
        }

        /// <summary>
        /// <b>The subdivision contract.</b> One 20 l/s terminal becomes two of 10, and nothing else in the
        /// model moves: not the Approved Document F requirement, not the space, not the system, not the
        /// system's duty, and not the permanent scenario key. That is what "the design duty is the sum of
        /// the terminals, never the count of them" has to mean in practice.
        /// </summary>
        [Fact]
        public void SubdividingATerminal_ChangesNoRequirementNoDutyAndNoScenarioKey()
        {
            PartOIterationPreparation preparation = Prepared();

            Guid key_Before = Assert.Single(preparation.OverheatingScenarios).Key;

            Dictionary<string, string> partF_Before = PartFJson(preparation.AnalyticalModel);

            Analytical.Query.VentilationSystemDesignDuty(preparation.AnalyticalModel.AdjacencyCluster, preparation.VentilationSystem, out double supply_Before, out double extract_Before);

            AdjacencyCluster adjacencyCluster = Subdivided(preparation, name_Bedroom, FlowClassification.Supply, 2);

            AnalyticalModel analyticalModel = new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster);

            //The requirement, untouched.
            Dictionary<string, string> partF_After = PartFJson(analyticalModel);
            foreach (KeyValuePair<string, string> keyValuePair in partF_Before)
            {
                Assert.Equal(keyValuePair.Value, partF_After[keyValuePair.Key]);
            }

            //The duty, unchanged, because it is a sum.
            Analytical.Query.VentilationSystemDesignDuty(adjacencyCluster, preparation.VentilationSystem, out double supply_After, out double extract_After);

            Assert.Equal(supply_Before, supply_After, 6);
            Assert.Equal(extract_Before, extract_After, 6);

            //The system, the same object.
            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetMechanicalSystems<VentilationSystem>());
            Assert.Equal(preparation.VentilationSystem.Guid, ventilationSystem.Guid);

            //And the permanent identity of the assessment, unchanged - re-derived from a scenario stated
            //over the subdivided model rather than read off the old object.
            PartOIterationPreparation preparation_After = Prepare(analyticalModel, PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation_After.Refusal);
            Assert.Equal(key_Before, Assert.Single(preparation_After.OverheatingScenarios).Key);
        }

        /// <summary>
        /// Subdivision is a design act and leaves no trace in the regulatory statement: the space still
        /// carries exactly the terminal requirements Approved Document F gave it.
        /// </summary>
        [Fact]
        public void Subdivision_CreatesNoAdditionalPartFRequirement()
        {
            PartOIterationPreparation preparation = Prepared();

            int count_Before = Requirements(Space(preparation.AnalyticalModel, name_Bedroom)).Count;

            AdjacencyCluster adjacencyCluster = Subdivided(preparation, name_Bedroom, FlowClassification.Supply, 4);

            Assert.Equal(count_Before, Requirements(Space(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), name_Bedroom)).Count);
        }

        /// <summary>
        /// Re-preparing a subdivided model adds nothing: the second pass creates a terminal only for a
        /// requirement that has <i>none</i>, so four terminals stay four rather than becoming five.
        /// </summary>
        [Fact]
        public void RePreparingASubdividedModel_AddsNoTerminal()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = Subdivided(preparation, name_Bedroom, FlowClassification.Supply, 4);

            PartOIterationPreparation preparation_After = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation_After.Refusal);

            Assert.Equal(4, Analytical.Query.VentilationTerminals(Analytical.Query.VentilationTerminals(preparation_After.AnalyticalModel.AdjacencyCluster, Space(preparation_After.AnalyticalModel, name_Bedroom)), FlowClassification.Supply).Count);
        }

        // =================================================================================================
        // D. The directional runtime realization
        // =================================================================================================

        /// <summary>
        /// <b>The room that is supplied is not extracted, and the room that is extracted is not supplied.</b>
        /// <para>
        /// This is the assertion the previous implementation could not satisfy. Air movements were derived
        /// from <c>CalculatedSupplyAirFlow</c> in BOTH directions, so every bedroom was extracted at its
        /// supply rate and every bathroom supplied at a rate it has no supply terminal for. The dwelling
        /// moved roughly the right total amount of air through the wrong rooms - and a balanced heat
        /// recovery system balances at the SYSTEM, with transfer air moving between the two, not in each
        /// room.
        /// </para>
        /// </summary>
        [Fact]
        public void TheHabitableRoomIsSuppliedOnly_AndTheWetRoomIsExtractedOnly()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //A habitable room: paragraph 1.67 supply, and no extract terminal at all.
            Movements(adjacencyCluster, name_Bedroom, preparation.AirHandlingUnit, out List<SpaceAirMovement> supply_Bedroom, out List<SpaceAirMovement> extract_Bedroom);

            Assert.Single(supply_Bedroom);
            Assert.Empty(extract_Bedroom);

            Assert.Equal(Duty(adjacencyCluster, preparation.AnalyticalModel, name_Bedroom, FlowClassification.Supply) / 1000.0, supply_Bedroom[0].AirFlow, 9);

            //A wet room: paragraph 1.17 extract, and no supply terminal at all.
            Movements(adjacencyCluster, name_Bathroom, preparation.AirHandlingUnit, out List<SpaceAirMovement> supply_Bathroom, out List<SpaceAirMovement> extract_Bathroom);

            Assert.Empty(supply_Bathroom);
            Assert.Single(extract_Bathroom);

            Assert.Equal(Duty(adjacencyCluster, preparation.AnalyticalModel, name_Bathroom, FlowClassification.Extract) / 1000.0, extract_Bathroom[0].AirFlow, 9);
        }

        /// <summary>
        /// The extract movement goes <b>to the unit</b>, not to an unstated destination. That is what heat
        /// recovery recovers from, and it is the only form the destination can usefully take downstream: an
        /// inter-zone air movement moves air INTO the zones it is assigned to, so an extract has to be a
        /// movement on the unit sourced from the room.
        /// </summary>
        [Fact]
        public void TheExtractMovement_NamesTheAirHandlingUnitAsItsDestination()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Movements(adjacencyCluster, name_Bathroom, preparation.AirHandlingUnit, out List<SpaceAirMovement> _, out List<SpaceAirMovement> extract);

            SpaceAirMovement spaceAirMovement = Assert.Single(extract);

            Assert.Equal(new ObjectReference(preparation.AirHandlingUnit).ToString(), spaceAirMovement.To);
            Assert.Equal(new ObjectReference(Space(preparation.AnalyticalModel, name_Bathroom)).ToString(), spaceAirMovement.From);
        }

        /// <summary>
        /// Re-preparing the same model produces the same air movements, not a second set beside them. A
        /// duplicated movement is a duplicated inter-zone air movement in the exported file, which doubles
        /// the ventilation of every room it touches.
        /// </summary>
        [Fact]
        public void RePreparing_ReplacesTheAirMovementsRatherThanDuplicatingThem()
        {
            PartOIterationPreparation preparation = Prepared();

            int count = AirMovements(preparation.AnalyticalModel).Count;

            PartOIterationPreparation preparation_After = Prepare(preparation.AnalyticalModel, PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation_After.Refusal);
            Assert.Equal(count, AirMovements(preparation_After.AnalyticalModel).Count);
        }

        // =================================================================================================
        // E. Topology
        // =================================================================================================

        /// <summary>Every terminal resolves to exactly one space and exactly one system.</summary>
        [Fact]
        public void EveryTerminal_ResolvesToOneSpaceAndOneSystem()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            foreach (VentilationTerminal ventilationTerminal in adjacencyCluster.GetObjects<VentilationTerminal>())
            {
                Assert.Single(adjacencyCluster.GetRelatedObjects<Space>(ventilationTerminal));
                Assert.Single(adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal));
            }
        }

        /// <summary>One system serves every sized space of the dwelling, and owns every one of its terminals.</summary>
        [Fact]
        public void OneSystem_ServesEverySizedSpaceAndOwnsEveryTerminal()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            VentilationSystem ventilationSystem = Assert.Single(adjacencyCluster.GetMechanicalSystems<VentilationSystem>());

            Assert.Equal(4, adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem).Count);

            Assert.Equal(adjacencyCluster.GetObjects<VentilationTerminal>().Count, Analytical.Query.VentilationTerminals(adjacencyCluster, ventilationSystem).Count);
        }

        /// <summary>
        /// The system's duty is the sum of its terminals and it agrees with what Approved Document F sized -
        /// two independent derivations of the same quantity, compared rather than reconciled.
        /// </summary>
        [Fact]
        public void SystemDesignDuty_SumsItsTerminalsAndAgreesWithPartF()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            double supply_Requirement = 0;
            double extract_Requirement = 0;

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData == null)
                {
                    continue;
                }

                supply_Requirement += partFSpaceData.ContinuousSupplyFlowRate_Lps ?? 0;
                extract_Requirement += partFSpaceData.ContinuousExtractFlowRate_Lps ?? 0;
            }

            Assert.Equal(supply_Requirement, preparation.DesignSupplyDuty_Lps, 6);
            Assert.Equal(extract_Requirement, preparation.DesignExtractDuty_Lps, 6);

            Assert.True(preparation.DesignSupplyDuty_Lps > 0);
        }

        /// <summary>
        /// <b>The existing one-system-per-space assumption, pinned rather than left latent.</b>
        /// <c>Create.TPD</c> and <c>Modify.AddAirMovementObjects</c> both read a space's ventilation system
        /// with <c>FirstOrDefault()</c>, so a second system on the same space would be silently ignored by
        /// one of them. On this workflow there is exactly one.
        /// </summary>
        [Fact]
        public void EverySpace_HasAtMostOneVentilationSystem()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.True(adjacencyCluster.GetRelatedObjects<VentilationSystem>(space).Count <= 1);
            }
        }

        /// <summary>
        /// <b>A model that already carries its own ventilation system is not ventilated twice.</b>
        /// <para>
        /// Found by the first licensed Iteration 1a run. The acceptance model arrives carrying the
        /// system-template assignment it was built with - its rooms split across an <c>NV</c> system, an
        /// <c>MV</c> system and a <c>UV</c> system - while the assessment states one MVHR route for the whole
        /// dwelling. Those systems serve the same rooms this iteration does, so realizing every system in the
        /// model gave each shared room <b>two</b> supply movements at once, one from each unit.
        /// </para>
        /// <para>
        /// Three rules together close it, and none of them touches the model's own data. The Base MVHR system
        /// is built rather than one of the model's picked, because attaching Base MVHR terminals to a system
        /// typed <c>NV</c> would be untrue and choosing between three would be a guess. Stale air movements on
        /// the assessed rooms are cleared whatever built them. And the realization is <b>scoped</b> to the
        /// system this iteration built. What the model says is reported as a warning, room by room, and left
        /// exactly as authored.
        /// </para>
        /// </summary>
        [Fact]
        public void AModelAlreadyServedByAVentilationSystem_IsNotVentilatedTwice()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            AirHandlingUnit airHandlingUnit = Analytical.Create.AirHandlingUnit("AHU1");
            adjacencyCluster.AddObject(airHandlingUnit);

            VentilationSystem ventilationSystem = Analytical.Create.MechanicalSystem(new VentilationSystemType("MV", "Mechanical ventilation"), null, "1") as VentilationSystem;
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);
            ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, airHandlingUnit.Name);
            adjacencyCluster.AddObject(ventilationSystem);

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                adjacencyCluster.AddRelation(ventilationSystem, space);
            }

            PartOIterationPreparation preparation = Prepare(new AnalyticalModel(analyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation.Refusal);

            //The Base MVHR system is this iteration's own, and the model's is still there, untouched.
            Assert.NotEqual(ventilationSystem.Guid, preparation.VentilationSystem.Guid);
            Assert.NotEqual(airHandlingUnit.Guid, preparation.AirHandlingUnit.Guid);

            AdjacencyCluster adjacencyCluster_After = preparation.AnalyticalModel.AdjacencyCluster;

            Assert.NotNull(adjacencyCluster_After.GetMechanicalSystems<VentilationSystem>().Find(x => x.Guid == ventilationSystem.Guid));

            //And it is reported rather than left silently contradicting the design.
            Assert.NotEmpty(preparation.Warnings.FindAll(x => x.Contains("is still related to ventilation system")));

            //Exactly one movement per direction per room, from the Base MVHR unit - not one from each unit.
            Assert.Empty(AirMovements(preparation.AnalyticalModel).FindAll(x => x.From == new ObjectReference(airHandlingUnit).ToString() || x.To == new ObjectReference(airHandlingUnit).ToString()));

            Movements(adjacencyCluster_After, name_Bedroom, preparation.AirHandlingUnit, out List<SpaceAirMovement> supply, out List<SpaceAirMovement> extract);

            Assert.Single(supply);
            Assert.Empty(extract);

            Movements(adjacencyCluster_After, name_Bathroom, preparation.AirHandlingUnit, out supply, out extract);

            Assert.Empty(supply);
            Assert.Single(extract);

            //Two movements per assessed room at most, and never four.
            foreach (Space space in adjacencyCluster_After.GetSpaces())
            {
                Assert.True((adjacencyCluster_After.GetRelatedObjects<SpaceAirMovement>(space) ?? new List<SpaceAirMovement>()).Count <= 2);
            }
        }

        /// <summary>
        /// A duty a designer has pushed <b>below</b> the requirement is refused at the system total,
        /// naming both figures. The model would otherwise simulate a dwelling ventilated below the rate
        /// the Approved Document requires of it, and no design intent makes that a legal building.
        /// <para>
        /// <b>The comparison became one-sided at Iteration 2, and this test with it.</b> It previously
        /// pushed the bedroom terminal <i>up</i> by 7.5 l/s and required a refusal, which encoded
        /// <c>Design == Required</c> - the assumption Iteration 2 exists to lift. Design airflow above the
        /// requirement is now design headroom, reported rather than refused, and is what an Approved
        /// Document O iteration spends when it raises a failing room; see
        /// <see cref="PartOVentilationUnitSelectionTests"/> and the test below. Below the requirement
        /// still refuses, which is what this now pins.
        /// </para>
        /// </summary>
        [Fact]
        public void ADutyBelowTheRequirement_Refuses()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            VentilationTerminal ventilationTerminal = Analytical.Query.VentilationTerminals(Analytical.Query.VentilationTerminals(adjacencyCluster, Space(preparation.AnalyticalModel, name_Bedroom)), FlowClassification.Supply)[0];

            ventilationTerminal.DesignFlowRate_Lps -= 2.5;
            adjacencyCluster.AddObject(ventilationTerminal);

            PartOIterationPreparation preparation_After = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.NotNull(preparation_After.Refusal);
            Assert.Null(preparation_After.AnalyticalModel);
            Assert.Contains("design supply duty", preparation_After.Refusal);
            Assert.Contains("below", preparation_After.Refusal);
        }

        /// <summary>
        /// A duty a designer has pushed <b>above</b> the requirement is design headroom: it is reported at
        /// the system total and at the room, and it is not refused.
        /// <para>
        /// The reporting matters as much as the acceptance. The two totals are derived separately precisely
        /// so the gap between them can be read, and an unremarked divergence would be indistinguishable
        /// from a mistake.
        /// </para>
        /// </summary>
        [Fact]
        public void ADutyAboveTheRequirement_IsReportedAsHeadroomAndNotRefused()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            //The bedroom gains supply and the bathroom the matching extract: on a balanced unit the two
            //sides have to stay equal, or Iteration 1a's conservation check refuses for a different reason
            //than the one under test.
            AddDuty(adjacencyCluster, preparation.AnalyticalModel, name_Bedroom, FlowClassification.Supply, 7.5);
            AddDuty(adjacencyCluster, preparation.AnalyticalModel, name_Bathroom, FlowClassification.Extract, 7.5);

            PartOIterationPreparation preparation_After = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation_After.Refusal);
            Assert.NotNull(preparation_After.AnalyticalModel);

            Assert.Equal(preparation.DesignSupplyDuty_Lps + 7.5, preparation_After.DesignSupplyDuty_Lps, 6);
            Assert.Equal(preparation.DesignExtractDuty_Lps + 7.5, preparation_After.DesignExtractDuty_Lps, 6);

            Assert.Contains(preparation_After.Notes, x => x.Contains("designed above the Approved Document F requirement"));
            Assert.Contains(preparation_After.Warnings, x => x.Contains(name_Bedroom) && x.Contains("design headroom"));
        }

        /// <summary>
        /// <b>The Approved Document F floor is per ROOM, and a system total that agrees does not discharge
        /// it.</b>
        /// <para>
        /// One bedroom is designed 2.5 l/s below its requirement and the living room is raised by the same
        /// 2.5 l/s, so the system total matches the requirement total exactly. An earlier revision checked
        /// only that total and passed this model - simulating a bedroom ventilated below the rate the
        /// Approved Document requires of it while reporting compliance. Surplus in one room is not
        /// tradeable against a shortfall in another, and the refusal names the deficient room.
        /// </para>
        /// </summary>
        [Fact]
        public void ARoomBelowItsRequirement_RefusesEvenWhereTheSystemTotalAgrees()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            AddDuty(adjacencyCluster, preparation.AnalyticalModel, name_Bedroom, FlowClassification.Supply, -2.5);
            AddDuty(adjacencyCluster, preparation.AnalyticalModel, name_LivingRoom, FlowClassification.Supply, 2.5);

            PartOIterationPreparation preparation_After = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.NotNull(preparation_After.Refusal);
            Assert.Null(preparation_After.AnalyticalModel);

            //Named, so an engineer is told which room to fix rather than that "something" disagrees.
            Assert.Contains(name_Bedroom, preparation_After.Refusal);
            Assert.Contains("below the", preparation_After.Refusal);

            //And it is the ROOM that is named, not the system total - which still agrees exactly.
            Assert.DoesNotContain("design supply duty", preparation_After.Refusal);
        }

        private static void AddDuty(AdjacencyCluster adjacencyCluster, AnalyticalModel analyticalModel, string name_Space, FlowClassification flowClassification, double increase_Lps)
        {
            VentilationTerminal ventilationTerminal = Analytical.Query.VentilationTerminals(Analytical.Query.VentilationTerminals(adjacencyCluster, Space(analyticalModel, name_Space)), flowClassification)[0];

            ventilationTerminal.DesignFlowRate_Lps += increase_Lps;

            adjacencyCluster.AddObject(ventilationTerminal);
        }

        // =================================================================================================
        // F. Requirement lineage across a Part F recalculation
        // =================================================================================================

        /// <summary>
        /// <c>PartFCalculator</c> mints new requirement guids on every run, so a design terminal's link is
        /// stale the moment Part F is recalculated. It is re-linked to the requirement that <i>replaced</i>
        /// the one it was made from - matched on room, Approved Document role and source paragraph - and the
        /// re-link is <b>reported</b>, never done silently. No terminal is added.
        /// </summary>
        [Fact]
        public void RecalculatingPartF_RelinksTheDesignTerminalsAndReportsIt()
        {
            PartOIterationPreparation preparation = Prepared();

            Guid guid_Terminal = Analytical.Query.VentilationTerminals(preparation.AnalyticalModel.AdjacencyCluster, Space(preparation.AnalyticalModel, name_Bedroom))[0].Guid;

            int count_Before = preparation.AnalyticalModel.AdjacencyCluster.GetObjects<VentilationTerminal>().Count;

            AnalyticalModel analyticalModel = Recalculated(preparation.AnalyticalModel);

            PartOIterationPreparation preparation_After = Prepare(analyticalModel, PartOIteration.BasePassive, "MVRE");

            Assert.Null(preparation_After.Refusal);

            //The same terminals, not new ones.
            Assert.Equal(count_Before, preparation_After.AnalyticalModel.AdjacencyCluster.GetObjects<VentilationTerminal>().Count);
            Assert.Equal(guid_Terminal, Analytical.Query.VentilationTerminals(preparation_After.AnalyticalModel.AdjacencyCluster, Space(preparation_After.AnalyticalModel, name_Bedroom))[0].Guid);

            //And the re-link is on the record.
            Assert.NotEmpty(preparation_After.Notes.FindAll(x => x.Contains("was re-linked from Approved Document F requirement")));
        }

        /// <summary>
        /// A design terminal whose requirement no longer exists <b>refuses</b>. Deleting it and re-running
        /// the Part F calculation are different answers to that situation and only an engineer can choose
        /// between them, so nothing is quietly dropped and nothing is quietly kept.
        /// </summary>
        [Fact]
        public void ATerminalWhoseRequirementNoLongerExists_Refuses()
        {
            PartOIterationPreparation preparation = Prepared();

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Bedroom);

            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            partFSpaceData.Terminals.RemoveAll(x => x.TerminalRole == PartFTerminalRole.Supply);

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);
            adjacencyCluster.AddObject(space);

            PartOIterationPreparation preparation_After = Prepare(new AnalyticalModel(preparation.AnalyticalModel, adjacencyCluster), PartOIteration.BasePassive, "MVRE");

            Assert.NotNull(preparation_After.Refusal);
            Assert.Null(preparation_After.AnalyticalModel);
            Assert.Contains("no such requirement", preparation_After.Refusal);
        }

        // =================================================================================================
        // G. Generic MEP safety
        // =================================================================================================

        /// <summary>
        /// <b>A Part F calculation on its own creates requirements and nothing else.</b> No design terminal,
        /// no ventilation system, no air movement, and not one internal condition touched. The realization
        /// happens because the Base MVHR operating scenario asks for design-rate operation - never because
        /// a space happens to carry an airflow.
        /// </summary>
        [Fact]
        public void ThePartFCalculationAlone_CreatesNoDesignTopologyAndNoRuntimeVentilation()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.NotEmpty(Requirements(Space(analyticalModel, name_Bedroom)));

            //GetObjects<T> answers null rather than an empty list where the cluster holds no object of that
            //type at all, and "holds none" is exactly the state under test here.
            Assert.Empty(Objects<VentilationTerminal>(adjacencyCluster));
            Assert.Empty(Objects<SpaceAirMovement>(adjacencyCluster));
            Assert.Empty(Objects<AirHandlingUnit>(adjacencyCluster));
            Assert.Empty(adjacencyCluster.GetMechanicalSystems<VentilationSystem>() ?? new List<VentilationSystem>());

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.False(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlow, out double _));
                Assert.False(space.InternalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlow, out double _));
            }
        }

        /// <summary>
        /// The natural ventilation route builds none of it either, so Iteration 1b's model is exactly what it
        /// was: no terminals, no system, no air movements, no mechanical airflow.
        /// </summary>
        [Fact]
        public void TheNaturalVentilationRoute_BuildsNoMechanicalTopology()
        {
            PartOIterationPreparation preparation = Prepare(Model(), PartOIteration.BaseNaturalVentilation, "NV");

            Assert.Null(preparation.Refusal);
            Assert.True(preparation.Successful);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Assert.Empty(Objects<VentilationTerminal>(adjacencyCluster));
            Assert.Empty(Objects<SpaceAirMovement>(adjacencyCluster));
            Assert.Empty(preparation.VentilationTerminals);
            Assert.Null(preparation.VentilationSystem);
            Assert.True(double.IsNaN(preparation.DesignSupplyDuty_Lps));

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                Assert.False(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlow, out double _));
            }
        }

        /// <summary>
        /// <b>A model with a ventilation system but no design terminals keeps the behaviour it always had:</b>
        /// the space's calculated supply airflow, in both directions, with the outward movement naming no
        /// destination. Every generic MEP model reaches that branch and none of them may notice that the
        /// terminal branch exists.
        /// </summary>
        [Fact]
        public void AModelWithNoDesignTerminals_KeepsTheLegacyAirMovementBehaviour()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Space> spaces = adjacencyCluster.GetSpaces();

            //A supply rate stated the generic way - on the internal condition, with no Part F application
            //and no design terminal behind it.
            foreach (Space space in spaces)
            {
                InternalCondition internalCondition = space.InternalCondition;
                internalCondition.SetValue(InternalConditionParameter.SupplyAirFlow, 0.05);
                space.InternalCondition = internalCondition;
                adjacencyCluster.AddObject(space);
            }

            AirHandlingUnit airHandlingUnit = Analytical.Create.AirHandlingUnit("AHU-01");
            adjacencyCluster.AddObject(airHandlingUnit);

            VentilationSystem ventilationSystem = Analytical.Create.MechanicalSystem(new VentilationSystemType("MV", "Mechanical ventilation"), null, "1") as VentilationSystem;
            ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, airHandlingUnit.Name);
            adjacencyCluster.AddObject(ventilationSystem);

            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                adjacencyCluster.AddRelation(ventilationSystem, space);
            }

            Analytical.Modify.AddAirMovementObjects(adjacencyCluster, analyticalModel.ProfileLibrary);

            //Two movements per space, both at the supply figure, and the second one with no destination -
            //exactly as before.
            foreach (Space space in adjacencyCluster.GetSpaces())
            {
                List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(space);

                Assert.Equal(2, spaceAirMovements.Count);

                Assert.All(spaceAirMovements, x => Assert.Equal(0.05, x.AirFlow, 9));
                Assert.Single(spaceAirMovements.FindAll(x => x.To == null));
            }
        }

        // =================================================================================================
        // H. The filtered-space guard
        // =================================================================================================

        /// <summary>
        /// <c>Modify.AssignMechanicalSystem</c> tested <c>spaces_Filtered == null</c> - the list it had just
        /// constructed, which is never null - where it meant <c>space_Filtered</c>. A space that is not in
        /// the cluster was therefore added to the filtered list as <c>null</c> instead of being skipped.
        /// It is skipped.
        /// </summary>
        [Fact]
        public void AssigningASystemToASpaceNotInTheCluster_SkipsThatSpace()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Space> spaces = adjacencyCluster.GetSpaces();

            VentilationSystemType ventilationSystemType = new VentilationSystemType("MV", "Mechanical ventilation");

            MechanicalSystem mechanicalSystem = Analytical.Create.MechanicalSystem(ventilationSystemType, null, "1");

            //One space of the model, and one that belongs to no model at all.
            Analytical.Modify.AssignMechanicalSystem(adjacencyCluster, mechanicalSystem, new List<Space> { spaces[0], new Space("Not In This Model") });

            List<Space> spaces_Related = adjacencyCluster.GetRelatedObjects<Space>(mechanicalSystem);

            Assert.Single(spaces_Related);
            Assert.Equal(spaces[0].Guid, spaces_Related[0].Guid);
        }

        // =================================================================================================
        // I. Reuse reconciles to the current scope
        // =================================================================================================

        /// <summary>
        /// <b>Reproduces the reported sequence: prepare a wider scope, then re-prepare a narrower one
        /// over the same system.</b>
        /// <para>
        /// <c>AddPartOBaseMVHRSystem</c> recognises an existing system by relation to the design terminals
        /// of the spaces it is CALLED with - so a first call naming two served spaces and a second, later
        /// call naming only one of them still finds and reuses the very same system (its design terminals
        /// belong to it regardless of which caller passed them in). Before this fix, the "Connect" step
        /// only ADDED the narrower call's relations; it never removed what the wider call had left, so the
        /// reused system's membership was the accumulated union of both scopes rather than the current one.
        /// </para>
        /// </summary>
        [Fact]
        public void ReusingTheSystemWithANarrowerSpaceScope_RemovesTheStaleOutOfScopeRelations()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<VentilationTerminal> ventilationTerminals = adjacencyCluster.RealizePartFVentilationTerminals(null, out List<string> _, out List<string> refusals_Terminals);

            Assert.Empty(refusals_Terminals);
            Assert.NotEmpty(ventilationTerminals);

            List<Space> spaces = adjacencyCluster.GetSpaces();

            Space space_Bedroom = spaces.Find(x => x.Name == name_Bedroom);
            Space space_Bathroom = spaces.Find(x => x.Name == name_Bathroom);

            Assert.NotEmpty(Analytical.Query.VentilationTerminals(adjacencyCluster, space_Bedroom));
            Assert.NotEmpty(Analytical.Query.VentilationTerminals(adjacencyCluster, space_Bathroom));

            //The wider, "whole model" call: both served spaces in scope.
            VentilationSystem ventilationSystem = adjacencyCluster.AddPartOBaseMVHRSystem(
                new List<Space> { space_Bedroom, space_Bathroom },
                out AirHandlingUnit airHandlingUnit,
                out List<string> _,
                out List<string> _,
                out List<string> refusals_1);

            Assert.NotNull(ventilationSystem);
            Assert.Empty(refusals_1);

            List<Space> spaces_Served_Before = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? new List<Space>();

            Assert.Contains(spaces_Served_Before, x => x.Guid == space_Bathroom.Guid);
            Assert.Contains(spaces_Served_Before, x => x.Guid == space_Bedroom.Guid);

            //The narrower, "subset" re-prepare: only Bedroom named this time.
            VentilationSystem ventilationSystem_After = adjacencyCluster.AddPartOBaseMVHRSystem(
                new List<Space> { space_Bedroom },
                out AirHandlingUnit airHandlingUnit_After,
                out List<string> notes_After,
                out List<string> _,
                out List<string> refusals_2);

            Assert.Empty(refusals_2);

            //The SAME system and unit, reused rather than a second one being built.
            Assert.Equal(ventilationSystem.Guid, ventilationSystem_After.Guid);
            Assert.Equal(airHandlingUnit.Guid, airHandlingUnit_After.Guid);

            List<Space> spaces_Served_After = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem_After) ?? new List<Space>();

            //No stale out-of-scope served-space relation.
            Assert.DoesNotContain(spaces_Served_After, x => x.Guid == space_Bathroom.Guid);
            Assert.Contains(spaces_Served_After, x => x.Guid == space_Bedroom.Guid);

            //No stale terminal relation: none of Bathroom's own terminals are still related to the system.
            List<VentilationTerminal> terminals_Bathroom = Analytical.Query.VentilationTerminals(adjacencyCluster, space_Bathroom);
            List<VentilationTerminal> terminals_System_After = adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem_After) ?? new List<VentilationTerminal>();

            foreach (VentilationTerminal ventilationTerminal_Bathroom in terminals_Bathroom)
            {
                Assert.DoesNotContain(terminals_System_After, x => x.Guid == ventilationTerminal_Bathroom.Guid);
            }

            //And it is on the record, not a silent removal.
            Assert.Contains(notes_After, x => x.Contains(space_Bathroom.Name) && x.Contains("removed"));
        }

        /// <summary>
        /// Idempotence: re-preparing the SAME scope a second time removes nothing and adds nothing beside
        /// it - the reconciliation is a no-op when the scope has not actually narrowed.
        /// </summary>
        [Fact]
        public void ReusingTheSystemWithTheSameSpaceScope_ChangesNoRelation()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            adjacencyCluster.RealizePartFVentilationTerminals(null, out List<string> _, out List<string> _);

            List<Space> spaces = adjacencyCluster.GetSpaces();

            Space space_Bedroom = spaces.Find(x => x.Name == name_Bedroom);
            Space space_Bathroom = spaces.Find(x => x.Name == name_Bathroom);

            List<Space> scope = new List<Space> { space_Bedroom, space_Bathroom };

            VentilationSystem ventilationSystem = adjacencyCluster.AddPartOBaseMVHRSystem(scope, out AirHandlingUnit _, out List<string> _, out List<string> _, out List<string> _);

            List<Space> spaces_Served_Before = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? new List<Space>();

            VentilationSystem ventilationSystem_After = adjacencyCluster.AddPartOBaseMVHRSystem(scope, out AirHandlingUnit _, out List<string> notes_After, out List<string> _, out List<string> refusals_After);

            Assert.Empty(refusals_After);
            Assert.Equal(ventilationSystem.Guid, ventilationSystem_After.Guid);

            List<Space> spaces_Served_After = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem_After) ?? new List<Space>();

            Assert.Equal(spaces_Served_Before.Count, spaces_Served_After.Count);
            Assert.DoesNotContain(notes_After, x => x.Contains("removed"));
        }

        // =================================================================================================
        // Fixture
        // =================================================================================================

        /// <summary>The dwelling, prepared at Iteration 1a over the stated route.</summary>
        private static PartOIterationPreparation Prepared(string ventilationStrategy = "MVRE")
        {
            PartOIterationPreparation result = Prepare(Model(), PartOIteration.BasePassive, ventilationStrategy);

            Assert.Null(result.Refusal);
            Assert.NotNull(result.AnalyticalModel);

            return result;
        }

        /// <summary>
        /// The production preparation, called exactly as <c>SAMAnalytical.PreparePartOIteration</c> calls it.
        /// </summary>
        private static PartOIterationPreparation Prepare(AnalyticalModel analyticalModel, PartOIteration partOIteration, string ventilationStrategy)
        {
            List<Zone> zones = analyticalModel.GetZones();

            Assert.NotEmpty(zones);

            Dictionary<Guid, string> dictionary = new Dictionary<Guid, string>();
            foreach (Zone zone in zones)
            {
                dictionary[zone.Guid] = ventilationStrategy;
            }

            return analyticalModel.PreparePartOIteration(partOIteration, null, dictionary);
        }

        /// <summary>
        /// One flat with the two shapes that matter: habitable rooms Approved Document F gives a supply
        /// terminal and no extract, and wet rooms it gives an extract terminal and no supply. Sized by the
        /// real <see cref="PartFCalculator"/>, so the requirements under test are the ones production makes.
        /// </summary>
        private static AnalyticalModel Model(string ventilationSystemTypeName = "MVRE")
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            //Named so the shared space-use classification recognises them.
            Dictionary<string, double> dictionary = new Dictionary<string, double>()
            {
                { name_LivingRoom, 30.0 },
                { name_Bedroom, 16.0 },
                { name_Kitchen, 12.0 },
                { name_Bathroom, 6.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new Space(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                InternalCondition internalCondition = new InternalCondition(keyValuePair.Key + " IC");

                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);

                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            //A flat, not a bag of loose rooms: every room opens off the living room. Without internal
            //partitions the dwelling has no transfer air network, so the air supplied into the bedroom has
            //nowhere to go and the air extracted from the bathroom has nowhere to come from - and the
            //preparation refuses, exactly as TAS refuses a zone whose air movements do not balance.
            Helpers.DwellingPartitions.Star(adjacencyCluster, name_LivingRoom, name_Bedroom, name_Kitchen, name_Bathroom);

            AnalyticalModel analyticalModel = new AnalyticalModel("Part O Base MVHR Dwelling", null, null, null, adjacencyCluster, null, new ProfileLibrary("Part O Base MVHR Fixture"));

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            AdjacencyCluster adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;

            Zone zone = new Zone(name_Zone);
            adjacencyCluster_Sized.AddObject(zone);

            //Related to every space this fixture builds, matching how a real model's own zoning relates a
            //dwelling zone to its rooms - Modify.PrepareBaseMVHR partitions the assessed scope by exactly
            //this relation (Query.PartFDwellingZones plus the zone -> space relation).
            foreach (Space space_Existing in adjacencyCluster_Sized.GetSpaces())
            {
                adjacencyCluster_Sized.AddRelation(zone, space_Existing);
            }

            return new AnalyticalModel(analyticalModel, adjacencyCluster_Sized);
        }

        /// <summary>The same model with the Part F calculation run again over it, minting fresh requirement guids.</summary>
        private static AnalyticalModel Recalculated(AnalyticalModel analyticalModel)
        {
            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate());

            return new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster);
        }

        /// <summary>
        /// Replaces one space's terminals of one direction with <paramref name="count"/> terminals sharing
        /// the duty and the requirement lineage - what a designer does when a room needs more than one
        /// diffuser.
        /// </summary>
        private static AdjacencyCluster Subdivided(PartOIterationPreparation preparation, string name_Space, FlowClassification flowClassification, int count)
        {
            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);

            List<VentilationTerminal> ventilationTerminals = Analytical.Query.VentilationTerminals(Analytical.Query.VentilationTerminals(adjacencyCluster, space), flowClassification);

            VentilationTerminal ventilationTerminal = Assert.Single(ventilationTerminals);

            VentilationSystem ventilationSystem = adjacencyCluster.GetRelatedObjects<VentilationSystem>(ventilationTerminal)[0];

            PartFTerminalReference partFTerminalReference = ventilationTerminal.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);

            double share = ventilationTerminal.DesignFlowRate_Lps.Value / count;

            ventilationTerminal.DesignFlowRate_Lps = share;
            adjacencyCluster.AddObject(ventilationTerminal);

            for (int index = 2; index <= count; index++)
            {
                VentilationTerminal ventilationTerminal_Extra = new VentilationTerminal(string.Format("{0} ({1})", ventilationTerminal.Name, index), flowClassification, share);
                ventilationTerminal_Extra.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(partFTerminalReference));

                adjacencyCluster.AddObject(ventilationTerminal_Extra);
                adjacencyCluster.AddRelation(ventilationTerminal_Extra, space);
                adjacencyCluster.AddRelation(ventilationTerminal_Extra, ventilationSystem);
            }

            return adjacencyCluster;
        }

        /// <summary>The air movements of one space, split by direction relative to the unit.</summary>
        private static void Movements(AdjacencyCluster adjacencyCluster, string name_Space, AirHandlingUnit airHandlingUnit, out List<SpaceAirMovement> supply, out List<SpaceAirMovement> extract)
        {
            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == name_Space);

            string reference_AirHandlingUnit = new ObjectReference(airHandlingUnit).ToString();

            supply = new List<SpaceAirMovement>();
            extract = new List<SpaceAirMovement>();

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(space) ?? new List<SpaceAirMovement>())
            {
                if (spaceAirMovement.From == reference_AirHandlingUnit)
                {
                    supply.Add(spaceAirMovement);
                }
                else if (spaceAirMovement.To == reference_AirHandlingUnit)
                {
                    extract.Add(spaceAirMovement);
                }
            }
        }

        private static double Duty(AdjacencyCluster adjacencyCluster, AnalyticalModel analyticalModel, string name_Space, FlowClassification flowClassification)
        {
            return Analytical.Query.VentilationTerminalDesignDuty_Lps(Analytical.Query.VentilationTerminals(adjacencyCluster, Space(analyticalModel, name_Space)), flowClassification).Value;
        }

        private static List<SpaceAirMovement> AirMovements(AnalyticalModel analyticalModel)
        {
            return Objects<SpaceAirMovement>(analyticalModel.AdjacencyCluster);
        }

        /// <summary>
        /// Every object of one type, as a list. <c>GetObjects&lt;T&gt;</c> answers <b>null</b> rather than an
        /// empty list where the cluster holds none of that type, and "holds none" is the state several of
        /// these tests are about.
        /// </summary>
        private static List<T> Objects<T>(AdjacencyCluster adjacencyCluster) where T : Core.IJSAMObject
        {
            return adjacencyCluster.GetObjects<T>() ?? new List<T>();
        }

        private static Space Space(AnalyticalModel analyticalModel, string name)
        {
            Space result = analyticalModel.GetSpaces().Find(x => x.Name == name);

            Assert.NotNull(result);

            return result;
        }

        private static List<PartFVentilationTerminalRequirement> Requirements(Space space)
        {
            return space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)?.Terminals ?? new List<PartFVentilationTerminalRequirement>();
        }

        private static PartFVentilationTerminalRequirement Requirement(Space space, PartFTerminalRole partFTerminalRole)
        {
            PartFVentilationTerminalRequirement result = Requirements(space).Find(x => x.TerminalRole == partFTerminalRole);

            Assert.NotNull(result);

            return result;
        }

        /// <summary>Every space's Approved Document F data, serialised, keyed by space name.</summary>
        private static Dictionary<string, string> PartFJson(AnalyticalModel analyticalModel)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (Space space in analyticalModel.GetSpaces())
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData != null)
                {
                    result[space.Name] = Core.Convert.ToString(partFSpaceData);
                }
            }

            Assert.NotEmpty(result);

            return result;
        }
    }
}
