// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for purge ventilation (Approved Document F, Volume 1: Dwellings, 2021 edition, paragraphs
    /// 1.26 to 1.31 and Table 1.4, page 11), for commissioning (Section 4 and Appendix C), and for the way
    /// the clause-level checks add up to an overall Part F conformance assessment status.
    /// </summary>
    public class PartFPurgeAndComplianceTests
    {
        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // Purge ventilation
        // ------------------------------------------------------------------

        /// <summary>
        /// Paragraph 1.27: at least four air changes per hour, directly to the outside. A 100 m3 room
        /// therefore needs 400 m3/h, which is 111.11 l/s.
        /// </summary>
        [Fact]
        public void RequiredPurgeRate_IsFourAirChangesPerHour()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Bathroom", "D01")
                .ExternalWall("Living Room"), "Living Room");

            Assert.True(partFPurgeVentilationData.IsRequired);
            Assert.Equal(4, partFPurgeVentilationData.RequiredAirChangesPerHour_Value, tolerance);
            Assert.Equal(100, partFPurgeVentilationData.RoomVolume_M3.Value, tolerance);
            Assert.Equal(4 * 100 * 1000 / 3600.0, partFPurgeVentilationData.RequiredPurgeRate_Lps.Value, 1e-6);
        }

        /// <summary>
        /// Paragraph 1.26 requires purge ventilation in HABITABLE rooms, so a wet room carries no purge
        /// record at all rather than a "not applicable" one for every bathroom in the dwelling.
        /// </summary>
        [Fact]
        public void WetRoom_CarriesNoPurgeRecord()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Bathroom", "D01")
                .ExternalWall("Living Room"));

            Assert.DoesNotContain(complianceResult.PurgeVentilation, x => x.SpaceName == "Bathroom");
            Assert.Contains(complianceResult.PurgeVentilation, x => x.SpaceName == "Living Room");
        }

        /// <summary>
        /// Table 1.4 selects the required area by opening type and angle, both of which are product
        /// properties. With neither recorded there is no row, so the required area is unknown and the room
        /// cannot be passed - it is NOT defaulted to the more permissive 1/20.
        /// </summary>
        [Fact]
        public void UnknownOpeningType_CannotBeDetermined()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Bathroom", "D01")
                .ExternalWall("Living Room"), "Living Room");

            Assert.Equal(PartFPurgeOpeningType.Undefined, partFPurgeVentilationData.OpeningType);
            Assert.Null(partFPurgeVentilationData.RequiredOpeningArea_M2);
            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// The window area the model carries is the area of the WINDOWS, not the area they open to, so it
        /// is reported as context and never used as the openable area. A fixed light adds to it and opens
        /// nothing.
        /// </summary>
        [Fact]
        public void ModelledWindowArea_IsNeverUsedAsTheOpenableArea()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Bathroom", "D01")
                .ExternalWall("Living Room"), "Living Room");

            Assert.True(partFPurgeVentilationData.HasExternalOpening);
            Assert.NotNull(partFPurgeVentilationData.ExternalApertureArea_M2);
            Assert.Null(partFPurgeVentilationData.OpenableWindowArea_M2);
            Assert.Contains("the area of the windows and not the area they open to", partFPurgeVentilationData.Diagnostic);
        }

        /// <summary>
        /// Table 1.4: a hinged or pivot window opening 30 degrees or more needs 1/20 of the room floor
        /// area. 40 m2 needs 2 m2, and 2.5 m2 of openable area satisfies it.
        /// </summary>
        [Fact]
        public void AdequateOpeningArea_Passes()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.HingedOrPivot30DegreesOrMore, openableWindowArea_M2: 2.5, openingAngle_Degrees: 45), "Living Room");

            Assert.Equal(2, partFPurgeVentilationData.RequiredOpeningArea_M2.Value, tolerance);
            Assert.Equal(PartFComplianceStatus.Pass, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>1.5 m2 of openable area is below the 2 m2 Table 1.4 requires.</summary>
        [Fact]
        public void InadequateOpeningArea_Fails()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.HingedOrPivot30DegreesOrMore, openableWindowArea_M2: 1.5), "Living Room");

            Assert.Equal(PartFComplianceStatus.Fail, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// <b>A non-finite recorded area must not pass.</b> NaN fails every comparison, so it would fall
        /// through both area checks and pass the room - absence of evidence reading as compliance.
        /// </summary>
        [Fact]
        public void NonFiniteProvidedOpeningArea_IsNotTakenAsPassing()
        {
            PartFModel partFModel = Model();

            PartFPurgeVentilationData partFPurgeVentilationData = PartFPurgeAssessor.Assess(
                partFModel.Get("Living Room"),
                partFModel.AdjacencyCluster,
                true,
                new PartFPurgeVentilationData("Living Room")
                {
                    PurgeMethod = PartFPurgeMethod.Openings,
                    OpeningType = PartFPurgeOpeningType.HingedOrPivot30DegreesOrMore,
                    OpenableWindowArea_M2 = double.NaN,
                });

            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// Table 1.4: a hinged or pivot window opening between 15 and 30 degrees needs 1/10 of the floor
        /// area, twice as much. The same 2.5 m2 that passed at 45 degrees now falls short of 4 m2.
        /// </summary>
        [Fact]
        public void ShallowerOpeningAngle_NeedsTwiceTheArea()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.HingedOrPivot15To30Degrees, openableWindowArea_M2: 2.5, openingAngle_Degrees: 20), "Living Room");

            Assert.Equal(4, partFPurgeVentilationData.RequiredOpeningArea_M2.Value, tolerance);
            Assert.Equal(PartFComplianceStatus.Fail, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// Paragraph 1.31: hinged or pivot windows with an opening angle of less than 15 degrees are not
        /// suitable for purge ventilation, so no amount of that area counts.
        /// </summary>
        [Fact]
        public void OpeningAngleBelowFifteenDegrees_Fails()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.HingedOrPivotUnder15Degrees, openableWindowArea_M2: 20, openingAngle_Degrees: 10), "Living Room");

            Assert.Equal(PartFComplianceStatus.Fail, partFPurgeVentilationData.ComplianceStatus);
            Assert.Contains("not suitable for purge ventilation", partFPurgeVentilationData.Diagnostic);
        }

        /// <summary>An external door counts too, at the same 1/20 Table 1.4 fraction as an opening sash.</summary>
        [Fact]
        public void ExternalDoorOpeningArea_Counts()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.ExternalDoor, externalDoorOpeningArea_M2: 2.1), "Living Room");

            Assert.Equal(2.1, partFPurgeVentilationData.ProvidedOpeningArea_M2().Value, tolerance);
            Assert.Equal(PartFComplianceStatus.Pass, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// Paragraph 1.28b allows purge to be delivered mechanically. It is judged against the four air
        /// changes per hour of paragraph 1.27 rather than against Table 1.4.
        /// </summary>
        [Fact]
        public void MechanicalPurgeAtOrAboveTheRequiredRate_Passes()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.MechanicalExtract, mechanicalPurgeCapacity_Lps: 120), "Living Room");

            Assert.Equal(PartFComplianceStatus.Pass, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>A mechanical purge below four air changes per hour fails.</summary>
        [Fact]
        public void MechanicalPurgeBelowTheRequiredRate_Fails()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.MechanicalExtract, mechanicalPurgeCapacity_Lps: 50), "Living Room");

            Assert.Equal(PartFComplianceStatus.Fail, partFPurgeVentilationData.ComplianceStatus);
        }

        /// <summary>
        /// Paragraph 1.27 requires the purge to be DIRECTLY to the outside. An internal habitable room
        /// cannot do that, and is sent to paragraphs 1.42 to 1.44 or to a mechanical system rather than
        /// being failed outright or passed.
        /// </summary>
        [Fact]
        public void InternalHabitableRoom_NeedsEngineeringReview()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Bathroom", "D01"), "Living Room");

            Assert.False(partFPurgeVentilationData.IsPurgeRouteDirectlyOutside);
            Assert.Equal(PartFComplianceStatus.EngineeringReviewRequired, partFPurgeVentilationData.ComplianceStatus);
            Assert.Contains("paragraphs 1.42 to 1.44", partFPurgeVentilationData.Diagnostic);
        }

        /// <summary>
        /// Paragraph 0.21: Part O may require a higher purge standard, and where it does the higher
        /// applies. It is reported separately rather than folded into the Part F figure.
        /// </summary>
        [Fact]
        public void PartOInteraction_IsReportedSeparately()
        {
            PartFPurgeVentilationData partFPurgeVentilationData = Purge(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.OpeningSashWindow, openableWindowArea_M2: 2.5), "Living Room");

            Assert.Contains("Approved Document O may require a higher purge ventilation standard", partFPurgeVentilationData.PartOInteractionNote);
            Assert.DoesNotContain("Part O", partFPurgeVentilationData.Diagnostic);
        }

        /// <summary>The purge inputs are engineering values and survive a recalculation.</summary>
        [Fact]
        public void PurgeInputs_SurviveARecalculation()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(Model()
                .PurgeInput("Living Room", PartFPurgeMethod.Openings, PartFPurgeOpeningType.OpeningSashWindow, openableWindowArea_M2: 2.5));

            PartFCalculator partFCalculator_Second = new(Analytical.Create.PartFData(Fixtures.GetPath("SAM_PartFSpaceRulesUKDwellingsMVHR.json")))
            {
                AdjacencyCluster = partFCalculator.AdjacencyCluster,
            };

            Assert.True(partFCalculator_Second.Calculate());

            PartFPurgeVentilationData partFPurgeVentilationData = Assert.Single(partFCalculator_Second.DwellingResults)
                .ComplianceResult.PurgeVentilation.Find(x => x.SpaceName == "Living Room");

            Assert.NotNull(partFPurgeVentilationData);
            Assert.Equal(2.5, partFPurgeVentilationData.OpenableWindowArea_M2.Value, tolerance);
            Assert.Equal(PartFPurgeOpeningType.OpeningSashWindow, partFPurgeVentilationData.OpeningType);
            Assert.Equal(PartFComplianceStatus.Pass, partFPurgeVentilationData.ComplianceStatus);
        }

        // ------------------------------------------------------------------
        // Overall status
        // ------------------------------------------------------------------

        /// <summary>
        /// The overall status is not a boolean and is never a majority verdict: one failed mandatory check
        /// fails the dwelling, however many others pass.
        /// </summary>
        [Fact]
        public void OneFailedMandatoryCheck_FailsTheDwelling()
        {
            PartFComplianceResult partFComplianceResult = new("Flat 1");

            partFComplianceResult.AddCheck(new PartFComplianceCheck("a", "x", "y") { Status = PartFComplianceStatus.Pass });
            partFComplianceResult.AddCheck(new PartFComplianceCheck("b", "x", "y") { Status = PartFComplianceStatus.Pass });
            partFComplianceResult.AddCheck(new PartFComplianceCheck("c", "x", "y") { Status = PartFComplianceStatus.Fail });

            Assert.Equal(PartFOverallStatus.Fail, partFComplianceResult.Resolve());
        }

        /// <summary>
        /// A failure outranks an unresolved check, an unresolved check outranks one needing review, and
        /// none of them can be hidden behind passes.
        /// </summary>
        [Theory]
        [InlineData(PartFComplianceStatus.Fail, PartFOverallStatus.Fail)]
        [InlineData(PartFComplianceStatus.EngineeringReviewRequired, PartFOverallStatus.EngineeringReviewRequired)]
        [InlineData(PartFComplianceStatus.CannotBeDetermined, PartFOverallStatus.CannotBeDetermined)]
        [InlineData(PartFComplianceStatus.NotAssessed, PartFOverallStatus.Partial)]
        [InlineData(PartFComplianceStatus.Pass, PartFOverallStatus.Pass)]
        [InlineData(PartFComplianceStatus.UserConfirmed, PartFOverallStatus.Pass)]
        [InlineData(PartFComplianceStatus.NotApplicable, PartFOverallStatus.Pass)]
        public void OverallStatus_TakesTheMostSevereMandatoryOutcome(PartFComplianceStatus partFComplianceStatus, PartFOverallStatus expected)
        {
            PartFComplianceResult partFComplianceResult = new("Flat 1");

            partFComplianceResult.AddCheck(new PartFComplianceCheck("a", "x", "y") { Status = PartFComplianceStatus.Pass });
            partFComplianceResult.AddCheck(new PartFComplianceCheck("b", "x", "y") { Status = partFComplianceStatus });

            Assert.Equal(expected, partFComplianceResult.Resolve());
        }

        /// <summary>A dwelling nobody assessed is reported as unassessed, not as passing.</summary>
        [Fact]
        public void NoChecks_IsNotAssessed()
        {
            Assert.Equal(PartFOverallStatus.NotAssessed, new PartFComplianceResult("Flat 1").Resolve());
        }

        /// <summary>
        /// A real dwelling with nothing confirmed cannot pass. Every requirement no analytical model
        /// contains - noise, maintenance access, controls, commissioning - is recorded as unresolved and
        /// holds the dwelling off a pass, because reporting silence as compliance would be worse than
        /// reporting nothing.
        /// </summary>
        [Fact]
        public void NothingConfirmed_CannotPass()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(Model());

            Assert.Empty(complianceResult.FailedChecks);
            Assert.NotEmpty(complianceResult.UnresolvedChecks);
            Assert.NotEqual(PartFOverallStatus.Pass, complianceResult.OverallStatus);
        }

        /// <summary>
        /// Every check that the model cannot decide names the paragraph it comes from, so an engineer can
        /// take the list to the Approved Document rather than guessing what is being asked.
        /// </summary>
        [Fact]
        public void EveryCheck_NamesItsSourceParagraph()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(Model());

            Assert.NotEmpty(complianceResult.Checks);

            Assert.All(complianceResult.Checks, x =>
            {
                Assert.False(string.IsNullOrWhiteSpace(x.SourceReference));
                Assert.False(string.IsNullOrWhiteSpace(x.Requirement));
                Assert.StartsWith("Approved Document F, Volume 1: Dwellings (2021 edition),", x.SourceReference);
            });
        }

        /// <summary>
        /// The non-calculable requirements are all present. They are the ones most easily lost, because
        /// nothing in the model prompts for them.
        /// </summary>
        [Theory]
        [InlineData("System designed and installed to minimise noise")]
        [InlineData("Reasonable access for maintenance")]
        [InlineData("Ventilation controls")]
        [InlineData("Installation of the ventilation system")]
        [InlineData("Extract terminals installed high in the room")]
        [InlineData("Background ventilators are not installed with mechanical ventilation with heat recovery")]
        [InlineData("Moist air from the wet rooms is not recirculated to the habitable rooms")]
        [InlineData("Outdoor air intake location")]
        [InlineData("Exhaust outlet location")]
        [InlineData("Operating and maintenance information issued to the building owner")]
        [InlineData("Home User Guide provided")]
        public void NonCalculableRequirement_IsRecordedAsAnOpenCheck(string name)
        {
            PartFComplianceCheck check = PartFAirflowNetworkTests.Check(PartFAirflowNetworkTests.Calculate(Model()), name);

            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, check.Status);
            Assert.True(check.IsMandatory);
        }

        // ------------------------------------------------------------------
        // Commissioning
        // ------------------------------------------------------------------

        /// <summary>Missing commissioning is unresolved, not failed: at design stage there is nothing to fail.</summary>
        [Fact]
        public void MissingCommissioning_IsUnresolvedRatherThanFailed()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(Model());

            Assert.Null(complianceResult.Commissioning);

            PartFComplianceCheck check = PartFAirflowNetworkTests.Check(complianceResult, "System commissioned and commissioning notice given");

            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, check.Status);
            Assert.Contains("expected at design stage", check.Evidence);
        }

        /// <summary>
        /// Appendix C paragraph C2: the measured rate for each fan must be equal to or greater than its
        /// design value. A shortfall is a failure, and the design value is never overwritten by it.
        /// </summary>
        [Fact]
        public void MeasuredRateBelowDesign_Fails()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(Model()
                .Zone("Flat 1", "Flats", true, "Living Room", "Kitchen", "Bathroom")
                .Commissioning("Flat 1", new PartFCommissioningData("Flat 1")
                {
                    CommissioningNoticeGiven = true,
                    AirFlowRateNoticeGiven = true,
                    MeasuredContinuousExtractTotal_Lps = 10,
                }), "Flats");

            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Dwelling(partFCalculator, "Flat 1").ComplianceResult;

            //Record a shortfall on a terminal and re-resolve the comparison.
            PartFVentilationTerminalRequirement terminal = complianceResult.Terminals.Find(x => x.TerminalRole == PartFTerminalRole.GeneralExtract);
            double design = terminal.ContinuousDesignFlowRate_Lps.Value;
            terminal.MeasuredContinuousFlowRate_Lps = design - 2;

            PartFCheckBuilder.Build(PartFAirflowNetworkTests.Dwelling(partFCalculator, "Flat 1"), Analytical.Create.PartFData(Fixtures.GetPath("SAM_PartFSpaceRulesUKDwellingsMVHR.json")));

            PartFComplianceCheck check = complianceResult.Checks.FindLast(x => x.Name == "Measured air flow rates meet the design air flow rates");

            Assert.Equal(PartFComplianceStatus.Fail, check.Status);

            //The design value is untouched.
            Assert.Equal(design, terminal.ContinuousDesignFlowRate_Lps.Value, tolerance);
        }

        /// <summary>
        /// A person's confirmation can resolve a check the model could not decide - that is what the
        /// recorded checklist is for.
        /// </summary>
        [Fact]
        public void UserConfirmation_ResolvesAnUndecidableCheck()
        {
            PartFCommissioningData partFCommissioningData = new("Flat 1");

            partFCommissioningData.InstallationChecks.Add(new PartFComplianceCheck("System designed and installed to minimise noise", "x", "y")
            {
                Status = PartFComplianceStatus.UserConfirmed,
                Evidence = "Acoustic assessment ACO-01 rev B.",
                ResponsiblePerson = "A Engineer",
                Date = "2026-08-06",
            });

            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(Model()
                .Zone("Flat 1", "Flats", true, "Living Room", "Kitchen", "Bathroom")
                .Commissioning("Flat 1", partFCommissioningData), "Flats");

            PartFComplianceCheck check = PartFAirflowNetworkTests.Check(
                PartFAirflowNetworkTests.Dwelling(partFCalculator, "Flat 1").ComplianceResult,
                "System designed and installed to minimise noise");

            Assert.Equal(PartFComplianceStatus.UserConfirmed, check.Status);
            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, check.CalculatedStatus);
            Assert.Equal("A Engineer", check.ConfirmedBy);
            Assert.Contains("ACO-01 rev B", check.UserEvidence);
            Assert.True(check.IsResolved);

            //The calculated evidence is not overwritten by the recorded one; both are kept.
            Assert.DoesNotContain("ACO-01 rev B", check.Evidence);
        }

        /// <summary>
        /// A confirmation cannot overturn a calculated failure. A failure here is arithmetic against the
        /// Approved Document, and a checkbox does not change arithmetic.
        /// <para>
        /// The confirmation is not discarded - it is recorded, and the check is redirected to engineering
        /// review because no alternative compliance method was offered with it. The calculated failure is
        /// retained on the check and keeps the whole dwelling at Fail.
        /// </para>
        /// </summary>
        [Fact]
        public void UserConfirmation_CannotOverturnACalculatedFailure()
        {
            PartFCommissioningData partFCommissioningData = new("Flat 1");

            partFCommissioningData.InstallationChecks.Add(new PartFComplianceCheck("Local kitchen extract from the room containing the cooking function", "x", "y")
            {
                Status = PartFComplianceStatus.UserConfirmed,
            });

            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .LocalExtractMethod("Studio", PartFExtractMethod.RecirculatingCookerHood)
                .Zone("Flat 1", "Flats", true, "Studio", "Bathroom")
                .Commissioning("Flat 1", partFCommissioningData), "Flats");

            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Dwelling(partFCalculator, "Flat 1").ComplianceResult;
            PartFComplianceCheck check = PartFAirflowNetworkTests.Check(complianceResult, "Local kitchen extract from the room containing the cooking function");

            Assert.Equal(PartFComplianceStatus.Fail, check.CalculatedStatus);
            Assert.NotEqual(PartFComplianceStatus.UserConfirmed, check.Status);
            Assert.Equal(PartFComplianceStatus.EngineeringReviewRequired, check.Status);
            Assert.False(check.IsResolved);

            Assert.Equal(PartFOverallStatus.Fail, complianceResult.OverallStatus);

            Assert.Contains(complianceResult.Warnings, x => x.Contains("cannot be turned into a pass by changing its status"));
        }

        /// <summary>
        /// Recording an alternative compliance method against a calculated failure moves the check to
        /// "alternative solution pending approval" rather than to a pass, and the calculated failure is
        /// still on the check afterwards.
        /// </summary>
        [Fact]
        public void AlternativeComplianceMethod_DoesNotEraseTheCalculatedFailure()
        {
            PartFCommissioningData partFCommissioningData = new("Flat 1");

            partFCommissioningData.InstallationChecks.Add(new PartFComplianceCheck("Local kitchen extract from the room containing the cooking function", "x", "y")
            {
                Status = PartFComplianceStatus.UserConfirmed,
                AlternativeComplianceMethod = "Ducted external extract to be installed under variation VAR-014, submitted to building control.",
                OverrideReason = "The recirculating hood shown is a placeholder from the architectural model.",
                ConfirmedBy = "A Engineer",
                ConfirmationDate = "2026-08-06",
            });

            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .LocalExtractMethod("Studio", PartFExtractMethod.RecirculatingCookerHood)
                .Zone("Flat 1", "Flats", true, "Studio", "Bathroom")
                .Commissioning("Flat 1", partFCommissioningData), "Flats");

            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Dwelling(partFCalculator, "Flat 1").ComplianceResult;
            PartFComplianceCheck check = PartFAirflowNetworkTests.Check(complianceResult, "Local kitchen extract from the room containing the cooking function");

            Assert.Equal(PartFComplianceStatus.Fail, check.CalculatedStatus);
            Assert.Equal(PartFComplianceStatus.AlternativeSolutionPendingApproval, check.Status);
            Assert.False(check.IsResolved);
            Assert.True(check.IsUserResolved);
            Assert.Contains("VAR-014", check.AlternativeComplianceMethod);
            Assert.Contains("calculated failure is retained", check.ResolutionSummary());

            //The dwelling is still a Fail: the recirculating hood also fails the paragraph 1.17 extract
            //provision check, and an alternative recorded against one check rescues only that check.
            Assert.Equal(PartFOverallStatus.Fail, complianceResult.OverallStatus);
        }

        /// <summary>
        /// Where an alternative compliance method is the only thing standing between the dwelling and a
        /// pass, the overall outcome is reported as its own state - below a pass, and distinct from an
        /// unaddressed failure - rather than being collapsed into either.
        /// </summary>
        [Fact]
        public void AlternativeSolution_IsItsOwnOverallState()
        {
            PartFComplianceResult complianceResult = new("Flat 1");

            complianceResult.AddCheck(new PartFComplianceCheck("Passed", "x", "y")
            {
                CalculatedStatus = PartFComplianceStatus.Pass,
                Status = PartFComplianceStatus.Pass,
            });

            complianceResult.AddCheck(new PartFComplianceCheck("Alternative", "x", "y")
            {
                CalculatedStatus = PartFComplianceStatus.Fail,
                Status = PartFComplianceStatus.AlternativeSolutionPendingApproval,
                AlternativeComplianceMethod = "Recorded.",
            });

            Assert.Equal(PartFOverallStatus.AlternativeSolutionPendingApproval, complianceResult.Resolve());
        }

        /// <summary>
        /// The guard holds even when a status is assigned straight onto a check rather than through
        /// <see cref="PartFComplianceCheck.ApplyUserResolution"/>: a calculated failure that has been
        /// relabelled as a pass is not resolved, and still fails the dwelling.
        /// </summary>
        [Fact]
        public void CalculatedFailure_RelabelledDirectly_StillFailsTheDwelling()
        {
            PartFComplianceResult complianceResult = new("Flat 1");

            complianceResult.AddCheck(new PartFComplianceCheck("Calculated failure", "x", "y")
            {
                CalculatedStatus = PartFComplianceStatus.Fail,
                Status = PartFComplianceStatus.Pass,
            });

            PartFComplianceCheck check = Assert.Single(complianceResult.Checks);

            Assert.True(check.IsCalculatedFailureOverstated);
            Assert.False(check.IsResolved);
            Assert.Equal(PartFOverallStatus.Fail, complianceResult.Resolve());
        }

        // ------------------------------------------------------------------
        // Serialisation and migration
        // ------------------------------------------------------------------

        /// <summary>
        /// A multi-terminal space keeps both terminals through a round trip to file and back. The
        /// secondary terminal is never dropped on the way.
        /// </summary>
        [Fact]
        public void MultiTerminalSpace_RoundTripsBothTerminals()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01"));

            PartFSpaceData partFSpaceData = partFCalculator.AdjacencyCluster.GetSpaces()
                .Find(x => x.Name == "Studio")
                .GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);

            Assert.Equal(2, partFSpaceData.Terminals.Count);

            PartFSpaceData result = new(partFSpaceData.ToJsonObject());

            Assert.Equal(2, result.Terminals.Count);
            Assert.Equal(30, result.ContinuousSupplyFlowRate_Lps.Value, tolerance);
            Assert.Equal(22, result.LocalKitchenExtractFlowRate_Lps.Value, tolerance);
            Assert.Equal(6.6, result.SetbackExtractFlowRate_Lps.Value, tolerance);

            //The legacy scalar keeps its meaning: the primary terminal's continuous design rate.
            Assert.Equal(30, result.ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(30, result.CalculatedFlowRate_Lps.Value, tolerance);
        }

        /// <summary>
        /// A model serialised before terminal-level sizing carries only the scalar rate. It reads back
        /// with that rate intact and an EMPTY terminal collection: the scalar cannot say whether a studio's
        /// flow was supply with a separate local kitchen extract or supply alone, and inventing a terminal
        /// the original calculation never established would be worse than having none.
        /// </summary>
        [Fact]
        public void LegacyModelWithoutTerminals_KeepsItsRateAndGainsNoInventedTerminal()
        {
            System.Text.Json.Nodes.JsonObject jsonObject = new()
            {
                ["_type"] = "SAM.Analytical.PartFSpaceData,SAM.Analytical",
                ["Name"] = "Studio",
                ["CalculatedFlowRate_Lps"] = 33.0,
                ["PartFVentilationType"] = "supply",
                ["PartFType"] = "Habitable",
                ["IsCookingSpace"] = true,
                ["IsTerminalSpace"] = true,
            };

            PartFSpaceData result = new(jsonObject);

            Assert.Equal(33, result.ContinuousDesignFlowRate_Lps.Value, tolerance);
            Assert.Equal(33, result.CalculatedFlowRate_Lps.Value, tolerance);
            Assert.Equal(PartFVentilationType.supply, result.PartFVentilationType);

            Assert.Empty(result.Terminals);
            Assert.Null(result.PrimaryTerminal());
            Assert.Null(result.ContinuousSupplyFlowRate_Lps);
            Assert.Null(result.LocalKitchenExtractFlowRate_Lps);
        }

        /// <summary>
        /// A model produced now is still readable by an older SAM build, which only knows
        /// CalculatedFlowRate_Lps.
        /// </summary>
        [Fact]
        public void CurrentModel_IsStillReadableByAnOlderBuild()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01"));

            System.Text.Json.Nodes.JsonObject jsonObject = partFCalculator.AdjacencyCluster.GetSpaces()
                .Find(x => x.Name == "Studio")
                .GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData)
                .ToJsonObject();

            Assert.True(jsonObject.ContainsKey("CalculatedFlowRate_Lps"));
            Assert.Equal(30, jsonObject["CalculatedFlowRate_Lps"].GetValue<double>(), tolerance);
        }

        /// <summary>The commissioning record, including its recorded checks, round trips through file.</summary>
        [Fact]
        public void CommissioningData_RoundTripsThroughJson()
        {
            PartFCommissioningData partFCommissioningData = new("Flat 1")
            {
                DwellingName = "Flat 1",
                MeasurementEquipment = "Hood XYZ 12345",
                CalibrationDate = "2026-01-15",
                CommissioningDate = "2026-08-01",
                CommissioningEngineer = "A Engineer",
                MeasuredContinuousSupplyTotal_Lps = 31,
                MeasuredContinuousExtractTotal_Lps = 30.5,
                CommissioningNoticeGiven = true,
                OperatingAndMaintenanceInformationIssued = true,
            };

            partFCommissioningData.InstallationChecks.Add(new PartFComplianceCheck("Ventilation controls", "x", "y")
            {
                Status = PartFComplianceStatus.UserConfirmed,
                ResponsiblePerson = "A Engineer",
            });

            PartFCommissioningData result = new(partFCommissioningData.ToJsonObject());

            Assert.Equal("Hood XYZ 12345", result.MeasurementEquipment);
            Assert.Equal("2026-01-15", result.CalibrationDate);
            Assert.Equal(31, result.MeasuredContinuousSupplyTotal_Lps.Value, tolerance);
            Assert.True(result.CommissioningNoticeGiven);
            Assert.False(result.AirFlowRateNoticeGiven);
            Assert.True(result.HasMeasuredValues);

            PartFComplianceCheck check = Assert.Single(result.InstallationChecks);

            Assert.Equal("Ventilation controls", check.Name);
            Assert.Equal(PartFComplianceStatus.UserConfirmed, check.Status);
            Assert.Equal("A Engineer", check.ResponsiblePerson);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// A small complete dwelling: one habitable room with an external window, a separate kitchen so
        /// the paragraph 1.17a cooking requirement is satisfied, and a bathroom, all connected.
        /// </summary>
        private static PartFModel Model()
        {
            return new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Kitchen", 12, 30)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Kitchen", "D01")
                .Partition("Living Room", "Bathroom", "D02")
                .ExternalWall("Living Room");
        }

        private static PartFPurgeVentilationData Purge(PartFModel partFModel, string name_Space)
        {
            PartFPurgeVentilationData result = PartFAirflowNetworkTests.Calculate(partFModel).PurgeVentilation.Find(x => x.SpaceName == name_Space);

            Assert.NotNull(result);

            return result;
        }
    }
}
