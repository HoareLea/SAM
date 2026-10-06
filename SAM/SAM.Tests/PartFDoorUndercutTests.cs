// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using SAM.Analytical.Enums;
using SAM.Tests.Helpers;
using System.Text.Json.Nodes;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// Tests for the internal door transfer air requirement of Approved Document F, Volume 1: Dwellings
    /// (2021 edition, for use in England) paragraph 1.25 (page 10).
    /// </summary>
    /// <remarks>
    /// "Internal doors should allow air to flow through the dwelling by providing a minimum free area
    /// equivalent to a 10mm undercut in a 760mm wide door. Doors should be undercut to achieve one of the
    /// following. a. If the floor finish is fitted: 10mm above the floor finish. b. If the floor finish is
    /// not fitted: 20mm above the floor surface."
    /// <para>
    /// The free area is the requirement; 10mm x 760mm = 7,600mm2. The two undercut heights are the datum
    /// it is measured from. An analytical model does not represent the gap under a door leaf, so the
    /// provided value is always an engineering input, and its absence is never treated as compliance.
    /// </para>
    /// </remarks>
    public class PartFDoorUndercutTests
    {
        private const double tolerance = 1e-6;

        // ------------------------------------------------------------------
        // The requirement
        // ------------------------------------------------------------------

        /// <summary>
        /// The reference free area is 7,600mm2, derived as the 10mm undercut across the 760mm reference
        /// door width the paragraph names.
        /// </summary>
        [Fact]
        public void ReferenceFreeArea_IsSevenThousandSixHundredSquareMillimetres()
        {
            Assert.Equal(760, PartFDoorTransferData.ReferenceDoorWidth_mm, tolerance);
            Assert.Equal(10, PartFDoorTransferData.ReferenceUndercutHeight_mm, tolerance);
            Assert.Equal(20, PartFDoorTransferData.UndercutHeightBeforeFloorFinish_mm, tolerance);
            Assert.Equal(7600, PartFDoorTransferData.NominalEquivalentFreeArea_mm2, tolerance);
        }

        /// <summary>Every internal door carries the requirement, whatever the solver puts through it.</summary>
        [Fact]
        public void EveryInternalDoor_CarriesTheRequirement()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(new PartFModel()
                .Space("Living Room", 40, 100)
                .Space("Bedroom 1", 20, 50)
                .Space("Hall", 10, 25)
                .Space("Bathroom", 8, 20)
                .Partition("Living Room", "Hall", "D01")
                .Partition("Bedroom 1", "Hall", "D02")
                .Partition("Hall", "Bathroom", "D03"));

            Assert.Equal(3, complianceResult.TransferPaths.Count);

            Assert.All(complianceResult.TransferPaths, x =>
            {
                Assert.True(x.RequiresTransferAirPath);
                Assert.True(x.IsInternalDwellingDoor);
                Assert.Equal(7600, x.MinimumRequiredFreeArea_mm2.Value, tolerance);
            });
        }

        // ------------------------------------------------------------------
        // Assessing what was provided
        // ------------------------------------------------------------------

        /// <summary>An unrecorded undercut is never a pass. Absence of evidence is not compliance.</summary>
        [Fact]
        public void UnknownUndercut_CannotBeDetermined()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01");

            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, partFDoorTransferData.ComplianceStatus);
            Assert.False(partFDoorTransferData.IsCompliant);
            Assert.Contains("7600 mm2", partFDoorTransferData.Diagnostic);
            Assert.Contains("10 mm undercut in a 760 mm wide door", partFDoorTransferData.Diagnostic);
        }

        /// <summary>
        /// A 10mm undercut in a 900mm door gives 9,000mm2, above the 7,600mm2 required, and meets the
        /// paragraph 1.25a datum of 10mm above a fitted floor finish.
        /// </summary>
        [Fact]
        public void CompliantTenMillimetreUndercutWithFloorFinishFitted_Passes()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", providedUndercutHeight_mm: 10, isFloorFinishFitted: true);

            Assert.Equal(9000, partFDoorTransferData.EffectiveProvidedFreeArea_mm2().Value, tolerance);
            Assert.Equal(PartFComplianceStatus.Pass, partFDoorTransferData.ComplianceStatus);
            Assert.True(partFDoorTransferData.IsCompliant);
        }

        /// <summary>
        /// Paragraph 1.25b: where the floor finish is not fitted, the undercut is measured 20mm above the
        /// floor surface. A 20mm undercut satisfies that datum and the free area.
        /// </summary>
        [Fact]
        public void TwentyMillimetreUndercutBeforeFloorFinish_Passes()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", providedUndercutHeight_mm: 20, isFloorFinishFitted: false);

            Assert.Equal(PartFComplianceStatus.Pass, partFDoorTransferData.ComplianceStatus);
        }

        /// <summary>
        /// A 10mm undercut is below the 20mm paragraph 1.25b requires before the floor finish is fitted,
        /// even though the free area it gives in a 900mm door is above 7,600mm2. Both conditions apply.
        /// </summary>
        [Fact]
        public void TenMillimetreUndercutBeforeFloorFinish_Fails()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", providedUndercutHeight_mm: 10, isFloorFinishFitted: false);

            Assert.Equal(PartFComplianceStatus.Fail, partFDoorTransferData.ComplianceStatus);
            Assert.Contains("below the 20 mm required", partFDoorTransferData.Diagnostic);
        }

        /// <summary>
        /// A shallow undercut in a narrow door falls below the free area, whatever the datum: 5mm across a
        /// 700mm door is 3,500mm2 against the 7,600mm2 required.
        /// </summary>
        [Fact]
        public void InsufficientUndercut_Fails()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", providedUndercutHeight_mm: 5, isFloorFinishFitted: true, doorWidth_M: 0.7);

            Assert.Equal(3500, partFDoorTransferData.EffectiveProvidedFreeArea_mm2().Value, tolerance);
            Assert.Equal(PartFComplianceStatus.Fail, partFDoorTransferData.ComplianceStatus);
            Assert.False(partFDoorTransferData.IsCompliant);
        }

        /// <summary>
        /// A transfer grille of at least the equivalent free area serves the same purpose as an undercut.
        /// Paragraph 1.25 sets a free area; the undercut is the arrangement it describes, not the only one.
        /// </summary>
        [Fact]
        public void TransferGrilleOfEquivalentArea_Passes()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", partFTransferDeviceType: PartFTransferDeviceType.TransferGrille, providedFreeArea_mm2: 8000);

            Assert.Equal(PartFComplianceStatus.Pass, partFDoorTransferData.ComplianceStatus);
            Assert.Equal(PartFTransferDeviceType.TransferGrille, partFDoorTransferData.TransferDeviceType);
        }

        /// <summary>A transfer grille below the equivalent free area fails, exactly as a shallow undercut does.</summary>
        [Fact]
        public void TransferGrilleBelowEquivalentArea_Fails()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", partFTransferDeviceType: PartFTransferDeviceType.TransferGrille, providedFreeArea_mm2: 5000);

            Assert.Equal(PartFComplianceStatus.Fail, partFDoorTransferData.ComplianceStatus);
        }

        /// <summary>
        /// A permanent opening between the rooms is a transfer path in its own right (Appendix A,
        /// page 37: an opening with no means of closing it).
        /// </summary>
        [Theory]
        [InlineData(PartFTransferDeviceType.PermanentOpening)]
        [InlineData(PartFTransferDeviceType.OpenPassage)]
        public void PermanentOpeningOfSufficientArea_Passes(PartFTransferDeviceType partFTransferDeviceType)
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", partFTransferDeviceType: partFTransferDeviceType, providedFreeArea_mm2: 200000);

            Assert.Equal(PartFComplianceStatus.Pass, partFDoorTransferData.ComplianceStatus);
        }

        /// <summary>
        /// Two spaces adjacent through a partition with no modelled door is reported as an unrepresented
        /// opening, not as an absent requirement and certainly not as a pass.
        /// </summary>
        [Fact]
        public void AdjacencyWithNoModelledDoor_IsReportedAsUnrepresented()
        {
            PartFComplianceResult complianceResult = PartFAirflowNetworkTests.Calculate(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom"));

            PartFDoorTransferData partFDoorTransferData = Assert.Single(complianceResult.TransferPaths);

            Assert.False(partFDoorTransferData.IsDoorRepresented);
            Assert.Equal(PartFComplianceStatus.CannotBeDetermined, partFDoorTransferData.ComplianceStatus);
            Assert.Contains("no door or other transfer opening is modelled", partFDoorTransferData.Diagnostic);
        }

        /// <summary>
        /// The clear width is read from the door aperture geometry, so an undercut entered on its own
        /// still produces an assessable free area.
        /// </summary>
        [Fact]
        public void ClearDoorWidth_IsReadFromTheApertureGeometry()
        {
            PartFDoorTransferData partFDoorTransferData = Door("D01", doorWidth_M: 0.826);

            Assert.Equal(826, partFDoorTransferData.ClearDoorWidth_mm.Value, 1e-3);
        }

        // ------------------------------------------------------------------
        // Engineering inputs survive a recalculation
        // ------------------------------------------------------------------

        /// <summary>
        /// The provided undercut is something only a person can supply, so recalculating the dwelling must
        /// not discard it. Everything derived is rewritten; the inputs are carried forward.
        /// </summary>
        [Fact]
        public void EngineeringInputs_SurviveARecalculation()
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01")
                .DoorInput("D01", providedUndercutHeight_mm: 12, isFloorFinishFitted: true);

            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(partFModel);

            //Recalculate from the model the first run produced, which is what SAM_UI does on every edit.
            PartFCalculator partFCalculator_Second = new(Analytical.Create.PartFData(Fixtures.GetPath("SAM_PartFSpaceRulesUKDwellingsMVHR.json")))
            {
                AdjacencyCluster = partFCalculator.AdjacencyCluster,
            };

            Assert.True(partFCalculator_Second.Calculate());

            PartFDoorTransferData partFDoorTransferData = Assert.Single(Assert.Single(partFCalculator_Second.DwellingResults).ComplianceResult.TransferPaths);

            Assert.Equal(12, partFDoorTransferData.ProvidedUndercutHeight_mm.Value, tolerance);
            Assert.True(partFDoorTransferData.IsFloorFinishFitted);
            Assert.Equal(PartFComplianceStatus.Pass, partFDoorTransferData.ComplianceStatus);

            //And the derived transfer flow was recalculated rather than carried over.
            Assert.Equal(8, partFDoorTransferData.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
        }

        /// <summary>The record written to the model is readable back from the door aperture.</summary>
        [Fact]
        public void TransferData_IsWrittenOntoTheDoorAperture()
        {
            PartFCalculator partFCalculator = PartFAirflowNetworkTests.Calculator(new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", "D01"));

            //Panel.Apertures hands out clones, so this reads through the shared helper - which is exactly
            //the path the write took.
            Assert.Single(partFCalculator.AdjacencyCluster.GetPartFDoorTransferData());
        }

        // ------------------------------------------------------------------
        // Serialisation
        // ------------------------------------------------------------------

        /// <summary>Every value on the record survives a round trip to file and back.</summary>
        [Fact]
        public void TransferData_RoundTripsThroughJson()
        {
            PartFDoorTransferData partFDoorTransferData = new("D01")
            {
                UpstreamSpaceName = "Studio",
                DownstreamSpaceName = "Bathroom",
                DwellingName = "Flat 1",
                RequiresTransferAirPath = true,
                IsInternalDwellingDoor = true,
                IsDoorRepresented = true,
                ContinuousDesignTransferFlowRate_Lps = 8,
                HighTransferFlowRate_Lps = 8,
                SetbackTransferFlowRate_Lps = 2.4,
                MinimumRequiredFreeArea_mm2 = 7600,
                RequiredUndercutHeightFinished_mm = 10,
                RequiredUndercutHeightBeforeFloorFinish_mm = 20,
                ProvidedUndercutHeight_mm = 12,
                ClearDoorWidth_mm = 900,
                IsFloorFinishFitted = true,
                TransferDeviceType = PartFTransferDeviceType.DoorUndercut,
                TransferFlowRateOverride_Lps = 7.5,
                RouteStatus = PartFTransferRouteStatus.UserOverridden,
                ComplianceStatus = PartFComplianceStatus.Pass,
                SourceReference = "paragraph 1.25",
                CalculationSource = "override",
                Diagnostic = "fine",
            };

            PartFDoorTransferData result = new(partFDoorTransferData.ToJsonObject());

            Assert.Equal("D01", result.Name);
            Assert.Equal("Studio", result.UpstreamSpaceName);
            Assert.Equal("Bathroom", result.DownstreamSpaceName);
            Assert.Equal(8, result.ContinuousDesignTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(2.4, result.SetbackTransferFlowRate_Lps.Value, tolerance);
            Assert.Equal(7600, result.MinimumRequiredFreeArea_mm2.Value, tolerance);
            Assert.Equal(12, result.ProvidedUndercutHeight_mm.Value, tolerance);
            Assert.Equal(900, result.ClearDoorWidth_mm.Value, tolerance);
            Assert.True(result.IsFloorFinishFitted);
            Assert.Equal(PartFTransferDeviceType.DoorUndercut, result.TransferDeviceType);
            Assert.Equal(7.5, result.TransferFlowRateOverride_Lps.Value, tolerance);
            Assert.Equal(PartFTransferRouteStatus.UserOverridden, result.RouteStatus);
            Assert.Equal(PartFComplianceStatus.Pass, result.ComplianceStatus);
        }

        /// <summary>
        /// The floor finish state is deliberately tri-state. "Not recorded" selects neither the 10mm
        /// paragraph 1.25a datum nor the 20mm paragraph 1.25b one, and must not come back as "not fitted".
        /// </summary>
        [Fact]
        public void UnknownFloorFinishState_SurvivesAsUnknown()
        {
            PartFDoorTransferData partFDoorTransferData = new("D01");

            Assert.Null(partFDoorTransferData.IsFloorFinishFitted);

            JsonObject jsonObject = partFDoorTransferData.ToJsonObject();

            Assert.False(jsonObject.ContainsKey("IsFloorFinishFitted"));
            Assert.Null(new PartFDoorTransferData(jsonObject).IsFloorFinishFitted);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static PartFDoorTransferData Door(
            string name_Door,
            PartFTransferDeviceType partFTransferDeviceType = PartFTransferDeviceType.DoorUndercut,
            double? providedUndercutHeight_mm = null,
            double? providedFreeArea_mm2 = null,
            bool? isFloorFinishFitted = null,
            double doorWidth_M = 0.9)
        {
            PartFModel partFModel = new PartFModel()
                .Space("Studio", 75, 300)
                .Space("Bathroom", 25, 100)
                .Partition("Studio", "Bathroom", name_Door, doorWidth_M);

            if (providedUndercutHeight_mm is not null || providedFreeArea_mm2 is not null || isFloorFinishFitted is not null)
            {
                partFModel.DoorInput(name_Door, partFTransferDeviceType, providedUndercutHeight_mm, providedFreeArea_mm2, isFloorFinishFitted);
            }

            return Assert.Single(PartFAirflowNetworkTests.Calculate(partFModel).TransferPaths);
        }
    }
}
