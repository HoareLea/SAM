// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>The join between the Part F sizing and the simulation.</b>
    /// <para>
    /// <c>PartFCalculator</c> has always written its sizing onto each space as <c>PartFSpaceData</c>, in litres per
    /// second, per terminal. The simulation has always read airflow through
    /// <c>Query.CalculatedSupplyAirFlow</c>, in cubic metres per second, off the space's
    /// <c>InternalCondition</c> - and that query never looked at <c>PartFSpaceData</c>. So the two representations
    /// were disconnected: a Part-F-sized model simulated with whatever its internal conditions happened to say,
    /// and the Part F numbers were reporting only.
    /// </para>
    /// <para>
    /// These tests cover the bridge, and the two hazards that make it more than a unit conversion: the summing
    /// query, and shared internal conditions.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with <c>PartOIterationPreparationTests</c> - see that class for why the two
    /// readers of the default Part F rule set must not run at the same time.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartFAirflowApplicationTests
    {
        /// <summary>
        /// <b>End to end on the real calculator: sized, applied, and readable by the query the simulation uses.</b>
        /// <para>
        /// Nothing is stubbed - <c>Query.DefaultPartFCalculator</c> loads the shipped rule set and sizes the
        /// dwelling, and the rates that come out the far side are read back through
        /// <c>Query.CalculatedSupplyAirFlow</c>, which is what the TAS export consumes.
        /// </para>
        /// </summary>
        [Fact]
        public void PartFSizedRates_ReachTheQueryTheSimulationReads()
        {
            AnalyticalModel analyticalModel = Sized();

            //Before: the sizing is on the model, but the simulation cannot see it.
            Space space_Bedroom_Before = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");

            Assert.NotNull(space_Bedroom_Before.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData));

            AnalyticalModel analyticalModel_Applied = analyticalModel.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> refusals, out List<string> notes);

            Assert.NotNull(analyticalModel_Applied);
            Assert.Empty(refusals);
            Assert.NotEmpty(notes);

            //After: a habitable room's supply is the Part F continuous design supply, in m3/s.
            Space space_Bedroom = analyticalModel_Applied.GetSpaces().Find(x => x.Name == "Bedroom 1");
            PartFSpaceData partFSpaceData = space_Bedroom.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.True(partFSpaceData.ContinuousSupplyFlowRate_Lps > 0, "The fixture did not size a supply rate, so this test would prove nothing.");

            Assert.Equal(partFSpaceData.ContinuousSupplyFlowRate_Lps.Value / 1000.0, space_Bedroom.CalculatedSupplyAirFlow(), 9);
        }

        /// <summary>
        /// <b>A wet room gets its extract, and an explicit zero supply.</b>
        /// <para>
        /// Under the balanced MVHR arrangement Part F sizes, a wet room's make-up air arrives as transfer air
        /// through the internal door, not as supply. Leaving a stale supply rate there would model a room
        /// ventilated twice.
        /// </para>
        /// </summary>
        [Fact]
        public void AWetRoom_GetsExtractAndAnExplicitZeroSupply()
        {
            AnalyticalModel analyticalModel = Sized().ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            Space space = analyticalModel.GetSpaces().Find(x => x.Name == "Bathroom");
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.True(partFSpaceData.ContinuousExtractFlowRate_Lps > 0, "The fixture did not size an extract rate, so this test would prove nothing.");

            Assert.True(space.InternalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlow, out double exhaust));
            Assert.Equal(partFSpaceData.ContinuousExtractFlowRate_Lps.Value / 1000.0, exhaust, 9);

            Assert.Equal(0.0, space.CalculatedSupplyAirFlow(), 9);
        }

        /// <summary>
        /// <b>THE SUMMING HAZARD.</b>
        /// <para>
        /// <c>CalculatedSupplyAirFlow</c> <i>adds</i> <c>SupplyAirFlow</c>, <c>SupplyAirFlowPerPerson</c>,
        /// <c>SupplyAirFlowPerArea</c> and <c>SupplyAirChangesPerHour</c>. A Part F rate written alongside an
        /// existing per-area rate would therefore over-ventilate the room, and silently. The other bases are
        /// cleared, and the space is named in the notes so the displacement is visible.
        /// </para>
        /// </summary>
        [Fact]
        public void AnExistingAirflowBasis_IsClearedAndReportedRatherThanAddedTo()
        {
            //A per-area supply rate that would otherwise be added to the Part F rate.
            AnalyticalModel analyticalModel = Sized(supplyAirFlowPerArea: 0.005);

            Space space_Before = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");
            double airFlow_Before = space_Before.CalculatedSupplyAirFlow();

            AnalyticalModel analyticalModel_Applied = analyticalModel.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> notes);

            Space space = analyticalModel_Applied.GetSpaces().Find(x => x.Name == "Bedroom 1");
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            double expected = partFSpaceData.ContinuousSupplyFlowRate_Lps.Value / 1000.0;

            //The Part F rate exactly - not the Part F rate plus what was there.
            Assert.Equal(expected, space.CalculatedSupplyAirFlow(), 9);
            Assert.True(airFlow_Before > 0, "The fixture did not state a competing rate, so this test would prove nothing.");

            //And the displacement is on the record.
            Assert.Contains(notes, x => x.Contains("Bedroom 1") && x.Contains("replacing"));
        }

        /// <summary>
        /// <b>THE SHARED INTERNAL CONDITION HAZARD.</b>
        /// <para>
        /// Part F rates are per room, but internal conditions are routinely shared between rooms. Writing in place
        /// would have one bedroom's rate overwrite another's - and in a block of flats that is the normal case.
        /// Each sized space gets its own clone, so two bedrooms sharing one condition end up with the rates their
        /// own volumes earned.
        /// </para>
        /// </summary>
        [Fact]
        public void TwoRoomsSharingAnInternalCondition_GetTheirOwnRates()
        {
            //Bedroom 1 and Bedroom 2 share one internal condition object, and have different volumes - so Part F
            //distributes different supply rates to them (paragraph 1.67, in proportion to habitable room volume).
            AnalyticalModel analyticalModel = Sized(shareInternalCondition: true).ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            Space space_1 = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");
            Space space_2 = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 2");

            double supply_1 = space_1.CalculatedSupplyAirFlow();
            double supply_2 = space_2.CalculatedSupplyAirFlow();

            //Each got its own Part F rate.
            Assert.Equal(space_1.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData).ContinuousSupplyFlowRate_Lps.Value / 1000.0, supply_1, 9);
            Assert.Equal(space_2.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData).ContinuousSupplyFlowRate_Lps.Value / 1000.0, supply_2, 9);

            //Different volumes, so different rates - which is the whole point: one shared condition could not have
            //carried both.
            Assert.NotEqual(supply_1, supply_2);

            //And the conditions really are separate objects now, with distinguishable names.
            Assert.NotEqual(space_1.InternalCondition.Guid, space_2.InternalCondition.Guid);
            Assert.NotEqual(space_1.InternalCondition.Name, space_2.InternalCondition.Name);
        }

        /// <summary>
        /// <b>THE NAME-COLLISION HAZARD.</b>
        /// <para>
        /// TAS identifies an internal condition by its NAME, and an untouched (unsized) space keeps its
        /// condition. If such a condition already carries a name matching the generated
        /// "<c>&lt;condition&gt; - &lt;space&gt;</c>" pattern, the generated clone must not reuse it - two
        /// different conditions under one name would associate one room with the other's gains and airflow.
        /// Existing names seed the name set, so the sized room gets a disambiguated clone instead.
        /// </para>
        /// </summary>
        [Fact]
        public void AnExistingConditionWithTheGeneratedName_IsNotReusedForTheClone()
        {
            AnalyticalModel analyticalModel = Sized();

            //An untouched space whose condition happens to be named exactly what the sized Bedroom 1's
            //clone would be generated as. Unsized - no PartFSpaceData - so it is left alone by the apply.
            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space_Untouched = new("Storage");
            space_Untouched.SetValue(SpaceParameter.Area, 5.0);
            space_Untouched.SetValue(SpaceParameter.Volume, 12.5);
            space_Untouched.InternalCondition = new InternalCondition("Bedroom 1 IC - Bedroom 1");
            adjacencyCluster.AddObject(space_Untouched);

            AnalyticalModel analyticalModel_Applied = new AnalyticalModel(analyticalModel, adjacencyCluster)
                .ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> refusals, out List<string> _);

            Assert.Empty(refusals);

            Space space_Bedroom = analyticalModel_Applied.GetSpaces().Find(x => x.Name == "Bedroom 1");
            Space space_Untouched_Applied = analyticalModel_Applied.GetSpaces().Find(x => x.Name == "Storage");

            //The untouched space keeps its condition untouched...
            Assert.Equal("Bedroom 1 IC - Bedroom 1", space_Untouched_Applied.InternalCondition.Name);

            //...and the clone is disambiguated instead of colliding with it - same name is the failure mode.
            Assert.NotEqual("Bedroom 1 IC - Bedroom 1", space_Bedroom.InternalCondition.Name);
            Assert.StartsWith("Bedroom 1 IC - Bedroom 1 (", space_Bedroom.InternalCondition.Name);
            Assert.NotEqual(space_Untouched_Applied.InternalCondition.Guid, space_Bedroom.InternalCondition.Guid);
        }

        /// <summary>The original model is not modified - an updated copy is returned, as the Part F components do.</summary>
        [Fact]
        public void TheSuppliedModel_IsNotModified()
        {
            AnalyticalModel analyticalModel = Sized(supplyAirFlowPerArea: 0.005);

            double airFlow_Before = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1").CalculatedSupplyAirFlow();

            analyticalModel.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            Assert.Equal(airFlow_Before, analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1").CalculatedSupplyAirFlow(), 9);
        }

        // ---------------------------------------------------------------------------------------------
        // Re-application idempotence
        //
        // The Part O workflow re-prepares the model it already prepared: the user can prepare twice, and an
        // Iteration 2B optimisation re-prepares once per round. The generated "<condition> - <space>" name
        // must be a fixed point of the application, not grow by one suffix per pass.
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>f(x) == f(f(x)).</b> Applying the rates to a model this method already applied them to keeps
        /// every generated internal-condition name exactly as it was - not "<c>Studio - Studio 1_0 - Studio
        /// 1_0 - ...</c>", one suffix longer per pass, which is what the production TM59 report showed.
        /// </summary>
        [Fact]
        public void Reapplication_KeepsTheGeneratedNames()
        {
            AnalyticalModel analyticalModel_Once = Sized().ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);
            AnalyticalModel analyticalModel_Twice = analyticalModel_Once.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);
            AnalyticalModel analyticalModel_Thrice = analyticalModel_Twice.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            foreach (Space space_Once in analyticalModel_Once.GetSpaces())
            {
                Space space_Twice = analyticalModel_Twice.GetSpaces().Find(x => x.Guid == space_Once.Guid);
                Space space_Thrice = analyticalModel_Thrice.GetSpaces().Find(x => x.Guid == space_Once.Guid);

                Assert.NotNull(space_Twice);
                Assert.NotNull(space_Thrice);

                Assert.Equal(space_Once.InternalCondition.Name, space_Twice.InternalCondition.Name);
                Assert.Equal(space_Once.InternalCondition.Name, space_Thrice.InternalCondition.Name);

                //The authored condition name survives as the base, with the space name applied exactly once.
                Assert.Equal(space_Once.Name + " IC - " + space_Once.Name, space_Thrice.InternalCondition.Name);
            }
        }

        /// <summary>
        /// <b>Legacy damage heals.</b> A model already carrying a multiplied name - written while the suffix
        /// was appended on every pass - collapses back to the intended name on the next application rather
        /// than growing further.
        /// </summary>
        [Fact]
        public void Reapplication_HealsAnAlreadyMultipliedName()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            space.InternalCondition = new InternalCondition("Bedroom 1 IC - Bedroom 1 - Bedroom 1");
            adjacencyCluster.AddObject(space);

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            partFCalculator.AdjacencyCluster = adjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so this test would prove nothing.");

            AnalyticalModel analyticalModel_Applied = new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster)
                .ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            Assert.Equal("Bedroom 1 IC - Bedroom 1", analyticalModel_Applied.GetSpaces().Find(x => x.Name == "Bedroom 1").InternalCondition.Name);
        }

        /// <summary>
        /// <b>The disambiguated case is stable too.</b> Two rooms sharing one name and one condition are
        /// numbered on the first application; the second application must produce the same SET of names -
        /// which room gets the bare name and which gets " (2)" may follow the space order, so it is the set
        /// that is pinned.
        /// </summary>
        [Fact]
        public void Reapplication_OfADisambiguatedName_StaysStable()
        {
            AnalyticalModel analyticalModel = Model();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            //A second "Bedroom 1", sharing the first's condition - the shape that takes the " (2)" suffix.
            InternalCondition internalCondition_Shared = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1").InternalCondition;

            Space space_Duplicate = new("Bedroom 1");
            space_Duplicate.SetValue(SpaceParameter.Area, 14.0);
            space_Duplicate.SetValue(SpaceParameter.Volume, 35.0);
            space_Duplicate.InternalCondition = internalCondition_Shared;
            adjacencyCluster.AddObject(space_Duplicate);

            AnalyticalModel analyticalModel_Sized = new AnalyticalModel(analyticalModel, adjacencyCluster);

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();
            partFCalculator.AdjacencyCluster = analyticalModel_Sized.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so this test would prove nothing.");

            AnalyticalModel analyticalModel_Once = new AnalyticalModel(analyticalModel_Sized, partFCalculator.AdjacencyCluster)
                .ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            List<string> names_Once = analyticalModel_Once.GetSpaces().ConvertAll(x => x.InternalCondition?.Name);

            //The disambiguation really happened: two conditions answering to the same generated base name.
            Assert.Contains("Bedroom 1 IC - Bedroom 1", names_Once);
            Assert.Contains("Bedroom 1 IC - Bedroom 1 (2)", names_Once);

            AnalyticalModel analyticalModel_Twice = analyticalModel_Once.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            List<string> names_Twice = analyticalModel_Twice.GetSpaces().ConvertAll(x => x.InternalCondition?.Name);

            names_Once.Sort(System.StringComparer.Ordinal);
            names_Twice.Sort(System.StringComparer.Ordinal);

            Assert.Equal(names_Once, names_Twice);
        }

        /// <summary>
        /// <b>A real collision is still refused, not grown.</b> The reservation of names that survive the
        /// call is what the idempotence fix must not weaken: an untouched space's condition keeps its name,
        /// and the sized room's clone is disambiguated around it - on every application, including the
        /// second.
        /// </summary>
        [Fact]
        public void Reapplication_StillRespectsAnUntouchedConditionWithTheGeneratedName()
        {
            AnalyticalModel analyticalModel = Sized();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space_Untouched = new("Storage");
            space_Untouched.SetValue(SpaceParameter.Area, 5.0);
            space_Untouched.SetValue(SpaceParameter.Volume, 12.5);
            space_Untouched.InternalCondition = new InternalCondition("Bedroom 1 IC - Bedroom 1");
            adjacencyCluster.AddObject(space_Untouched);

            AnalyticalModel analyticalModel_Once = new AnalyticalModel(analyticalModel, adjacencyCluster)
                .ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            AnalyticalModel analyticalModel_Twice = analyticalModel_Once.ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> _, out List<string> _);

            Space space_Bedroom = analyticalModel_Twice.GetSpaces().Find(x => x.Name == "Bedroom 1");
            Space space_Untouched_Applied = analyticalModel_Twice.GetSpaces().Find(x => x.Name == "Storage");

            Assert.Equal("Bedroom 1 IC - Bedroom 1", space_Untouched_Applied.InternalCondition.Name);
            Assert.NotEqual("Bedroom 1 IC - Bedroom 1", space_Bedroom.InternalCondition.Name);
            Assert.StartsWith("Bedroom 1 IC - Bedroom 1 (", space_Bedroom.InternalCondition.Name);

            //And stable on the second application: no climbing disambiguation index.
            Assert.Equal(analyticalModel_Once.GetSpaces().Find(x => x.Name == "Bedroom 1").InternalCondition.Name, space_Bedroom.InternalCondition.Name);
        }


        /// <summary>
        /// <b>Measured commissioning rates are refused.</b> They are evidence recorded from site, and driving them
        /// into a design simulation would report a measurement as a design intent.
        /// </summary>
        [Fact]
        public void MeasuredCommissioningRates_AreRefused()
        {
            Assert.Null(Sized().ApplyPartFVentilationRates(PartFOperatingMode.MeasuredCommissioning, out List<string> refusals, out List<string> _));
            Assert.Single(refusals);
        }

        /// <summary>A model that was never sized refuses, naming the component to run first.</summary>
        [Fact]
        public void AnUnsizedModel_RefusesAndSaysWhatToRunFirst()
        {
            Assert.Null(Model().ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> refusals, out List<string> _));
            Assert.Contains(refusals, x => x.Contains("Part F calculation"));
        }

        /// <summary>
        /// A sized space with no internal condition is refused rather than given an invented one - a condition
        /// carries occupancy, gains and setpoints, none of which this is entitled to decide.
        /// </summary>
        [Fact]
        public void ASizedSpaceWithNoInternalCondition_IsRefused()
        {
            AnalyticalModel analyticalModel = Sized();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            Space space = adjacencyCluster.GetSpaces().Find(x => x.Name == "Bedroom 1");
            space.InternalCondition = null;
            adjacencyCluster.AddObject(space);

            new AnalyticalModel(analyticalModel, adjacencyCluster).ApplyPartFVentilationRates(PartFOperatingMode.ContinuousDesign, out List<string> refusals, out List<string> _);

            Assert.Contains(refusals, x => x.Contains("Bedroom 1") && x.Contains("internal condition"));
        }

        // ---------------------------------------------------------------------------------------------
        // The iteration mapping
        // ---------------------------------------------------------------------------------------------

        /// <summary>BasePassive runs at the Approved Document F sizing condition. A restatement, not a judgement.</summary>
        [Fact]
        public void BasePassive_RunsAtTheContinuousDesignCondition()
        {
            Assert.Equal(PartFOperatingMode.ContinuousDesign, PartOIteration.BasePassive.PartOIterationOperatingMode(out string refusal));
            Assert.Null(refusal);
        }

        /// <summary>
        /// <b>AcousticRestricted is refused rather than mapped to the high rate.</b> Its assumptions say boost is
        /// <i>available</i>, and simulating a whole season at Table 1.2's high rate is a much more favourable claim
        /// than making boost available to a control strategy. That difference is an engineering decision.
        /// </summary>
        [Fact]
        public void AcousticRestricted_IsRefusedRatherThanAssumedToRunAtBoost()
        {
            Assert.Null(PartOIteration.AcousticRestricted.PartOIterationOperatingMode(out string refusal));
            Assert.False(string.IsNullOrWhiteSpace(refusal));
        }

        /// <summary>An unstated stage has no operating condition; it is not defaulted to the sizing case.</summary>
        [Fact]
        public void AnUnstatedIteration_HasNoOperatingCondition()
        {
            Assert.Null(PartOIteration.Undefined.PartOIterationOperatingMode(out string _));
            Assert.Null(PartOIteration.ActiveTrimCooling.PartOIterationOperatingMode(out string _));
        }

        // ---------------------------------------------------------------------------------------------
        // Fixture
        // ---------------------------------------------------------------------------------------------

        /// <summary>
        /// A one-dwelling model sized by the REAL Part F calculator with the shipped rule set - three habitable
        /// rooms of differing volume plus two wet rooms, which is enough for Table 1.3 note 1 not to apply and for
        /// the paragraph 1.67 volume-proportional distribution to give each bedroom a different rate.
        /// </summary>
        private static AnalyticalModel Sized(double? supplyAirFlowPerArea = null, bool shareInternalCondition = false)
        {
            AnalyticalModel analyticalModel = Model(supplyAirFlowPerArea, shareInternalCondition);

            PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

            Assert.NotNull(partFCalculator);

            partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

            Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

            return new AnalyticalModel(analyticalModel, partFCalculator.AdjacencyCluster);
        }

        private static AnalyticalModel Model(double? supplyAirFlowPerArea = null, bool shareInternalCondition = false)
        {
            AdjacencyCluster adjacencyCluster = new();

            //Named so the shared space-use classification recognises them.
            Dictionary<string, double> dictionary = new()
            {
                { "Living Room", 30.0 },
                { "Bedroom 1", 16.0 },
                { "Bedroom 2", 11.0 },
                { "Kitchen", 12.0 },
                { "Bathroom", 6.0 },
            };

            InternalCondition internalCondition_Shared = new("Shared IC");

            if (supplyAirFlowPerArea.HasValue)
            {
                internalCondition_Shared.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, supplyAirFlowPerArea.Value);
            }

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                //Shared on purpose in one test: two bedrooms on one condition is the shape that used to make one
                //room's rate overwrite the other's.
                InternalCondition internalCondition = shareInternalCondition ? internalCondition_Shared : new InternalCondition(keyValuePair.Key + " IC");

                if (supplyAirFlowPerArea.HasValue && !shareInternalCondition)
                {
                    internalCondition.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, supplyAirFlowPerArea.Value);
                }

                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            return new AnalyticalModel("Part F Dwelling", null, null, null, adjacencyCluster);
        }
    }
}
