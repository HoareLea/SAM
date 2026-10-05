// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using Xunit;
using AnalyticalCreate = SAM.Analytical.Create;

namespace SAM.Tests
{
    /// <summary>
    /// <b>Preparing a Part O iteration validates the model against the stage. It neither rewrites the model
    /// to fit the stage, nor labels the result with an assumption the model does not satisfy.</b>
    /// <para>
    /// <c>SAMAnalytical.PreparePartOIteration</c> exists to carry the Approved Document F airflows onto the
    /// internal conditions the simulation reads, and to state the scenarios the assessment is attributed to.
    /// It once did a third thing: it reset every <c>PartOOpeningProperties.OpeningRestriction</c> in the model
    /// to <c>Unrestricted</c> whenever the stage's "Openings Restricted" assumption was false. That was not
    /// the small change it looked like. <c>PartOOpeningProperties.Schedule</c> is <b>derived</b> from
    /// <c>OpeningRestriction</c>, not stored beside it, so the restriction IS the availability schedule's
    /// identity - and resetting it deleted the aperture's <c>PartO_DayOpen_HH_HH</c> schedule from the model
    /// that reached TAS. A modeller who authored <c>restriction_ = NightClosed</c> on
    /// <c>SAMAnalytical.AddOpeningPropertiesByPartO</c>, exactly as that component's own documented example
    /// says to, got a TBD with no Part O availability schedule on the aperture.
    /// </para>
    /// <para>
    /// The disagreement that leaves - a model TAS simulates with night closure while the stage states
    /// <c>Openings Restricted = false</c> - is <b>reported, not acted on</b>. Opening behaviour is
    /// orthogonal to the mitigation stage: a base case may legitimately mix restricted and unrestricted
    /// openings, so the stage-asserted assumption is the thing that is wrong, and moving it from the stage
    /// to the model is a separate change with its own scenario-identity consequences. These tests pin both
    /// halves - <b>the authored opening data survives untouched, and preparation neither rewrites it nor
    /// blocks on it.</b>
    /// </para>
    /// <para>
    /// <b>Why these test library calls and not the Grasshopper component.</b> <c>SAM.Tests</c> references no
    /// Grasshopper assembly, so <c>Prepared</c> below performs the same steps, in the same order and with the
    /// same gating, that <c>SAMAnalytical.PreparePartOIteration.SolveInstance</c> performs. Keeping the
    /// decision in the library rather than in the component is what makes it testable at all.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Shares a collection with <c>PartFAirflowApplicationTests</c> - the suite's only other reader of the
    /// default Part F rule set - so the two never run at the same time. Both reach it through the process-wide
    /// <c>ActiveSetting.Setting</c>, whose lazy load and whose stored <c>PartFData</c> are shared by reference
    /// between every <c>PartFCalculator</c> built from them; running both classes concurrently made a wet
    /// room intermittently size no extract. Same precedent as the two readers of the default aperture
    /// construction library.
    /// </remarks>
    [Collection("SAM.Analytical.ActiveSetting default Part F data")]
    public class PartOIterationPreparationTests
    {
        // -------------------------------------------------------------------------------------------------
        // A. NightClosed under an iteration that assumes unrestricted openings
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The reported symptom, end to end in library terms: an aperture authored <c>NightClosed</c> still
        /// states <c>NightClosed</c> after preparation, and still derives the availability schedule the TAS
        /// aperture-control write looks for - name, hour count and all 24 values.
        /// </summary>
        [Fact]
        public void NightClosedAperture_SurvivesPreparation_WithItsAvailabilitySchedule()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive);

            PartOOpeningProperties partOOpeningProperties = PartO(preparation.AnalyticalModel);

            Assert.Equal(OpeningRestriction.NightClosed, partOOpeningProperties.OpeningRestriction);
            Assert.Equal(8, partOOpeningProperties.NightOpenFromHour);
            Assert.Equal(23, partOOpeningProperties.NightOpenToHour);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.NotNull(schedule);
            Assert.Equal("PartO_DayOpen_08_23", schedule.Name);
            Assert.Equal("000000001111111111111110", schedule.ValuesText);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 8 && hour < 23, schedule[hour]);
            }
        }

        /// <summary>
        /// The other half of the same case: the disagreement between the model and the stage is REPORTED -
        /// naming the aperture, what it states and what the stage assumes - and nothing is blocked by it.
        /// The run stays successful and still states its scenario, because a base case may legitimately
        /// contain a night-closed opening; the stage-asserted assumption is what is wrong, not the model.
        /// </summary>
        [Fact]
        public void NightClosedAperture_UnderBasePassive_IsReportedWithoutBlocking()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive);

            Assert.Equal(PartOOpeningCompatibility.Incompatible, preparation.OpeningCompatibility);

            Assert.True(preparation.Successful);
            Assert.Empty(preparation.Refusals);
            Assert.Single(preparation.OverheatingScenarios);

            string summary = Assert.Single(preparation.Notes.FindAll(x => x.Contains("stage states")));

            Assert.Contains("Window", summary);
            Assert.Contains("NightClosed", summary);
            Assert.Contains("BasePassive", summary);
        }

        /// <summary>
        /// The availability WINDOW is authored data too, not only the fact of a restriction: a non-default
        /// window must come out the far side naming its own schedule, or the reuse-by-value contract on the
        /// TAS side silently collapses every window onto one schedule.
        /// </summary>
        [Fact]
        public void NightClosedAperture_CustomWindow_SurvivesPreparationUnchanged()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed, 7, 22), PartOIteration.BasePassive);

            PartOOpeningProperties partOOpeningProperties = PartO(preparation.AnalyticalModel);

            Assert.Equal(7, partOOpeningProperties.NightOpenFromHour);
            Assert.Equal(22, partOOpeningProperties.NightOpenToHour);
            Assert.Equal("PartO_DayOpen_07_22", partOOpeningProperties.Schedule.Name);
        }

        /// <summary>
        /// <c>AlwaysClosed</c> is the other authored restriction, and it carries no schedule at all - it is
        /// expressed downstream as an opening factor of zero. It survives for the same reason: it is a
        /// statement about the building, not about the assessment stage.
        /// </summary>
        [Fact]
        public void AlwaysClosedAperture_SurvivesPreparation()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.AlwaysClosed), PartOIteration.BasePassive);

            Assert.Equal(OpeningRestriction.AlwaysClosed, PartO(preparation.AnalyticalModel).OpeningRestriction);
            Assert.Equal(PartOOpeningCompatibility.Incompatible, preparation.OpeningCompatibility);
            Assert.True(preparation.Successful);
        }

        /// <summary>
        /// <b>The root cause, pinned.</b> This is the operation preparation used to perform, run explicitly:
        /// resetting the restriction does not merely change a restriction state, it deletes the availability
        /// schedule with it, because the schedule is derived from the restriction and stored nowhere else.
        /// That is the whole reason preparation no longer does this, and it is what a caller of
        /// <c>Modify.ResetPartOOpeningRestrictions</c> - which remains available for callers who do intend
        /// that destructive change - is now told to expect.
        /// </summary>
        [Fact]
        public void TheResetPreparationUsedToPerform_DeletesTheAvailabilityScheduleWithTheRestriction()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            Assert.NotNull(PartO(analyticalModel).Schedule);

            AnalyticalModel analyticalModel_Reset = analyticalModel.ResetPartOOpeningRestrictions(out List<string> notes);

            Assert.NotEmpty(notes);
            Assert.Equal(OpeningRestriction.Unrestricted, PartO(analyticalModel_Reset).OpeningRestriction);
            Assert.Null(PartO(analyticalModel_Reset).Schedule);
        }

        // -------------------------------------------------------------------------------------------------
        // B. The matching case
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// A genuinely unrestricted opening at a stage that assumes unrestricted openings: nothing to refuse,
        /// and the assessment proceeds exactly as it always did.
        /// </summary>
        [Fact]
        public void UnrestrictedAperture_UnderBasePassive_ProceedsAndStatesItsScenario()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted), PartOIteration.BasePassive);

            Assert.Equal(PartOOpeningCompatibility.Compatible, preparation.OpeningCompatibility);
            Assert.Empty(preparation.Refusals);
            Assert.True(preparation.Successful);
            Assert.Single(preparation.OverheatingScenarios);
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), preparation.OverheatingScenarios[0].OperatingAssumptions.Value(Analytical.Query.OpeningsRestricted));
        }

        /// <summary>A model with no operable opening cannot contradict an opening assumption.</summary>
        [Fact]
        public void ModelWithNoOperableOpening_IsCompatible()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted, openingProperties: OpeningPropertiesKind.None);

            Assert.Equal(PartOOpeningCompatibility.Compatible, analyticalModel.PartOIterationOpeningCompatibility(PartOIteration.BasePassive, out string summary, out List<string> _));
            Assert.Null(summary);
        }

        /// <summary>A stage that states nothing about openings cannot disagree with the model about them.</summary>
        [Fact]
        public void UndefinedIteration_IsCompatible()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            Assert.Equal(PartOOpeningCompatibility.Compatible, analyticalModel.PartOIterationOpeningCompatibility(PartOIteration.Undefined, out string summary, out List<string> _));
            Assert.Null(summary);
        }

        // -------------------------------------------------------------------------------------------------
        // C. The reverse mismatch
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// A stage that states restricted openings over a model that restricts nothing is the same
        /// mis-statement as the reverse, and must not be the direction that passes quietly once a mitigated
        /// stage gains a settled operating condition. Reported, and the model is not mutated to add a
        /// restriction either.
        /// </summary>
        [Fact]
        public void UnrestrictedModel_UnderAcousticRestricted_IsReportedWithoutMutatingTheModel()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            PartOOpeningCompatibility partOOpeningCompatibility = analyticalModel.PartOIterationOpeningCompatibility(PartOIteration.AcousticRestricted, out string summary, out List<string> _);

            Assert.Equal(PartOOpeningCompatibility.Incompatible, partOOpeningCompatibility);
            Assert.NotNull(summary);
            Assert.Contains("AcousticRestricted", summary);
            Assert.Contains("Window", summary);

            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        /// <summary>
        /// A model that restricts SOME openings satisfies a stage that assumes restricted openings - which
        /// openings are restricted is the modeller's business, not this validation's.
        /// </summary>
        [Fact]
        public void PartlyRestrictedModel_UnderAcousticRestricted_IsCompatible()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed, second: OpeningRestriction.Unrestricted);

            Assert.Equal(PartOOpeningCompatibility.Compatible, analyticalModel.PartOIterationOpeningCompatibility(PartOIteration.AcousticRestricted, out string summary, out List<string> _));
            Assert.Null(summary);
        }

        // -------------------------------------------------------------------------------------------------
        // D. ProfileOpeningProperties - the other authoring path
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// A first-class <c>DailyAvailabilitySchedule</c> IS deterministically classifiable: it is binary and
        /// exactly 24 hours long. Available every hour is positively unrestricted, so the assessment proceeds.
        /// </summary>
        [Fact]
        public void ProfileOpeningProperties_WithAnAllAvailableSchedule_IsPositivelyUnrestricted()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted, openingProperties: OpeningPropertiesKind.ScheduleAlwaysAvailable), PartOIteration.BasePassive);

            Assert.Equal(PartOOpeningCompatibility.Compatible, preparation.OpeningCompatibility);
            Assert.True(preparation.Successful);
            Assert.Single(preparation.OverheatingScenarios);
        }

        /// <summary>
        /// The same carrier stating a schedule with hours off is positively RESTRICTED, and refuses under a
        /// stage that assumes unrestricted openings - the identical treatment a <c>NightClosed</c>
        /// <c>PartOOpeningProperties</c> gets. The two authoring paths must not disagree about the same
        /// engineering fact; that asymmetry is what gave the original defect away.
        /// </summary>
        [Fact]
        public void ProfileOpeningProperties_WithARestrictingSchedule_IsReportedUnderBasePassive()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted, openingProperties: OpeningPropertiesKind.ScheduleNightShut), PartOIteration.BasePassive);

            Assert.Equal(PartOOpeningCompatibility.Incompatible, preparation.OpeningCompatibility);
            Assert.True(preparation.Successful);
            Assert.Contains(preparation.Notes, x => x.Contains("Night Shut"));

            //And the schedule itself is untouched by the refusal.
            Assert.True(SingleAperture(preparation.AnalyticalModel).TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties));
            Assert.Equal("Night Shut", ((ProfileOpeningProperties)openingProperties).Schedule.Name);
        }

        /// <summary>
        /// <b>Unknown is not unrestricted.</b> The legacy general-valued <c>Profile</c> carrier states
        /// availability in a form with no deterministic reading as restricted or unrestricted. Assuming it
        /// unrestricted would recreate the original defect through the other authoring path, so it is
        /// reported as unknown - a different verdict from a proven disagreement, and said so.
        /// </summary>
        [Fact]
        public void ProfileOpeningProperties_WithOnlyALegacyProfile_IsReportedAsUnknown()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted, openingProperties: OpeningPropertiesKind.LegacyProfile), PartOIteration.BasePassive);

            Assert.Equal(PartOOpeningCompatibility.Unknown, preparation.OpeningCompatibility);
            Assert.True(preparation.Successful);

            string summary = Assert.Single(preparation.Notes.FindAll(x => x.Contains("cannot classify")));

            Assert.Contains("Night Shut", summary);
        }

        /// <summary>
        /// The unknown verdict is about UNCLASSIFIABLE availability, not about the carrier type. A
        /// <c>ProfileOpeningProperties</c> stating neither schedule nor profile supplies the TAS write with no
        /// schedule source at all, so the opening is available every hour - provably, not by assumption.
        /// </summary>
        [Fact]
        public void ProfileOpeningProperties_StatingNoAvailabilityAtAll_IsPositivelyUnrestricted()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted, openingProperties: OpeningPropertiesKind.ProfileCarrierWithNothing);

            Assert.Equal(PartOOpeningCompatibility.Compatible, analyticalModel.PartOIterationOpeningCompatibility(PartOIteration.BasePassive, out string summary, out List<string> _));
            Assert.Null(summary);
        }

        /// <summary>
        /// One pane nobody can classify makes the whole aperture unclassifiable, even beside a pane that is
        /// provably restricted: calling the aperture "restricted" would be a claim resting on a pane nobody
        /// has read.
        /// </summary>
        [Fact]
        public void PartOOpeningRestricted_UnknownDominatesWithinOneAperture()
        {
            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());

            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed));
            aperture.AddSingleOpeningProperties(new ProfileOpeningProperties(0.6, new Profile("Legacy", ProfileType.Other, Values(1))));

            Assert.Null(aperture.PartOOpeningRestricted(out string evidence));
            Assert.NotNull(evidence);
        }

        /// <summary>
        /// The classification table itself, read off what the TAS write will be given rather than off the
        /// authoring vocabulary. A restricted pane beside an unrestricted one restricts the aperture.
        /// </summary>
        [Fact]
        public void PartOOpeningRestricted_ClassifiesEachCarrier()
        {
            Assert.False(Restricted(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.Unrestricted)));
            Assert.True(Restricted(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.NightClosed)));
            Assert.True(Restricted(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.AlwaysClosed)));

            Assert.False(Restricted(new ProfileOpeningProperties(0.6, AllAvailable())));
            Assert.True(Restricted(new ProfileOpeningProperties(0.6, NightShut())));
            Assert.False(Restricted(new ProfileOpeningProperties(0.6)));
            Assert.Null(Restricted(new ProfileOpeningProperties(0.6, new Profile("Legacy", ProfileType.Other, Values(1)))));

            Assert.False(Restricted(new OpeningProperties(0.6)));

            //Schedule wins over the legacy profile beside it - the same precedence the TAS write applies.
            Assert.True(Restricted(new ProfileOpeningProperties(0.6, new Profile("Legacy", ProfileType.Other, Values(1)), NightShut())));

            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());
            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0, OpeningRestriction.Unrestricted));
            aperture.AddSingleOpeningProperties(new PartOOpeningProperties(0.6, 1.0, 30.0, OpeningRestriction.NightClosed));

            Assert.True(aperture.PartOOpeningRestricted(out string _));
        }

        // -------------------------------------------------------------------------------------------------
        // E. Unrelated schedules and profile references, including through a refusal
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Every profile reference an internal condition carries - occupancy, both equipment gains, lighting,
        /// infiltration, pollutant, heating, cooling, humidification, dehumidification and ventilation - must
        /// come out of preparation naming exactly the same profile. Preparation clones each sized space's
        /// internal condition, and a clone that copied a hand-picked subset of the fields would lose the rest
        /// silently. Run over the REFUSING case on purpose: a refusal must not be a licence to leave the
        /// model half-transformed.
        /// </summary>
        [Fact]
        public void Preparation_PreservesEveryProfileNameOnEveryInternalCondition()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            Dictionary<string, Dictionary<ProfileType, string>> before = ProfileNames(analyticalModel);

            Assert.NotEmpty(before);

            Dictionary<string, Dictionary<ProfileType, string>> after = ProfileNames(Prepared(analyticalModel, PartOIteration.BasePassive).AnalyticalModel);

            Assert.Equal(before.Count, after.Count);

            foreach (KeyValuePair<string, Dictionary<ProfileType, string>> keyValuePair in before)
            {
                Assert.True(after.ContainsKey(keyValuePair.Key), string.Format("Space '{0}' lost its internal condition.", keyValuePair.Key));

                Dictionary<ProfileType, string> profileNames = after[keyValuePair.Key];

                Assert.Equal(keyValuePair.Value.Count, profileNames.Count);

                foreach (KeyValuePair<ProfileType, string> keyValuePair_Profile in keyValuePair.Value)
                {
                    Assert.True(profileNames.ContainsKey(keyValuePair_Profile.Key), string.Format("Space '{0}' lost its {1} profile reference.", keyValuePair.Key, keyValuePair_Profile.Key));
                    Assert.Equal(keyValuePair_Profile.Value, profileNames[keyValuePair_Profile.Key]);
                }
            }
        }

        /// <summary>
        /// <b>Review finding: a legacy Ventilation profile that actually resolves would activate
        /// TBD's own mechanical ventilation (ticV) alongside this iteration's directional inter-zone air
        /// movements - the same space's design airflow reaching the simulation twice.</b>
        /// <para>
        /// <c>Model()</c> gives every space <c>InternalCondition.VentilationProfileName = "Ventilation
        /// Continuous"</c>, but the fixture's own <c>ProfileLibrary</c> never carries a profile by that
        /// name - which is exactly why <see cref="Preparation_PreservesEveryProfileNameOnEveryInternalCondition"/>
        /// can assert this iteration is prepared successfully over it: the name never RESOLVES, so SAM_Tas's
        /// <c>Modify.UpdateInternalCondition</c> - gated on <c>InternalCondition.GetProfile(ProfileType.Ventilation,
        /// profileLibrary)</c> returning non-null, exactly the test this iteration now runs itself - would
        /// never have activated <c>ticV</c> from it in the first place. Adding the profile here is the one
        /// change that turns "a name is on the internal condition" into a genuine double-count risk, and
        /// preparation must refuse rather than silently carry both runtime representations forward.
        /// </para>
        /// <para>
        /// The smallest safe correction: nothing is cleared or rewritten on the way to the refusal, so this
        /// does not conflict with <see cref="Preparation_PreservesEveryProfileNameOnEveryInternalCondition"/>
        /// - that test's profile never resolves, so it never reaches this gate.
        /// </para>
        /// </summary>
        [Fact]
        public void ALegacyVentilationProfileThatResolves_RefusesRatherThanRiskingADoubleCount()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            //The one change: the name every space's internal condition already carries now resolves in the
            //model's own library, exactly the test SAM_Tas's ticV gate itself runs. AnalyticalModel.ProfileLibrary
            //returns a COPY on every read - the same trap AdjacencyCluster has - so the augmented library has
            //to be put back onto a new model rather than mutated in place.
            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;
            profileLibrary.Add(new Profile("Ventilation Continuous", ProfileType.Ventilation, Values(1)));
            analyticalModel = new AnalyticalModel(analyticalModel, analyticalModel.AdjacencyCluster, null, profileLibrary);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, Strategies(analyticalModel, "MVRE"));

            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);

            //Named, so the refusal is actionable rather than a generic failure.
            Assert.Contains("Ventilation profile", preparation.Refusal);
            Assert.Contains("ticV", preparation.Refusal);

            //Nothing was rewritten on the way to the refusal - VentilationProfileName is read, never cleared.
            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        /// <summary>
        /// The system-type references beside the profile references, which are the other named lookup an
        /// internal condition carries into TAS.
        /// </summary>
        [Fact]
        public void Preparation_PreservesSystemTypeNames()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive);

            foreach (Space space in preparation.AnalyticalModel.GetSpaces())
            {
                InternalCondition internalCondition = space.InternalCondition;

                Assert.Equal("Ventilation System", internalCondition.GetSystemTypeName<VentilationSystemType>());
                Assert.Equal("Heating System", internalCondition.GetSystemTypeName<HeatingSystemType>());
                Assert.Equal("Cooling System", internalCondition.GetSystemTypeName<CoolingSystemType>());
            }
        }

        /// <summary>
        /// The profiles the names point at are model data too - a preserved name pointing into an emptied
        /// library resolves to nothing.
        /// </summary>
        [Fact]
        public void Preparation_PreservesTheProfileLibrary()
        {
            ProfileLibrary profileLibrary = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive).AnalyticalModel.ProfileLibrary;

            Assert.NotNull(profileLibrary);

            Assert.NotNull(profileLibrary.GetProfile("Occupancy 24h", ProfileType.Occupancy));
            Assert.NotNull(profileLibrary.GetProfile("Heating Setpoint", ProfileType.Heating));
            Assert.NotNull(profileLibrary.GetProfile("Cooling Setpoint", ProfileType.Cooling));
            Assert.NotNull(profileLibrary.GetProfile("Infiltration Constant", ProfileType.Infiltration));
        }

        /// <summary>
        /// The parameters carried beside the restriction on the same opening properties - the TAS aperture
        /// function and the opening factor - are the fields a partial rebuild of the object would drop.
        /// </summary>
        [Fact]
        public void Preparation_PreservesTheOpeningFunctionAndFactor()
        {
            PartOOpeningProperties partOOpeningProperties = PartO(Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive).AnalyticalModel);

            Assert.True(partOOpeningProperties.TryGetValue(OpeningPropertiesParameter.Function, out string function));
            Assert.Equal("zdwno,0,19.00,21.00,99.00", function);
            Assert.Equal(0.75, partOOpeningProperties.Factor);
        }

        /// <summary>The model handed in is never the model handed back, and is never touched.</summary>
        [Fact]
        public void Preparation_LeavesTheSuppliedModelUnchanged()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            Prepared(analyticalModel, PartOIteration.BasePassive);

            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        // -------------------------------------------------------------------------------------------------
        // F. The change the iteration IS supposed to make still happens
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The guard against "fixing" schedule loss by disabling the transformation. Preparation must still
        /// write the Part F continuous design rate onto the internal condition the simulation reads, and must
        /// still clear the bases <c>CalculatedSupplyAirFlow</c> would otherwise SUM with it.
        /// </summary>
        [Fact]
        public void Preparation_StillAppliesThePartFRate_AndStillClearsTheSummingBases()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted, supplyAirFlowPerArea: 0.005);

            Space space_Before = analyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");

            Assert.True(space_Before.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlowPerArea, out double perArea_Before) && perArea_Before > 0, "The fixture did not seed a competing per-area rate, so this test would prove nothing.");

            PartOIterationPreparation preparation = Prepared(analyticalModel, PartOIteration.BasePassive);

            Space space = preparation.AnalyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.True(partFSpaceData.ContinuousSupplyFlowRate_Lps > 0, "The fixture did not size a supply rate, so this test would prove nothing.");

            //Read back through the query the simulation uses, so this is the number the export sees.
            Assert.Equal(partFSpaceData.ContinuousSupplyFlowRate_Lps.Value / 1000.0, space.CalculatedSupplyAirFlow(), 9);

            Assert.True(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlowPerArea, out double perArea));
            Assert.Equal(0, perArea);
        }

        /// <summary>
        /// A reported disagreement over opening behaviour does not disturb the Part F application: the rates
        /// are still applied, still readable through the query the simulation uses, and the run is still
        /// successful.
        /// </summary>
        [Fact]
        public void AReportedOpeningDisagreement_StillAppliesThePartFRates()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive);

            Assert.True(preparation.Successful);

            Space space = preparation.AnalyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.Equal(partFSpaceData.ContinuousSupplyFlowRate_Lps.Value / 1000.0, space.CalculatedSupplyAirFlow(), 9);
        }

        // -------------------------------------------------------------------------------------------------
        // G. Repeated preparation
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Preparing an already-prepared model must not degrade it. Full model equality is deliberately NOT
        /// asserted: each pass gives every sized space its own freshly named, freshly guided internal
        /// condition, which is the documented behaviour that stops one room's Part F rate landing on another's.
        /// What must not drift is everything this task is about - the opening restrictions, the schedules
        /// derived from them, the profile references each condition carries, and the verdict itself.
        /// </summary>
        [Fact]
        public void PreparingTwice_LosesNoRestrictionScheduleOrProfileReference()
        {
            PartOIterationPreparation preparation_Once = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BasePassive);
            PartOIterationPreparation preparation_Twice = Prepared(preparation_Once.AnalyticalModel, PartOIteration.BasePassive);

            Assert.Equal(OpeningRestriction.NightClosed, PartO(preparation_Twice.AnalyticalModel).OpeningRestriction);
            Assert.Equal("PartO_DayOpen_08_23", PartO(preparation_Twice.AnalyticalModel).Schedule.Name);
            Assert.Equal(preparation_Once.OpeningCompatibility, preparation_Twice.OpeningCompatibility);

            Dictionary<string, Dictionary<ProfileType, string>> once = ProfileNames(preparation_Once.AnalyticalModel);
            Dictionary<string, Dictionary<ProfileType, string>> twice = ProfileNames(preparation_Twice.AnalyticalModel);

            Assert.Equal(once.Count, twice.Count);

            foreach (KeyValuePair<string, Dictionary<ProfileType, string>> keyValuePair in once)
            {
                Assert.Equal(keyValuePair.Value, twice[keyValuePair.Key]);
            }
        }

        /// <summary>The compatible path is idempotent too, scenarios included.</summary>
        [Fact]
        public void PreparingTwice_OnACompatibleModel_StatesTheSameScenario()
        {
            PartOIterationPreparation preparation_Once = Prepared(Model(OpeningRestriction.Unrestricted), PartOIteration.BasePassive);
            PartOIterationPreparation preparation_Twice = Prepared(preparation_Once.AnalyticalModel, PartOIteration.BasePassive);

            Assert.True(preparation_Twice.Successful);
            Assert.Equal(preparation_Once.OverheatingScenarios[0].Key, preparation_Twice.OverheatingScenarios[0].Key);
        }

        // -------------------------------------------------------------------------------------------------
        // H. The natural-ventilation route - Iteration 1b
        //
        // PartFCalculator is unconditionally System 4 shaped: paragraph 1.67 gives every habitable room a
        // mechanical supply terminal and every wet room a continuous extract terminal, with no input anywhere
        // for how the dwelling is actually ventilated. Carrying those rates onto a naturally ventilated
        // dwelling simulates an MVHR system nobody described - and does it SUCCESSFULLY, which is worse than
        // refusing, because nothing in the result says the system was invented.
        //
        // These tests use the SAME fixture as the mechanical tests above - the real Part F calculator over the
        // shipped rule set - so the sizing being skipped is provably present and provably not applied.
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The blocker, closed.</b> One model, prepared twice: once stating MVRE and once stating NV. The
        /// mechanical statement applies the Part F continuous supply, the natural-ventilation statement
        /// applies nothing - so what the simulation reads back through
        /// <c>Query.CalculatedSupplyAirFlow</c> is zero for every space, while the Part F sizing that would
        /// have been applied is still sitting on the model, unread.
        /// </summary>
        [Fact]
        public void NVDwelling_InventsNoContinuousMechanicalSupplyOrExtract()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            //The control. Same model, stated mechanical: the rates ARE applied, so this fixture is one that
            //could have had an MVHR system invented for it.
            PartOIterationPreparation preparation_Mechanical = Prepared(analyticalModel, PartOIteration.BasePassive, "MVRE");

            Assert.Equal(PartOPartFAirflowApplication.Apply, preparation_Mechanical.AirflowApplication);
            Assert.Contains(preparation_Mechanical.AnalyticalModel.GetSpaces(), x => x.CalculatedSupplyAirFlow() > 0);
            Assert.Contains(preparation_Mechanical.AnalyticalModel.GetSpaces(), x => x.CalculatedExhaustAirFlow() > 0);

            PartOIterationPreparation preparation = Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            Assert.Equal(PartOPartFAirflowApplication.SkipNaturalVentilation, preparation.AirflowApplication);
            Assert.True(preparation.Successful);
            Assert.Empty(preparation.Refusals);

            foreach (Space space in preparation.AnalyticalModel.GetSpaces())
            {
                AssertNoContinuousMechanicalAirflow(space);
            }

            //And the Part F sizing that was NOT applied is still there, so this is a decision not to use it
            //rather than an absence of anything to use.
            PartFSpaceData partFSpaceData = preparation.AnalyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1").GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.NotNull(partFSpaceData);
            Assert.True(partFSpaceData.ContinuousSupplyFlowRate_Lps > 0, "The fixture did not size a supply rate, so this test would prove nothing.");
        }

        /// <summary>
        /// The internal conditions themselves are untouched on this path - no per-space clone, no renaming,
        /// no cleared summing bases. Nothing was applied, so nothing needed rewriting.
        /// </summary>
        [Fact]
        public void NVDwelling_LeavesTheInternalConditionsExactlyAsAuthored()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed, supplyAirFlowPerArea: 0.005);

            PartOIterationPreparation preparation = Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            foreach (Space space in preparation.AnalyticalModel.GetSpaces())
            {
                Assert.Equal(space.Name + " IC", space.InternalCondition.Name);

                //The competing per-area rate is authored building data on this path, not something that would
                //double-count against a Part F rate, because there is no Part F rate beside it.
                Assert.True(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlowPerArea, out double perArea));
                Assert.Equal(0.005, perArea);
            }

            AssertSameProfileNames(analyticalModel, preparation.AnalyticalModel);
        }

        /// <summary>
        /// The whole point of the workflow: the authored opening data reaches the far side of the
        /// natural-ventilation path intact, schedule and all. This is the aperture control the TAS write
        /// looks for.
        /// </summary>
        [Fact]
        public void NVDwelling_NightClosedAperture_SurvivesWithItsAvailabilitySchedule()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");

            PartOOpeningProperties partOOpeningProperties = PartO(preparation.AnalyticalModel);

            Assert.Equal(OpeningRestriction.NightClosed, partOOpeningProperties.OpeningRestriction);
            Assert.Equal(8, partOOpeningProperties.NightOpenFromHour);
            Assert.Equal(23, partOOpeningProperties.NightOpenToHour);

            DailyAvailabilitySchedule schedule = partOOpeningProperties.Schedule;

            Assert.NotNull(schedule);
            Assert.Equal("PartO_DayOpen_08_23", schedule.Name);
            Assert.Equal("000000001111111111111110", schedule.ValuesText);

            for (int hour = 0; hour < 24; hour++)
            {
                Assert.Equal(hour >= 8 && hour < 23, schedule[hour]);
            }

            //And the parameters carried beside it, which the TAS aperture control also reads.
            Assert.True(partOOpeningProperties.TryGetValue(OpeningPropertiesParameter.Function, out string function));
            Assert.Equal("zdwno,0,19.00,21.00,99.00", function);
            Assert.Equal(0.75, partOOpeningProperties.Factor);
        }

        /// <summary>The scenario is still stated, and it states the strategy that was asked for.</summary>
        [Fact]
        public void NVDwelling_StatesAnNVScenario()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");

            OverheatingScenario overheatingScenario = Assert.Single(preparation.OverheatingScenarios);

            Assert.Equal("NV", overheatingScenario.VentilationStrategy);
            Assert.Equal(PartOAssessmentScope.Dwelling, overheatingScenario.Scope);
            Assert.Equal(PartOIteration.BaseNaturalVentilation, overheatingScenario.Iteration);
        }

        /// <summary>
        /// <b>Absence of Part F data is not an error on this path.</b> A naturally ventilated dwelling that
        /// was never run through a Part F component has nothing to apply and needs nothing to apply - where
        /// the mechanical path rightly refuses with "run a Part F component first", this one prepares.
        /// </summary>
        [Fact]
        public void NVDwelling_WithNoPartFDataAtAll_StillPrepares()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed, partF: false);

            Assert.All(analyticalModel.GetSpaces(), x => Assert.Null(x.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)));

            //The control: stated mechanical, the same model refuses outright.
            PartOIterationPreparation preparation_Mechanical = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, Strategies(analyticalModel, "MVRE"));

            Assert.NotNull(preparation_Mechanical.Refusal);
            Assert.Null(preparation_Mechanical.AnalyticalModel);

            PartOIterationPreparation preparation = Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            Assert.True(preparation.Successful);
            Assert.Equal("NV", Assert.Single(preparation.OverheatingScenarios).VentilationStrategy);
            Assert.Equal(OpeningRestriction.NightClosed, PartO(preparation.AnalyticalModel).OpeningRestriction);
            Assert.Equal("PartO_DayOpen_08_23", PartO(preparation.AnalyticalModel).Schedule.Name);
        }

        /// <summary>
        /// The note says what was not done and why, and is explicit that this is NOT a natural-ventilation
        /// Part F sizing - because "SAM applied no mechanical airflow" and "SAM sized the dwelling's System 1
        /// provision" are very different claims and only the first one is true.
        /// </summary>
        [Fact]
        public void NVDwelling_SaysWhyNoMechanicalAirflowWasApplied_WithoutClaimingItSizedAnything()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");

            string note = Assert.Single(preparation.Notes.FindAll(x => x.Contains("Natural Ventilation")));

            Assert.Contains("NOT applied", note);
            Assert.Contains("Natural Ventilation route", note);
            Assert.Contains("does not have", note);

            //The claim it must never make.
            Assert.Contains("does NOT mean", note);
            Assert.Contains("background ventilator", note);

            //Raised to the user, not merely recorded.
            Assert.Contains(note, preparation.Warnings);
        }

        /// <summary>The supplied model is not touched on this path either.</summary>
        [Fact]
        public void NVPreparation_LeavesTheSuppliedModelUnchanged()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        /// <summary>
        /// Preparing an already-prepared NV model changes nothing - the restriction, the schedule, the
        /// scenario identity and the absence of mechanical airflow all hold on the second pass.
        /// </summary>
        [Fact]
        public void NVPreparation_IsIdempotent()
        {
            PartOIterationPreparation preparation_Once = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");
            PartOIterationPreparation preparation_Twice = Prepared(preparation_Once.AnalyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            Assert.True(preparation_Twice.Successful);
            Assert.Equal(PartOPartFAirflowApplication.SkipNaturalVentilation, preparation_Twice.AirflowApplication);

            Assert.Equal(OpeningRestriction.NightClosed, PartO(preparation_Twice.AnalyticalModel).OpeningRestriction);
            Assert.Equal("PartO_DayOpen_08_23", PartO(preparation_Twice.AnalyticalModel).Schedule.Name);
            Assert.Equal(preparation_Once.OpeningCompatibility, preparation_Twice.OpeningCompatibility);
            Assert.Equal(preparation_Once.OverheatingScenarios[0].Key, preparation_Twice.OverheatingScenarios[0].Key);

            AssertSameProfileNames(preparation_Once.AnalyticalModel, preparation_Twice.AnalyticalModel);

            foreach (Space space in preparation_Twice.AnalyticalModel.GetSpaces())
            {
                AssertNoContinuousMechanicalAirflow(space);
            }
        }

        // -------------------------------------------------------------------------------------------------
        // I. An unsettled ventilation route refuses
        //
        // The rule this replaced was "anything that is not NV is mechanical". Under it, UV, an empty panel,
        // a typo and a stale word all wrote Approved Document F System 4 supply and extract onto every sized
        // space in the model - successfully, and with nothing downstream saying an MVHR system had been
        // invented. There are exactly two Part O routes and everything else refuses.
        //
        // Mixed is one case of the same thing rather than a category of its own: ApplyPartFVentilationRates
        // is whole-model, so a model whose zones disagree has no answer available that is right for both
        // halves. All of them are RefuseUnstatedRoute, and WHICH absence it was is in the refusal text.
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The mapping, stated once and totally. <c>NV</c> and <c>MVRE</c> are the words this codebase's
        /// models and licensed acceptance runs use; the longer spellings are the same routes said the way
        /// the architecture says them.
        /// </summary>
        [Theory]
        [InlineData("NV", PartOVentilationMode.NaturalVentilation)]
        [InlineData("nv", PartOVentilationMode.NaturalVentilation)]
        [InlineData(" NV ", PartOVentilationMode.NaturalVentilation)]
        [InlineData("NaturalVentilation", PartOVentilationMode.NaturalVentilation)]
        [InlineData("Natural Ventilation", PartOVentilationMode.NaturalVentilation)]
        [InlineData("MVHR", PartOVentilationMode.MVHR)]
        [InlineData("mvhr", PartOVentilationMode.MVHR)]
        [InlineData("MVRE", PartOVentilationMode.MVHR)]
        public void AStatedRoute_ResolvesToItsMode(string ventilationStrategy, PartOVentilationMode expected)
        {
            Assert.Equal(expected, Analytical.Query.PartOVentilationMode(ventilationStrategy, out string refusal));
            Assert.Null(refusal);
        }

        /// <summary>
        /// <b>Nothing else is a route, and nothing else is quietly read as mechanical.</b> This is the whole
        /// point of the type: each of these words used to reach the airflow application and be applied.
        /// </summary>
        [Theory]
        [InlineData("MV")]
        [InlineData("UV")]
        [InlineData("EOL")]
        [InlineData("CAV")]
        [InlineData("MVHRR")]
        [InlineData("Mechanical")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void AnythingElse_IsNoRouteAtAll(string ventilationStrategy)
        {
            Assert.Equal(PartOVentilationMode.Undefined, Analytical.Query.PartOVentilationMode(ventilationStrategy, out string refusal));
            Assert.NotNull(refusal);
        }

        /// <summary>
        /// The two refusals that have to say more than "unrecognised", because both words mean something
        /// real elsewhere in SAM and somebody typing them has a reasonable belief that they work.
        /// </summary>
        [Fact]
        public void MVAndUV_AreRefusedWithTheReasonTheyAreNotRoutes()
        {
            Analytical.Query.PartOVentilationMode("MV", out string refusal_MV);

            Assert.Contains("System 3", refusal_MV);
            Assert.Contains("System 4", refusal_MV);
            Assert.Contains("MVHR", refusal_MV);

            Analytical.Query.PartOVentilationMode("UV", out string refusal_UV);

            Assert.Contains("corridor", refusal_UV);
        }

        /// <summary>
        /// <b>The whole-model consequence, on the production path.</b> Each of these once prepared
        /// successfully with continuous mechanical supply and extract written onto every sized space.
        /// </summary>
        [Theory]
        [InlineData("MV")]
        [InlineData("UV")]
        [InlineData("Mechanical Ventilation")]
        [InlineData("")]
        public void AnUnstatedRoute_RefusesAndAppliesNothing(string ventilationStrategy)
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, Strategies(analyticalModel, ventilationStrategy));

            Assert.Equal(PartOVentilationMode.Undefined, preparation.VentilationMode);
            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, preparation.AirflowApplication);

            //No half-prepared model comes back: a refusal that still handed one out is a model somebody
            //simulates.
            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Empty(preparation.OverheatingScenarios);
            Assert.False(preparation.Successful);

            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        /// <summary>
        /// A zone that states nothing at all is named, so the refusal points at the zone to fix rather than
        /// at the model in general.
        /// </summary>
        [Fact]
        public void AZoneStatingNothing_IsNamedInTheRefusal()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, new Dictionary<Guid, string>());

            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, preparation.AirflowApplication);
            Assert.Contains("Flat 1", preparation.Refusal);
            Assert.Contains("No Part O ventilation route was stated", preparation.Refusal);
        }

        /// <summary>
        /// <b>No assessed zone is an unstated route, not a default.</b> This is a deliberate behaviour
        /// change: the old gate kept applying here, on the reasoning that a model with no zones should
        /// prepare exactly as it did before the gate existed. That reasoning is what let an unstated route
        /// write System 4 airflow onto every sized space in a zone-less model.
        /// </summary>
        [Fact]
        public void NoAssessedZones_Refuse()
        {
            Assert.Equal(PartOVentilationMode.Undefined, Analytical.Query.PartOVentilationMode(null, null, out string refusal));
            Assert.NotNull(refusal);

            Assert.Equal(PartOVentilationMode.Undefined, Analytical.Query.PartOVentilationMode(new List<Zone>(), new Dictionary<Guid, string>(), out refusal));
            Assert.NotNull(refusal);

            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, Analytical.Query.PartOPartFAirflowApplication(PartOVentilationMode.Undefined, out string diagnostic));
            Assert.NotNull(diagnostic);
        }

        /// <summary>
        /// The mixed refusal, and the diagnostic that makes it actionable: every affected zone is named
        /// beside the route it states.
        /// </summary>
        [Fact]
        public void MixedRoutes_RefuseAndNameEveryZoneWithItsRoute()
        {
            AnalyticalModel analyticalModel = ModelWithTwoZones(out Zone zone_NaturalVentilation, out Zone zone_Mechanical);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                null,
                new Dictionary<Guid, string>
                {
                    { zone_NaturalVentilation.Guid, "NV" },
                    { zone_Mechanical.Guid, "MVRE" },
                });

            Assert.False(preparation.Successful);
            Assert.NotNull(preparation.Refusal);

            Assert.Contains("Flat 1", preparation.Refusal);
            Assert.Contains("NaturalVentilation", preparation.Refusal);
            Assert.Contains("Flat 2", preparation.Refusal);
            Assert.Contains("MVHR", preparation.Refusal);

            //Named as the thing that cannot be resolved rather than as one of the two routes.
            Assert.Equal(PartOVentilationMode.Undefined, preparation.VentilationMode);
            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, preparation.AirflowApplication);
        }

        /// <summary>
        /// <b>No half-prepared model comes back</b>, and the supplied model is exactly as it was found.
        /// </summary>
        [Fact]
        public void MixedRoutes_ReturnNoModelAndMutateNothing()
        {
            AnalyticalModel analyticalModel = ModelWithTwoZones(out Zone zone_NaturalVentilation, out Zone zone_Mechanical);

            string json_Before = SAM.Core.Convert.ToString(analyticalModel);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                null,
                new Dictionary<Guid, string>
                {
                    { zone_NaturalVentilation.Guid, "NV" },
                    { zone_Mechanical.Guid, "MVRE" },
                });

            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, preparation.AirflowApplication);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Empty(preparation.OverheatingScenarios);

            Assert.Equal(json_Before, SAM.Core.Convert.ToString(analyticalModel));
        }

        /// <summary>
        /// A zone stating NV beside a zone stating nothing at all is a refusal too, and it is diagnosed as
        /// the silent zone rather than as a disagreement - silence is not a statement that the zone is
        /// naturally ventilated, so there is nothing for the NV zone to disagree WITH.
        /// </summary>
        [Fact]
        public void NVBesideAZoneStatingNothing_Refuses()
        {
            AnalyticalModel analyticalModel = ModelWithTwoZones(out Zone zone_NaturalVentilation, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BaseNaturalVentilation,
                null,
                new Dictionary<Guid, string> { { zone_NaturalVentilation.Guid, "NV" } });

            Assert.Equal(PartOPartFAirflowApplication.RefuseUnstatedRoute, preparation.AirflowApplication);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Contains("Flat 2", preparation.Refusal);
            Assert.Contains("No Part O ventilation route was stated", preparation.Refusal);
        }

        /// <summary>
        /// <b>An assessed dwelling must not be thermally coupled to an unassessed one through a shared
        /// generic unit.</b>
        /// <para>
        /// <c>Query.PartOVentilationMode(zones, ...)</c> settles the route from the ASSESSED zones alone,
        /// so a caller naming a subset - a Grasshopper <c>zones_</c> input is explicitly built to take one
        /// ("leave unconnected to use every zone") - can genuinely leave a second, unassessed dwelling in
        /// the same model unseen by the mixed-route check. Terminal realization stays whole-model because
        /// <c>Modify.ApplyPartFVentilationRates</c> already wrote every sized space, but the system and its
        /// air handling unit must serve only the assessed dwelling, or the unassessed one would be pulled
        /// onto the same unit and thermally coupled to it while preparation still reported success.
        /// </para>
        /// </summary>
        [Fact]
        public void AssessingOneDwelling_LeavesAnUnassessedDwellingsSpaceOffTheSharedUnit()
        {
            AnalyticalModel analyticalModel = ModelWithTwoSizedDwellings(out Zone zone_Assessed, out Space space_Unassessed);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_Assessed },
                new Dictionary<Guid, string> { { zone_Assessed.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);
            Assert.NotNull(preparation.AnalyticalModel);
            Assert.NotNull(preparation.VentilationSystem);
            Assert.NotNull(preparation.AirHandlingUnit);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            List<Space> spaces_Served = adjacencyCluster.GetRelatedObjects<Space>(preparation.VentilationSystem) ?? new List<Space>();

            Assert.DoesNotContain(spaces_Served, x => x.Guid == space_Unassessed.Guid);
        }

        // -------------------------------------------------------------------------------------------------
        // I.5. One Base MVHR system per assessed dwelling
        //
        // PartFCalculator sizes each dwelling zone independently, so combining two assessed dwellings onto
        // one shared generic AHU would route separate flats through plant neither dwelling's Part F sizing
        // describes. These pin the four things a second, independently assessed dwelling must never do to
        // the first: share its system, its unit, its duty or its transfer air.
        // -------------------------------------------------------------------------------------------------

        [Fact]
        public void TwoAssessedDwellings_EachGetsItsOwnVentilationSystemAndAirHandlingUnit()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1, zone_2 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" }, { zone_2.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);
            Assert.NotNull(preparation.AnalyticalModel);

            Assert.Equal(2, preparation.VentilationSystems.Count);
            Assert.Equal(2, preparation.AirHandlingUnits.Count);

            Assert.NotEqual(preparation.VentilationSystems[0].Guid, preparation.VentilationSystems[1].Guid);
            Assert.NotEqual(preparation.AirHandlingUnits[0].Guid, preparation.AirHandlingUnits[1].Guid);

            //The singular convenience properties agree with the first entry of the plural ones - they never
            //disagree, so a caller built before a model could carry more than one dwelling still sees a
            //consistent answer.
            Assert.Equal(preparation.VentilationSystems[0].Guid, preparation.VentilationSystem.Guid);
            Assert.Equal(preparation.AirHandlingUnits[0].Guid, preparation.AirHandlingUnit.Guid);
        }

        [Fact]
        public void TwoAssessedDwellings_DutiesDoNotCrossTheDwellingBoundary()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1, zone_2 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" }, { zone_2.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            VentilationSystem system_Flat2 = SystemServing(adjacencyCluster, preparation.VentilationSystems, "Flat 2 Bedroom");
            VentilationSystem system_Flat1 = preparation.VentilationSystems.Find(x => x.Guid != system_Flat2.Guid);

            Assert.NotNull(system_Flat1);

            adjacencyCluster.VentilationSystemDesignDuty(system_Flat2, out double supply_Flat2, out double extract_Flat2);
            adjacencyCluster.VentilationSystemDesignDuty(system_Flat1, out double supply_Flat1, out double extract_Flat1);

            //Flat 2's own system carries only its own 8 l/s supply and 8 l/s extract - never Flat 1's duty,
            //and never the sum of both, which is what one shared system would have reported.
            Assert.Equal(8.0, supply_Flat2, 6);
            Assert.Equal(8.0, extract_Flat2, 6);

            Assert.NotEqual(supply_Flat1, supply_Flat2);

            //The total reported on the preparation is the SUM across both dwellings' own systems.
            Assert.Equal(supply_Flat1 + supply_Flat2, preparation.DesignSupplyDuty_Lps, 6);
            Assert.Equal(extract_Flat1 + extract_Flat2, preparation.DesignExtractDuty_Lps, 6);
        }

        [Fact]
        public void TwoAssessedDwellings_TerminalRelationsStayWithTheirOwnSystem()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1, zone_2 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" }, { zone_2.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            HashSet<Guid> guids_Space_Flat1 = SpaceGuids(adjacencyCluster.GetRelatedObjects<Space>(zone_1));
            HashSet<Guid> guids_Space_Flat2 = SpaceGuids(adjacencyCluster.GetRelatedObjects<Space>(zone_2));

            Assert.NotEmpty(guids_Space_Flat1);
            Assert.NotEmpty(guids_Space_Flat2);

            int count_TerminalsChecked = 0;

            foreach (VentilationSystem ventilationSystem in preparation.VentilationSystems)
            {
                List<Space> spaces_System = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? new List<Space>();

                bool servesFlat1 = spaces_System.Exists(s => guids_Space_Flat1.Contains(s.Guid));
                bool servesFlat2 = spaces_System.Exists(s => guids_Space_Flat2.Contains(s.Guid));

                //A system serves exactly one dwelling's spaces, never both and never neither.
                Assert.True(servesFlat1 ^ servesFlat2, "One system serves spaces of both dwellings, or of neither.");

                List<VentilationTerminal> terminals_System = adjacencyCluster.GetRelatedObjects<VentilationTerminal>(ventilationSystem) ?? new List<VentilationTerminal>();

                Assert.NotEmpty(terminals_System);

                foreach (VentilationTerminal ventilationTerminal in terminals_System)
                {
                    List<Space> spaces_Terminal = adjacencyCluster.GetRelatedObjects<Space>(ventilationTerminal) ?? new List<Space>();

                    HashSet<Guid> guids_Own = servesFlat1 ? guids_Space_Flat1 : guids_Space_Flat2;

                    Assert.True(spaces_Terminal.TrueForAll(s => guids_Own.Contains(s.Guid)),
                        string.Format("A terminal owned by {0}'s system belongs to the other dwelling's space.", servesFlat1 ? "Flat 1" : "Flat 2"));

                    count_TerminalsChecked++;
                }
            }

            Assert.True(count_TerminalsChecked > 0, "No terminal was found on either system, so nothing was actually checked.");
        }

        [Fact]
        public void TwoAssessedDwellings_TransferMovementsDoNotCrossTheDwellingBoundary()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1, zone_2 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" }, { zone_2.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);

            AdjacencyCluster adjacencyCluster = preparation.AnalyticalModel.AdjacencyCluster;

            HashSet<string> references_Flat1 = SpaceReferences(adjacencyCluster.GetRelatedObjects<Space>(zone_1));
            HashSet<string> references_Flat2 = SpaceReferences(adjacencyCluster.GetRelatedObjects<Space>(zone_2));

            Assert.NotEmpty(references_Flat1);
            Assert.NotEmpty(references_Flat2);

            int count_Checked = 0;

            foreach (SpaceAirMovement spaceAirMovement in adjacencyCluster.GetObjects<SpaceAirMovement>() ?? new List<SpaceAirMovement>())
            {
                bool fromFlat1 = spaceAirMovement.From != null && references_Flat1.Contains(spaceAirMovement.From);
                bool fromFlat2 = spaceAirMovement.From != null && references_Flat2.Contains(spaceAirMovement.From);
                bool toFlat1 = spaceAirMovement.To != null && references_Flat1.Contains(spaceAirMovement.To);
                bool toFlat2 = spaceAirMovement.To != null && references_Flat2.Contains(spaceAirMovement.To);

                Assert.False(fromFlat1 && toFlat2, "A transfer movement crosses from Flat 1 into Flat 2.");
                Assert.False(fromFlat2 && toFlat1, "A transfer movement crosses from Flat 2 into Flat 1.");

                if (fromFlat1 || fromFlat2 || toFlat1 || toFlat2)
                {
                    count_Checked++;
                }
            }

            Assert.True(count_Checked > 0, "No air movement touching either dwelling's spaces was found, so nothing was actually checked.");
        }

        /// <summary>The ventilation system, among <paramref name="ventilationSystems"/>, related to the named space.</summary>
        private static VentilationSystem SystemServing(AdjacencyCluster adjacencyCluster, List<VentilationSystem> ventilationSystems, string name_Space)
        {
            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem) ?? new List<Space>();

                if (spaces.Exists(x => x.Name == name_Space))
                {
                    return ventilationSystem;
                }
            }

            return null;
        }

        private static HashSet<Guid> SpaceGuids(List<Space> spaces)
        {
            HashSet<Guid> result = new HashSet<Guid>();

            foreach (Space space in spaces ?? new List<Space>())
            {
                if (space != null)
                {
                    result.Add(space.Guid);
                }
            }

            return result;
        }

        private static HashSet<string> SpaceReferences(List<Space> spaces)
        {
            HashSet<string> result = new HashSet<string>();

            foreach (Space space in spaces ?? new List<Space>())
            {
                if (space != null)
                {
                    result.Add(new SAM.Core.ObjectReference(space).ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// The MVHR route still applies, exactly as it always has. Both spellings reach it, so the licensed
        /// acceptance model's <c>MVRE</c> and the architecture's <c>MVHR</c> are provably one route.
        /// </summary>
        [Theory]
        [InlineData("MVRE")]
        [InlineData("MVHR")]
        public void TheMVHRRoute_KeepsTheExistingApplication(string ventilationStrategy)
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted), PartOIteration.BasePassive, ventilationStrategy);

            Assert.Equal(PartOVentilationMode.MVHR, preparation.VentilationMode);
            Assert.Equal(PartOPartFAirflowApplication.Apply, preparation.AirflowApplication);

            Space space = preparation.AnalyticalModel.GetSpaces().Find(x => x.Name == "Bedroom 1");
            PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.Equal(partFSpaceData.ContinuousSupplyFlowRate_Lps.Value / 1000.0, space.CalculatedSupplyAirFlow(), 9);
        }

        // -------------------------------------------------------------------------------------------------
        // J. The iteration and the route have to agree
        //
        // BasePassive (Iteration 1a) and BaseNaturalVentilation (Iteration 1b) are ALTERNATIVE base
        // configurations of the same dwelling, not successive stages. Which one applies is decided by the
        // route, and pairing them the other way round is not a formality: an iteration's operating
        // assumptions go into the derived OverheatingScenario.Key, so BasePassive permanently asserts
        // "Mechanical Ventilation At Design Rate = True" about every result attributed to it.
        // -------------------------------------------------------------------------------------------------

        /// <summary>Each base iteration is defined over exactly one route.</summary>
        [Fact]
        public void EachBaseIteration_StatesItsRoute()
        {
            Assert.Equal(PartOVentilationMode.MVHR, PartOIteration.BasePassive.PartOIterationVentilationMode(out string refusal));
            Assert.Null(refusal);

            Assert.Equal(PartOVentilationMode.NaturalVentilation, PartOIteration.BaseNaturalVentilation.PartOIterationVentilationMode(out refusal));
            Assert.Null(refusal);

            //The stages that are not characterised state no route, for the same reason they have no
            //Approved Document F operating condition.
            Assert.Equal(PartOVentilationMode.Undefined, PartOIteration.AcousticRestricted.PartOIterationVentilationMode(out refusal));
            Assert.NotNull(refusal);

            Assert.Equal(PartOVentilationMode.Undefined, PartOIteration.ActiveTrimCooling.PartOIterationVentilationMode(out refusal));
            Assert.NotNull(refusal);

            Assert.Equal(PartOVentilationMode.Undefined, PartOIteration.Undefined.PartOIterationVentilationMode(out refusal));
            Assert.NotNull(refusal);
        }

        /// <summary>
        /// <b>An NV dwelling is not prepared at the MVHR base iteration</b>, even though the airflow gate
        /// would have skipped correctly. The simulation would have been right and its permanent identity
        /// would have been wrong, which is the harder failure to notice.
        /// </summary>
        [Fact]
        public void AnNVDwellingAtTheMVHRIteration_Refuses()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BasePassive, null, Strategies(analyticalModel, "NV"));

            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);
            Assert.Empty(preparation.OverheatingScenarios);

            Assert.Contains("BasePassive", preparation.Refusal);
            Assert.Contains("BaseNaturalVentilation", preparation.Refusal);

            //The route itself was settled - this is not the unstated-route refusal wearing a different hat.
            Assert.Equal(PartOVentilationMode.NaturalVentilation, preparation.VentilationMode);
        }

        /// <summary>And the same both ways round.</summary>
        [Fact]
        public void AnMVHRDwellingAtTheNVIteration_Refuses()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(PartOIteration.BaseNaturalVentilation, null, Strategies(analyticalModel, "MVRE"));

            Assert.NotNull(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel);

            Assert.Contains("BaseNaturalVentilation", preparation.Refusal);
            Assert.Contains("BasePassive", preparation.Refusal);
            Assert.Equal(PartOVentilationMode.MVHR, preparation.VentilationMode);
        }

        /// <summary>
        /// The two base iterations state opposite things about mechanical ventilation, and both statements
        /// are inside the permanent scenario key. This is the reason BaseNaturalVentilation had to exist.
        /// </summary>
        [Fact]
        public void TheTwoBaseIterations_AssertOppositeMechanicalVentilationAssumptions()
        {
            OverheatingOperatingAssumptions assumptions_MVHR = PartOIteration.BasePassive.PartOOperatingAssumptions(out string _);
            OverheatingOperatingAssumptions assumptions_NV = PartOIteration.BaseNaturalVentilation.PartOOperatingAssumptions(out string _);

            Assert.Equal(OverheatingOperatingAssumptions.Text(true), assumptions_MVHR.Value(Analytical.Query.MechanicalVentilationAtDesignRate));
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), assumptions_NV.Value(Analytical.Query.MechanicalVentilationAtDesignRate));

            //Everything else about the two base configurations is the same: neither restricts openings,
            //neither has boost, neither has a summer bypass. Only the mechanical claim differs.
            Assert.Equal(OverheatingOperatingAssumptions.Text(false), assumptions_NV.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(assumptions_MVHR.Value(Analytical.Query.OpeningsRestricted), assumptions_NV.Value(Analytical.Query.OpeningsRestricted));
            Assert.Equal(assumptions_MVHR.Value(Analytical.Query.BoostAvailable), assumptions_NV.Value(Analytical.Query.BoostAvailable));
            Assert.Equal(assumptions_MVHR.Value(Analytical.Query.SummerBypassAvailable), assumptions_NV.Value(Analytical.Query.SummerBypassAvailable));
        }

        /// <summary>
        /// The two base iterations therefore key differently over the same zone, so an NV result and an
        /// MVHR result of the same dwelling can never be filed as one another.
        /// </summary>
        [Fact]
        public void TheTwoBaseIterations_KeyDifferentlyOverTheSameZone()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            OverheatingScenario overheatingScenario_MVHR = Assert.Single(Prepared(analyticalModel, PartOIteration.BasePassive, "MVRE").OverheatingScenarios);
            OverheatingScenario overheatingScenario_NV = Assert.Single(Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV").OverheatingScenarios);

            Assert.Equal(overheatingScenario_MVHR.ZoneGuid, overheatingScenario_NV.ZoneGuid);
            Assert.NotEqual(overheatingScenario_MVHR.Key, overheatingScenario_NV.Key);
        }

        /// <summary>
        /// <b>The stale-metadata control.</b> A dwelling that has been through a Part F sizing looks
        /// mechanical: its internal conditions carry <c>VentilationSystemTypeName = "MVRE"</c>, which is
        /// exactly what an inference-based route would read. The stated route wins, nothing mechanical is
        /// applied - and the metadata is still there afterwards, unchanged, because forcing a route by
        /// rewriting it would put the decision straight back into the metadata it was taken out of.
        /// </summary>
        [Fact]
        public void AStaleMVREOnTheModel_DoesNotOverrideAnExplicitNVRoute_AndIsNotRewritten()
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed, ventilationSystemTypeName: "MVRE");

            //The control: the model itself reads as mechanical, by the rule the rest of SAM still uses.
            Assert.True(Analytical.Query.IsMechanicalVentilation("MVRE"));

            PartOIterationPreparation preparation = Prepared(analyticalModel, PartOIteration.BaseNaturalVentilation, "NV");

            Assert.Equal(PartOVentilationMode.NaturalVentilation, preparation.VentilationMode);
            Assert.Equal(PartOPartFAirflowApplication.SkipNaturalVentilation, preparation.AirflowApplication);

            foreach (Space space in preparation.AnalyticalModel.GetSpaces())
            {
                AssertNoContinuousMechanicalAirflow(space);

                //Preserved, not corrected. It is evidence about the design, and this preparation is not the
                //place that reconciles it.
                Assert.Equal("MVRE", space.InternalCondition.GetSystemTypeName<VentilationSystemType>());
            }
        }

        // -------------------------------------------------------------------------------------------------
        // K. The Iteration 1b A/B: NV-OPEN against NV-NIGHT
        //
        // The licensed acceptance runs two cases through TAS off one dwelling, differing only in the
        // authored opening availability. These are the library-side invariants that make that comparison
        // mean anything: everything except the opening data has to be identical, and the opening data has
        // to be different in exactly the way it was authored.
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>NV-OPEN.</b> An unrestricted opening stays unrestricted, states no availability schedule at
        /// all, and agrees with the stage rather than being reported against it.
        /// </summary>
        [Fact]
        public void Iteration1b_Open_LeavesTheOpeningUnrestrictedAndCompatible()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.Unrestricted), PartOIteration.BaseNaturalVentilation, "NV");

            PartOOpeningProperties partOOpeningProperties = PartO(preparation.AnalyticalModel);

            Assert.Equal(OpeningRestriction.Unrestricted, partOOpeningProperties.OpeningRestriction);

            //Unrestricted is represented WITHOUT a schedule - there is nothing to make unavailable - so the
            //TAS write is given a bare function and no availability multiplier.
            Assert.Null(partOOpeningProperties.Schedule);

            Assert.Equal(PartOOpeningCompatibility.Compatible, preparation.OpeningCompatibility);
            Assert.True(preparation.Successful);
        }

        /// <summary>
        /// <b>NV-NIGHT.</b> The same dwelling with the restriction authored: NightClosed 08-23 survives and
        /// derives the schedule the TAS aperture-control write looks for, with all 24 values.
        /// </summary>
        [Fact]
        public void Iteration1b_Night_KeepsTheAuthoredRestrictionAndItsSchedule()
        {
            PartOIterationPreparation preparation = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");

            PartOOpeningProperties partOOpeningProperties = PartO(preparation.AnalyticalModel);

            Assert.Equal(OpeningRestriction.NightClosed, partOOpeningProperties.OpeningRestriction);
            Assert.Equal(8, partOOpeningProperties.NightOpenFromHour);
            Assert.Equal(23, partOOpeningProperties.NightOpenToHour);

            Assert.Equal("PartO_DayOpen_08_23", partOOpeningProperties.Schedule.Name);
            Assert.Equal("000000001111111111111110", partOOpeningProperties.Schedule.ValuesText);

            Assert.True(preparation.Successful);
        }

        /// <summary>
        /// <b>The A/B invariant.</b> Both cases take the same route, apply the same nothing, keep the same
        /// internal conditions and state the same scenario identity - so any difference the two TAS runs
        /// produce is attributable to the opening availability and to nothing else.
        /// <para>
        /// The scenario keys being EQUAL is the point, not an oversight: opening behaviour is a property of
        /// the model rather than of the stage, so both cases are the same assessment of the same zone at
        /// the same iteration. That is also why the results have to be told apart by the run they came
        /// from - see the OverheatingScenario:v2 note in the architecture document.
        /// </para>
        /// </summary>
        [Fact]
        public void Iteration1b_OpenAndNight_DifferOnlyInTheOpeningAvailability()
        {
            PartOIterationPreparation preparation_Open = Prepared(Model(OpeningRestriction.Unrestricted), PartOIteration.BaseNaturalVentilation, "NV");
            PartOIterationPreparation preparation_Night = Prepared(Model(OpeningRestriction.NightClosed), PartOIteration.BaseNaturalVentilation, "NV");

            Assert.Equal(preparation_Open.VentilationMode, preparation_Night.VentilationMode);
            Assert.Equal(preparation_Open.AirflowApplication, preparation_Night.AirflowApplication);

            OverheatingScenario overheatingScenario_Open = Assert.Single(preparation_Open.OverheatingScenarios);
            OverheatingScenario overheatingScenario_Night = Assert.Single(preparation_Night.OverheatingScenarios);

            //Compared field by field rather than by Key. The two cases are built by two calls to the
            //fixture, so each mints its own "Flat 1" with its own guid, and the zone guid is inside the
            //key - comparing keys here would compare the fixture, not the preparation. What has to match is
            //everything the preparation itself decides.
            Assert.Equal(overheatingScenario_Open.Iteration, overheatingScenario_Night.Iteration);
            Assert.Equal(overheatingScenario_Open.Scope, overheatingScenario_Night.Scope);
            Assert.Equal(overheatingScenario_Open.VentilationStrategy, overheatingScenario_Night.VentilationStrategy);
            Assert.Equal(
                SAM.Core.Convert.ToString(overheatingScenario_Open.OperatingAssumptions),
                SAM.Core.Convert.ToString(overheatingScenario_Night.OperatingAssumptions));

            AssertSameProfileNames(preparation_Open.AnalyticalModel, preparation_Night.AnalyticalModel);

            List<Space> spaces_Open = preparation_Open.AnalyticalModel.GetSpaces();
            List<Space> spaces_Night = preparation_Night.AnalyticalModel.GetSpaces();

            Assert.Equal(spaces_Open.Count, spaces_Night.Count);

            foreach (Space space_Open in spaces_Open)
            {
                Space space_Night = spaces_Night.Find(x => x.Name == space_Open.Name);

                Assert.NotNull(space_Night);

                AssertNoContinuousMechanicalAirflow(space_Open);
                AssertNoContinuousMechanicalAirflow(space_Night);

                //The internal conditions the simulation reads are identical - gains, setpoints, airflow
                //parameters and system references included - once the two fixture builds' own object guids
                //are taken out. Those differ because each case is built by its own call to Model(), which
                //is a fact about the fixture and not about the preparation.
                Assert.Equal(
                    WithoutGuids(SAM.Core.Convert.ToString(space_Open.InternalCondition)),
                    WithoutGuids(SAM.Core.Convert.ToString(space_Night.InternalCondition)));
            }

            //And the one thing that IS different.
            Assert.Null(PartO(preparation_Open.AnalyticalModel).Schedule);
            Assert.Equal("PartO_DayOpen_08_23", PartO(preparation_Night.AnalyticalModel).Schedule.Name);
        }

        // -------------------------------------------------------------------------------------------------
        // Fixture
        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// <b>The production preparation, called directly.</b> <c>Modify.PreparePartOIteration</c> is what
        /// <c>SAMAnalytical.PreparePartOIteration</c> calls for every decision it makes, so this exercises
        /// the shipped path rather than a copy of it - the component contributes only Grasshopper parameter
        /// reading and message levels on top.
        /// </summary>
        /// <param name="ventilationStrategy">
        /// The Part O route, stated for every zone. <c>MVRE</c> by default, which resolves to the MVHR
        /// route the opening tests above have always run on - the Part F airflow application is only
        /// reached there.
        /// </param>
        private static PartOIterationPreparation Prepared(AnalyticalModel analyticalModel, PartOIteration partOIteration, string ventilationStrategy = "MVRE")
        {
            PartOIterationPreparation result = analyticalModel.PreparePartOIteration(partOIteration, null, Strategies(analyticalModel, ventilationStrategy));

            Assert.Null(result.Refusal);
            Assert.NotNull(result.AnalyticalModel);

            return result;
        }

        /// <summary>One stated strategy for every zone the model carries.</summary>
        private static Dictionary<Guid, string> Strategies(AnalyticalModel analyticalModel, string ventilationStrategy)
        {
            List<Zone> zones = analyticalModel.GetZones() ?? new List<Zone>();

            Assert.NotEmpty(zones);

            Dictionary<Guid, string> result = new Dictionary<Guid, string>();
            foreach (Zone zone in zones)
            {
                result[zone.Guid] = ventilationStrategy;
            }

            return result;
        }

        /// <summary>
        /// The same dwelling with a SECOND zone beside "Flat 1", so a model can state two different
        /// ventilation strategies at once. Neither zone holds spaces - the airflow decision is taken from
        /// what the zones STATE, and adding rooms to them would prove nothing extra about that.
        /// </summary>
        private static AnalyticalModel ModelWithTwoZones(out Zone zone_NaturalVentilation, out Zone zone_Mechanical)
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.NightClosed);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            adjacencyCluster.AddObject(new Zone("Flat 2"));

            AnalyticalModel result = new AnalyticalModel(analyticalModel, adjacencyCluster);

            List<Zone> zones = result.GetZones();

            zone_NaturalVentilation = zones.Find(x => x.Name == "Flat 1");
            zone_Mechanical = zones.Find(x => x.Name == "Flat 2");

            Assert.NotNull(zone_NaturalVentilation);
            Assert.NotNull(zone_Mechanical);

            return result;
        }

        /// <summary>
        /// One properly Part-F-sized and zoned dwelling ("Flat 1", from <see cref="Model"/>), plus a second,
        /// independent space carrying its own directly-authored Part F requirement in a SEPARATE zone
        /// ("Flat 2") that the test does not assess. The second space is deliberately given no transfer-air
        /// partition - scoped out of the assessed dwelling, it is never meant to reach the balance or
        /// transfer-air steps at all, only to prove it is kept off the assessed dwelling's shared unit.
        /// </summary>
        private static AnalyticalModel ModelWithTwoSizedDwellings(out Zone zone_Assessed, out Space space_Unassessed)
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            zone_Assessed = adjacencyCluster.GetObjects<Zone>()?.Find(x => x.Name == "Flat 1");
            Assert.NotNull(zone_Assessed);

            //Model() already relates "Flat 1" to every space it builds.

            Space space = new Space("Flat 2 Bedroom");
            space.SetValue(SpaceParameter.Area, 12.0);
            space.SetValue(SpaceParameter.Volume, 30.0);

            InternalCondition internalCondition = new InternalCondition("Flat 2 Bedroom IC");
            internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, "Ventilation System");
            space.InternalCondition = internalCondition;

            PartFSpaceData partFSpaceData = new PartFSpaceData(
                "Flat 2 Bedroom",
                PartFType.Habitable,
                PartFVentilationType.supply,
                true,
                null,
                true,
                true,
                true,
                false,
                "Volume",
                8.0);

            partFSpaceData.Terminals.Add(new PartFVentilationTerminalRequirement("Flat 2 Bedroom - Supply", space.Guid, PartFTerminalRole.Supply)
            {
                SpaceName = space.Name,
                OperatingMode = PartFOperatingMode.ContinuousDesign,
                ContinuousDesignFlowRate_Lps = 8.0,
                IsInBalancedFlow = true,
                IsRequired = true,
                SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.67 (page 16)",
            });

            space.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData);

            adjacencyCluster.AddObject(space);

            Zone zone_Unassessed = new Zone("Flat 2");
            adjacencyCluster.AddObject(zone_Unassessed);
            adjacencyCluster.AddRelation(zone_Unassessed, space);

            space_Unassessed = space;

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Two INDEPENDENT, properly Part-F-sized and zoned dwellings, both meant to be assessed together:
        /// "Flat 1" (from <see cref="Model"/>, 5 rooms) and a second, smaller "Flat 2" (a supplied habitable
        /// room and an extracted wet room, partitioned so its own transfer air network closes on its own -
        /// the smallest shape that can complete the whole Base MVHR chain independently of Flat 1).
        /// <para>
        /// Unlike <see cref="ModelWithTwoSizedDwellings"/>, BOTH dwellings here are meant to be assessed -
        /// this fixture is for proving what happens when Approved Document F's "size each dwelling
        /// independently" rule reaches Base MVHR realization, not for proving an unassessed dwelling is
        /// left alone.
        /// </para>
        /// </summary>
        // ---- Running selected dwellings in isolation ---------------------------------------------------

        /// <summary>
        /// Preparing one of two assessed dwellings with isolation asked for returns the <b>isolated derived
        /// model</b>: only that dwelling's spaces are thermal spaces, and the model states what it was
        /// isolated for.
        /// </summary>
        [Fact]
        public void PreparePartOIteration_Isolated_SimulatesOnlyTheSelectedDwelling()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                true);

            Assert.Null(preparation.Refusal);
            Assert.NotNull(preparation.AnalyticalModel);

            //Flat 2's rooms were in the source model and are not in the simulated one.
            foreach (Space space in preparation.AnalyticalModel.GetSpaces() ?? [])
            {
                Assert.DoesNotContain("Flat 2", space.Name, StringComparison.Ordinal);
            }

            PartOIsolationContext partOIsolationContext = preparation.AnalyticalModel.GetValue<PartOIsolationContext>(AnalyticalModelParameter.PartOIsolationContext);

            Assert.NotNull(partOIsolationContext);
            Assert.True(partOIsolationContext.IsValid);
            Assert.Contains(zone_1.Guid, partOIsolationContext.Guids_Zone);
            Assert.Contains(zone_1.Name, partOIsolationContext.Names_Dwelling);
        }

        /// <summary>
        /// The same preparation without isolation keeps the whole building - and carries no isolation
        /// context, so "isolated" is always something a model states rather than something inferred.
        /// </summary>
        [Fact]
        public void PreparePartOIteration_NotIsolated_KeepsTheWholeBuilding()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } });

            Assert.Null(preparation.Refusal);
            Assert.Null(preparation.AnalyticalModel.GetValue<PartOIsolationContext>(AnalyticalModelParameter.PartOIsolationContext));

            Assert.Contains(preparation.AnalyticalModel.GetSpaces() ?? [], x => x.Name.StartsWith("Flat 2", StringComparison.Ordinal));
        }

        /// <summary>
        /// <b>Isolation is a thermal model scope, not a ventilation sizing method.</b> The selected
        /// dwelling's Approved Document F requirement and its design terminal airflow are identical whether
        /// or not the rest of the building was simulated - which is the whole reason isolation is applied
        /// after the preparation rather than before it.
        /// </summary>
        [Fact]
        public void PreparePartOIteration_Isolated_DoesNotMovePartFOrTheDesignDuty()
        {
            //ONE model, prepared twice. The fixture mints fresh guids on every call, so two separately
            //built buildings could not be compared by identity at all - and identity is exactly what this
            //test is about. The preparation works on a copy, so the second call sees the same source as the
            //first.
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation_Full = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } });

            PartOIterationPreparation preparation_Isolated = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                true);

            Assert.Null(preparation_Full.Refusal);
            Assert.Null(preparation_Isolated.Refusal);

            //The dwelling's own design duty - the middle term of the Iteration 2 invariant - is untouched.
            Assert.Equal(preparation_Full.DesignSupplyDuty_Lps, preparation_Isolated.DesignSupplyDuty_Lps, 6);
            Assert.Equal(preparation_Full.DesignExtractDuty_Lps, preparation_Isolated.DesignExtractDuty_Lps, 6);

            //And so is every selected space's Part F data.
            foreach (Space space_Isolated in preparation_Isolated.AnalyticalModel.GetSpaces() ?? [])
            {
                Space space_Full = preparation_Full.AnalyticalModel.GetSpaces()?.Find(x => x.Guid == space_Isolated.Guid);

                Assert.NotNull(space_Full);

                PartFSpaceData partFSpaceData_Full = space_Full.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                PartFSpaceData partFSpaceData_Isolated = space_Isolated.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

                Assert.Equal(partFSpaceData_Full?.ToJsonObject()?.ToJsonString(), partFSpaceData_Isolated?.ToJsonObject()?.ToJsonString());
            }
        }

        /// <summary>
        /// Only the isolated dwelling is in the run's assessment scope. An excluded dwelling is not reported
        /// as failed, passed or unassessed inside this run - it simply is not part of it.
        /// </summary>
        [Fact]
        public void PreparePartOIteration_Isolated_ScopesTheOverheatingScenariosToTheSelection()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                true);

            Assert.Null(preparation.Refusal);

            OverheatingScenario overheatingScenario = Assert.Single(preparation.OverheatingScenarios);

            Assert.Equal(zone_1.Guid, overheatingScenario.ZoneGuid);
            Assert.NotEqual(zone_2.Guid, overheatingScenario.ZoneGuid);
        }

        /// <summary>
        /// The preparation records what isolation did, so the dialog can show the scope and the cut before
        /// anybody starts a long conversion.
        /// </summary>
        [Fact]
        public void PreparePartOIteration_Isolated_ReportsTheScope()
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                true);

            Assert.Null(preparation.Refusal);
            Assert.Contains(preparation.Notes, x => x.Contains("Thermal model scope: ISOLATED", StringComparison.Ordinal));
        }

        // ---- The generated MVHR plant zone's humidistat ------------------------------------------------

        /// <summary>
        /// <b>The prepared unit states no humidity control as a valid pair of limits</b> - humidity lower
        /// limit 0%, upper limit 100% - whether or not the run is isolated.
        /// <para>
        /// These two profiles become the humidistat on the unit's generated TAS plant zone ("MVHR-01"):
        /// <c>SAM.Analytical.Tas.Modify.UpdateIZAMs</c> writes Humidification to the humidity LOWER limit
        /// (TBD <c>Profiles.ticHLL</c>) and Dehumidification to the UPPER (<c>ticHUL</c>). Transposed -
        /// lower 100%, upper 0% - TAS refuses the model before simulating: "Internal Condition 'MVHR-01'
        /// humidistat has overlapping limits". See <c>PartOHumidistatTests</c> for the whole account,
        /// including that the transposition predates both Part O and isolation.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreparePartOIteration_TheUnitStatesNoHumidityControlAsAValidLimitPair(bool isolate)
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                isolate);

            Assert.Null(preparation.Refusal);

            List<AirHandlingUnitAirMovement> airHandlingUnitAirMovements = preparation.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnitAirMovement>();

            Assert.NotNull(airHandlingUnitAirMovements);
            Assert.NotEmpty(airHandlingUnitAirMovements);

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in airHandlingUnitAirMovements)
            {
                Profile profile_LowerLimit = airHandlingUnitAirMovement.Humidification;
                Profile profile_UpperLimit = airHandlingUnitAirMovement.Dehumidification;

                Assert.NotNull(profile_LowerLimit);
                Assert.NotNull(profile_UpperLimit);

                Assert.Equal(0, profile_LowerLimit[0], 6);
                Assert.Equal(100, profile_UpperLimit[0], 6);

                //At every hour, not only the first: the assertion is that the limits never cross, and a
                //value profile repeats over the day.
                for (int i = 0; i <= 23; i++)
                {
                    Assert.True(
                        profile_LowerLimit[i] <= profile_UpperLimit[i],
                        string.Format("Hour {0}: lower limit {1} is above upper limit {2}.", i, profile_LowerLimit[i], profile_UpperLimit[i]));
                }
            }
        }

        /// <summary>
        /// <b>A Flat-1-style prepared model reports no overlapping humidity limits from the Check authority
        /// the Part O pre-simulation gate runs</b> - isolated, which is the run the defect was found on, and
        /// not isolated, which had the same defect and simply had not been read out of TAS yet.
        /// <para>
        /// Asserted on the whole-model log, which is exactly what <c>PartOPreSimulationCheck</c> runs, and
        /// on each generated unit's own record, which is where such an error comes from.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PreparePartOIteration_ThePreparedModelReportsNoOverlappingHumidityLimits(bool isolate)
        {
            AnalyticalModel analyticalModel = ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone _);

            PartOIterationPreparation preparation = analyticalModel.PreparePartOIteration(
                PartOIteration.BasePassive,
                new List<Zone> { zone_1 },
                new Dictionary<Guid, string> { { zone_1.Guid, "MVRE" } },
                null,
                isolate);

            Assert.Null(preparation.Refusal);

            Assert.Empty(PartOHumidistatTests.HumidityRecords(Analytical.Create.Log(preparation.AnalyticalModel)));

            foreach (AirHandlingUnitAirMovement airHandlingUnitAirMovement in preparation.AnalyticalModel.AdjacencyCluster.GetObjects<AirHandlingUnitAirMovement>() ?? [])
            {
                Assert.Empty(Analytical.Create.Log(airHandlingUnitAirMovement));
            }
        }

        private static AnalyticalModel ModelWithTwoAssessedDwellings(out Zone zone_1, out Zone zone_2)
        {
            AnalyticalModel analyticalModel = Model(OpeningRestriction.Unrestricted);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            zone_1 = adjacencyCluster.GetObjects<Zone>()?.Find(x => x.Name == "Flat 1");
            Assert.NotNull(zone_1);

            //Model() already relates "Flat 1" to every space it builds.

            Space space_Supply = new Space("Flat 2 Bedroom");
            space_Supply.SetValue(SpaceParameter.Area, 12.0);
            space_Supply.SetValue(SpaceParameter.Volume, 30.0);

            InternalCondition internalCondition_Supply = new InternalCondition("Flat 2 Bedroom IC");
            internalCondition_Supply.SetValue(InternalConditionParameter.VentilationSystemTypeName, "Ventilation System");
            space_Supply.InternalCondition = internalCondition_Supply;

            PartFSpaceData partFSpaceData_Supply = new PartFSpaceData(
                "Flat 2 Bedroom",
                PartFType.Habitable,
                PartFVentilationType.supply,
                true,
                null,
                true,
                true,
                true,
                false,
                "Volume",
                8.0);

            partFSpaceData_Supply.Terminals.Add(new PartFVentilationTerminalRequirement("Flat 2 Bedroom - Supply", space_Supply.Guid, PartFTerminalRole.Supply)
            {
                SpaceName = space_Supply.Name,
                OperatingMode = PartFOperatingMode.ContinuousDesign,
                ContinuousDesignFlowRate_Lps = 8.0,
                IsInBalancedFlow = true,
                IsRequired = true,
                SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.67 (page 16)",
            });

            space_Supply.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData_Supply);

            adjacencyCluster.AddObject(space_Supply);

            Space space_Extract = new Space("Flat 2 Bathroom");
            space_Extract.SetValue(SpaceParameter.Area, 6.0);
            space_Extract.SetValue(SpaceParameter.Volume, 15.0);

            InternalCondition internalCondition_Extract = new InternalCondition("Flat 2 Bathroom IC");
            internalCondition_Extract.SetValue(InternalConditionParameter.VentilationSystemTypeName, "Ventilation System");
            space_Extract.InternalCondition = internalCondition_Extract;

            //Matched to the Bedroom's own 8.0 l/s supply, not a different figure: with only these two rooms
            //in Flat 2's star, the transfer air network has nowhere else to absorb an imbalance, and TAS
            //refuses to simulate a zone whose air movements do not balance.
            PartFSpaceData partFSpaceData_Extract = new PartFSpaceData(
                "Flat 2 Bathroom",
                PartFType.WetRoom,
                PartFVentilationType.extract,
                false,
                null,
                true,
                true,
                false,
                false,
                "Volume",
                8.0);

            partFSpaceData_Extract.Terminals.Add(new PartFVentilationTerminalRequirement("Flat 2 Bathroom - Extract", space_Extract.Guid, PartFTerminalRole.GeneralExtract)
            {
                SpaceName = space_Extract.Name,
                OperatingMode = PartFOperatingMode.ContinuousDesign,
                ContinuousDesignFlowRate_Lps = 8.0,
                IsInBalancedFlow = true,
                IsRequired = true,
                SourceReference = "Approved Document F, Volume 1: Dwellings (2021 edition), paragraph 1.17 (page 8), Table 1.2 (page 10) and paragraph 1.70 (page 17)",
            });

            space_Extract.SetValue(SpaceParameter.PartFSpaceData, partFSpaceData_Extract);

            adjacencyCluster.AddObject(space_Extract);

            //The partition that closes Flat 2's own transfer air network, independently of Flat 1's.
            Helpers.DwellingPartitions.Partition(adjacencyCluster, space_Supply.Name, space_Extract.Name, 100);

            zone_2 = new Zone("Flat 2");
            adjacencyCluster.AddObject(zone_2);
            adjacencyCluster.AddRelation(zone_2, space_Supply);
            adjacencyCluster.AddRelation(zone_2, space_Extract);

            return new AnalyticalModel(analyticalModel, adjacencyCluster);
        }

        /// <summary>
        /// Nothing the simulation could read as continuous mechanical supply or extract reached this space.
        /// <para>
        /// <c>CalculatedSupplyAirFlow</c> answers <c>NaN</c> where no rate is stated on any of its four
        /// summing bases, and that is the honest answer here rather than a defect: a stated ZERO is what the
        /// mechanical path writes onto a wet room to say "no supply here, make-up air arrives as transfer
        /// air", which is a different fact from "nobody stated a supply rate at all". Either value is
        /// accepted; a positive one is not.
        /// </para>
        /// <para>
        /// The parameter checks are the stronger half: the two values the TAS export reads were never
        /// written, so no mechanical system was invented rather than one being written and then zeroed.
        /// </para>
        /// </summary>
        private static void AssertNoContinuousMechanicalAirflow(Space space)
        {
            double supply = space.CalculatedSupplyAirFlow();
            double extract = space.CalculatedExhaustAirFlow();

            Assert.True(double.IsNaN(supply) || supply == 0, string.Format("Space '{0}' was given a continuous mechanical supply of {1} m3/s that its dwelling does not have.", space.Name, supply));
            Assert.True(double.IsNaN(extract) || extract == 0, string.Format("Space '{0}' was given a continuous mechanical extract of {1} m3/s that its dwelling does not have.", space.Name, extract));

            Assert.False(space.InternalCondition.TryGetValue(InternalConditionParameter.SupplyAirFlow, out double _));
            Assert.False(space.InternalCondition.TryGetValue(InternalConditionParameter.ExhaustAirFlow, out double _));
        }

        /// <summary>
        /// Compared entry by entry rather than as nested dictionaries, because the inner dictionaries compare
        /// by reference and a nested Assert.Equal would pass for the wrong reason.
        /// </summary>
        private static void AssertSameProfileNames(AnalyticalModel analyticalModel_Expected, AnalyticalModel analyticalModel)
        {
            Dictionary<string, Dictionary<ProfileType, string>> expected = ProfileNames(analyticalModel_Expected);
            Dictionary<string, Dictionary<ProfileType, string>> actual = ProfileNames(analyticalModel);

            Assert.Equal(expected.Count, actual.Count);

            foreach (KeyValuePair<string, Dictionary<ProfileType, string>> keyValuePair in expected)
            {
                Assert.Equal(keyValuePair.Value, actual[keyValuePair.Key]);
            }
        }

        private static readonly Construction wallConstruction = new Construction(Guid.NewGuid(), "Wall");
        private static readonly ApertureConstruction windowConstruction = new ApertureConstruction(Guid.NewGuid(), "Window", ApertureType.Window);

        private static Point3D P(double x, double y, double z) => new Point3D(x, y, z);

        private static Face3D WallFace() => new Face3D(new Polygon3D(new Point3D[] { P(0, 0, 0), P(10, 0, 0), P(10, 10, 0), P(0, 10, 0) }));
        private static Face3D ApertureFace(double offset = 0) => new Face3D(new Polygon3D(new Point3D[] { P(1 + offset, 1, 0), P(3 + offset, 1, 0), P(3 + offset, 3, 0), P(1 + offset, 3, 0) }));

        /// <summary>Which opening carrier the fixture states, so each classification branch has a model.</summary>
        private enum OpeningPropertiesKind
        {
            /// <summary>A PartOOpeningProperties stating the restriction under test.</summary>
            PartO,

            /// <summary>No opening properties at all - not an operable opening.</summary>
            None,

            /// <summary>A first-class availability schedule, available all 24 hours.</summary>
            ScheduleAlwaysAvailable,

            /// <summary>A first-class availability schedule with the night hours off.</summary>
            ScheduleNightShut,

            /// <summary>The legacy general-valued carrier, with no schedule beside it.</summary>
            LegacyProfile,

            /// <summary>The profile carrier stating neither schedule nor profile.</summary>
            ProfileCarrierWithNothing,
        }

        private static bool? Restricted(IOpeningProperties openingProperties)
        {
            return openingProperties.PartOOpeningRestricted(out string _);
        }

        private static DailyAvailabilitySchedule AllAvailable()
        {
            bool[] values = new bool[DailyAvailabilitySchedule.HourCount];
            for (int hour = 0; hour < values.Length; hour++)
            {
                values[hour] = true;
            }

            return new DailyAvailabilitySchedule("Always Available", values);
        }

        private static DailyAvailabilitySchedule NightShut()
        {
            bool[] values = new bool[DailyAvailabilitySchedule.HourCount];
            for (int hour = 0; hour < values.Length; hour++)
            {
                values[hour] = hour >= 8 && hour < 23;
            }

            return new DailyAvailabilitySchedule("Night Shut", values);
        }

        private static PartOOpeningProperties PartO(AnalyticalModel analyticalModel)
        {
            Assert.True(SingleAperture(analyticalModel).TryGetValue(ApertureParameter.OpeningProperties, out IOpeningProperties openingProperties));

            return Assert.IsType<PartOOpeningProperties>(openingProperties);
        }

        private static Aperture SingleAperture(AnalyticalModel analyticalModel)
        {
            List<Aperture> apertures = analyticalModel.AdjacencyCluster.GetApertures();

            Assert.NotNull(apertures);

            return apertures[0];
        }

        /// <summary>
        /// The same JSON with every object guid's VALUE blanked, so two independently built fixtures can be
        /// compared on their engineering content. Only the value is blanked, never the whole line - dropping
        /// the property would also hide a missing one.
        /// </summary>
        private static string WithoutGuids(string json)
        {
            return System.Text.RegularExpressions.Regex.Replace(json, "\"Guid\": \"[^\"]*\"", "\"Guid\": \"\"");
        }

        /// <summary>Every profile reference every space's internal condition carries, keyed by space name.</summary>
        private static Dictionary<string, Dictionary<ProfileType, string>> ProfileNames(AnalyticalModel analyticalModel)
        {
            Dictionary<string, Dictionary<ProfileType, string>> result = new Dictionary<string, Dictionary<ProfileType, string>>();

            foreach (Space space in analyticalModel.GetSpaces())
            {
                InternalCondition internalCondition = space?.InternalCondition;
                if (internalCondition == null)
                {
                    continue;
                }

                result[space.Name] = internalCondition.GetProfileTypeDictionary();
            }

            return result;
        }

        /// <summary>
        /// A Part-F-sized dwelling whose internal conditions carry a full set of profile references, one zone
        /// to state a scenario over, and one wall carrying the authored opening data under test. The Part F
        /// sizing is the real calculator over the shipped rule set - nothing is stubbed, so what the tests
        /// read back is what the production path produces.
        /// </summary>
        private static AnalyticalModel Model(OpeningRestriction openingRestriction, int nightOpenFromHour = 8, int nightOpenToHour = 23, double? supplyAirFlowPerArea = null, OpeningPropertiesKind openingProperties = OpeningPropertiesKind.PartO, OpeningRestriction? second = null, bool partF = true, string ventilationSystemTypeName = "Ventilation System")
        {
            AdjacencyCluster adjacencyCluster = new AdjacencyCluster();

            //Named so the shared space-use classification recognises them.
            Dictionary<string, double> dictionary = new Dictionary<string, double>()
            {
                { "Living Room", 30.0 },
                { "Bedroom 1", 16.0 },
                { "Bedroom 2", 11.0 },
                { "Kitchen", 12.0 },
                { "Bathroom", 6.0 },
            };

            foreach (KeyValuePair<string, double> keyValuePair in dictionary)
            {
                Space space = new Space(keyValuePair.Key);

                space.SetValue(SpaceParameter.Area, keyValuePair.Value);
                space.SetValue(SpaceParameter.Volume, keyValuePair.Value * 2.5);

                InternalCondition internalCondition = new InternalCondition(keyValuePair.Key + " IC");

                internalCondition.SetProfileName(ProfileType.Occupancy, "Occupancy 24h");
                internalCondition.SetProfileName(ProfileType.EquipmentSensible, "Equipment Sensible Weekday");
                internalCondition.SetProfileName(ProfileType.EquipmentLatent, "Equipment Latent Weekday");
                internalCondition.SetProfileName(ProfileType.Lighting, "Lighting Weekday");
                internalCondition.SetProfileName(ProfileType.Infiltration, "Infiltration Constant");
                internalCondition.SetProfileName(ProfileType.Pollutant, "Pollutant Constant");
                internalCondition.SetProfileName(ProfileType.Heating, "Heating Setpoint");
                internalCondition.SetProfileName(ProfileType.Cooling, "Cooling Setpoint");
                internalCondition.SetProfileName(ProfileType.Humidification, "Humidification Setpoint");
                internalCondition.SetProfileName(ProfileType.Dehumidification, "Dehumidification Setpoint");
                internalCondition.SetProfileName(ProfileType.Ventilation, "Ventilation Continuous");

                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);
                internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, "Heating System");
                internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, "Cooling System");

                if (supplyAirFlowPerArea.HasValue)
                {
                    internalCondition.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, supplyAirFlowPerArea.Value);
                }

                space.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space);
            }

            //A flat whose rooms all open off the living room. A dwelling with no internal partitions has no
            //transfer air network, and the Base MVHR preparation refuses one - a supplied bedroom whose air
            //cannot reach an extract is a building TAS will not simulate.
            Helpers.DwellingPartitions.Star(adjacencyCluster, "Living Room", "Bedroom 1", "Bedroom 2", "Kitchen", "Bathroom");

            ProfileLibrary profileLibrary = new ProfileLibrary("Part O Fixture");

            profileLibrary.Add(new Profile("Occupancy 24h", ProfileType.Occupancy, Values(1)));
            profileLibrary.Add(new Profile("Heating Setpoint", ProfileType.Heating, Values(20)));
            profileLibrary.Add(new Profile("Cooling Setpoint", ProfileType.Cooling, Values(24)));
            profileLibrary.Add(new Profile("Infiltration Constant", ProfileType.Infiltration, Values(0.25)));

            AnalyticalModel analyticalModel = new AnalyticalModel("Part O Dwelling", null, null, null, adjacencyCluster, null, profileLibrary);

            //partF: false gives the same dwelling with no PartFSpaceData on any space - a model that was never
            //run through a Part F component, which is a legitimate starting point for a naturally ventilated
            //dwelling and an outright refusal for a mechanical one.
            AdjacencyCluster adjacencyCluster_Sized;

            if (partF)
            {
                PartFCalculator partFCalculator = Analytical.Query.DefaultPartFCalculator();

                Assert.NotNull(partFCalculator);

                partFCalculator.AdjacencyCluster = analyticalModel.AdjacencyCluster;

                Assert.True(partFCalculator.Calculate(), "The Part F calculation did not run, so every test resting on it would be meaningless.");

                //The wall and the zone are added AFTER the sizing, so the fixture states exactly the openings
                //under test and the Part F calculation is the same one PartFAirflowApplicationTests exercises.
                adjacencyCluster_Sized = partFCalculator.AdjacencyCluster;
            }
            else
            {
                adjacencyCluster_Sized = analyticalModel.AdjacencyCluster;
            }

            Zone zone_Flat1 = new Zone("Flat 1");
            adjacencyCluster_Sized.AddObject(zone_Flat1);

            //Related to every space this fixture builds, matching how a real model's own zoning relates a
            //dwelling zone to its rooms. Modify.PrepareBaseMVHR partitions the assessed scope into one Base
            //MVHR system per dwelling zone using exactly this relation (Query.PartFDwellingZones plus the
            //zone -> space relation), so a zone this fixture leaves unrelated would resolve to no spaces at
            //all rather than to "every space", which is what every existing test here still assesses.
            foreach (Space space_Existing in adjacencyCluster_Sized.GetSpaces())
            {
                adjacencyCluster_Sized.AddRelation(zone_Flat1, space_Existing);
            }

            Panel panel = AnalyticalCreate.Panel(wallConstruction, PanelType.Wall, WallFace());

            Aperture aperture = AnalyticalCreate.Aperture(windowConstruction, ApertureFace());

            ISingleOpeningProperties singleOpeningProperties = null;
            switch (openingProperties)
            {
                case OpeningPropertiesKind.None:
                    break;

                case OpeningPropertiesKind.ScheduleAlwaysAvailable:
                    singleOpeningProperties = new ProfileOpeningProperties(0.6, AllAvailable());
                    break;

                case OpeningPropertiesKind.ScheduleNightShut:
                    singleOpeningProperties = new ProfileOpeningProperties(0.6, NightShut());
                    break;

                case OpeningPropertiesKind.LegacyProfile:
                    singleOpeningProperties = new ProfileOpeningProperties(0.6, new Profile("Night Shut", ProfileType.Other, Values(1)));
                    break;

                case OpeningPropertiesKind.ProfileCarrierWithNothing:
                    singleOpeningProperties = new ProfileOpeningProperties(0.6);
                    break;

                default:
                    singleOpeningProperties = new PartOOpeningProperties(1.2, 1.0, 30.0, openingRestriction, nightOpenFromHour, nightOpenToHour) { Factor = 0.75 };
                    break;
            }

            if (singleOpeningProperties != null)
            {
                singleOpeningProperties.SetValue(OpeningPropertiesParameter.Function, "zdwno,0,19.00,21.00,99.00");

                aperture.AddSingleOpeningProperties(singleOpeningProperties);
            }

            panel.AddAperture(aperture);

            if (second.HasValue)
            {
                Aperture aperture_Second = AnalyticalCreate.Aperture(windowConstruction, ApertureFace(4));
                aperture_Second.AddSingleOpeningProperties(new PartOOpeningProperties(1.2, 1.0, 30.0, second.Value));

                panel.AddAperture(aperture_Second);
            }

            adjacencyCluster_Sized.AddObject(panel);

            return new AnalyticalModel(analyticalModel, adjacencyCluster_Sized);
        }

        private static IEnumerable<double> Values(double value)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < 24; i++)
            {
                result.Add(value);
            }

            return result;
        }
    }
}
