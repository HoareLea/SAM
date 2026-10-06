// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical;
using System.Collections.Generic;
using Xunit;

namespace SAM.Tests
{
    /// <summary>
    /// <b>What an engineer reads on a ventilation unit product, and what stays internal.</b>
    /// <para>
    /// <see cref="VentilationUnitCapacityDescriptor.Rank"/> is a catalogue tie-breaker. It is not an
    /// airflow, not an efficiency, not a performance score and not an engineering output - and a manual
    /// product picker that prints "rank 10" invites an engineer to read a preference number as a rating,
    /// or to compare two products by it. So the picker reads
    /// <see cref="VentilationUnitCapacityDescriptor.Label"/> and the diagnostics keep
    /// <see cref="VentilationUnitCapacityDescriptor.ToString"/>.
    /// </para>
    /// <para>
    /// <b>Hiding it must change nothing about what it does.</b> The second half of this file re-pins the
    /// deterministic ordering rank exists for, so that a presentation change cannot have quietly become a
    /// selection change.
    /// </para>
    /// </summary>
    public class PartOEquipmentPresentationTests
    {
        // =================================================================================================
        // A. The engineer-facing label
        // =================================================================================================

        /// <summary>
        /// The label names the product and states both maximum airflows - the two things a person choosing
        /// a box by hand needs - and says nothing about rank.
        /// </summary>
        [Fact]
        public void TheLabel_NamesTheProductAndItsCapacities()
        {
            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor = new(MRXBOXReference(), 150, 150, 10);

            string label = ventilationUnitCapacityDescriptor.Label;

            Assert.Contains("Nuaire", label);
            Assert.Contains("MRXBOXAB-ECO5-AECV", label);
            Assert.Contains("MR-ECO-COOL-V", label);

            //Capacity is IN the label: choosing a unit by hand is choosing a capacity.
            Assert.Contains("150", label);
            Assert.Contains("l/s", label);
        }

        /// <summary>
        /// <b>And it never mentions rank</b>, in any casing, for any rank value - including one that
        /// happens to read like an airflow.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(20)]
        [InlineData(150)]
        [InlineData(1000000)]
        public void TheLabel_NeverMentionsRank(int rank)
        {
            string label = new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, rank).Label;

            Assert.DoesNotContain("rank", label, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Two products differing only by rank read identically to an engineer - which is the whole
        /// intent. Rank is not a distinguishing engineering property and must not be presented as one.
        /// </summary>
        [Fact]
        public void TwoProductsDifferingOnlyByRank_ReadIdentically()
        {
            Assert.Equal(
                new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 20).Label,
                new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 99).Label);
        }

        /// <summary>
        /// <see cref="VentilationUnitCapacityDescriptor.ToString"/> is unchanged and still carries the
        /// rank. It is the diagnostic form: the catalogue-conflict and same-rank-ambiguity refusals embed
        /// it precisely because rank is the subject of what they are refusing, and a log that hid it would
        /// leave an engineer unable to see why two products tied.
        /// </summary>
        [Fact]
        public void ToString_IsStillTheDiagnosticFormAndKeepsTheRank()
        {
            string description = new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 20).ToString();

            Assert.Contains("rank 20", description);
            Assert.Contains("190", description);
        }

        /// <summary>A descriptor naming nothing still produces a label rather than throwing.</summary>
        [Fact]
        public void ADescriptorNamingNothing_StillLabelsSomething()
        {
            Assert.False(string.IsNullOrWhiteSpace(new VentilationUnitCapacityDescriptor(null, 150, 150, 10).Label));
        }

        // =================================================================================================
        // B. Rank still does exactly what it did
        // =================================================================================================

        /// <summary>
        /// The catalogue's preference still breaks a tie between two equally sized products, and still
        /// prefers the LOWER rank. Hiding rank from a label changed no ordering.
        /// </summary>
        [Theory]
        [InlineData(10, 20, "Preferred")]
        [InlineData(20, 10, "Other")]
        public void Rank_StillBreaksATieBetweenEquallySizedProducts(int rank_Preferred, int rank_Other, string model_Expected)
        {
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Preferred", null), 150, 150, rank_Preferred),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Other", null), 150, 150, rank_Other),
            ];

            VentilationUnitSelection ventilationUnitSelection = ventilationUnitCapacityDescriptors.SelectSmallestCapableVentilationUnit(100, 100);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal(model_Expected, ventilationUnitSelection.VentilationUnitReference.Model);
        }

        /// <summary>
        /// And the ordering is independent of the order the descriptors arrived in, so a catalogue read
        /// from a directory still cannot let the file system choose a dwelling's plant.
        /// </summary>
        [Fact]
        public void Rank_StillOrdersIndependentlyOfArrivalOrder()
        {
            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_Preferred = new(new VentilationUnitReference("Test Fixture", "Preferred", null), 150, 150, 10);
            VentilationUnitCapacityDescriptor ventilationUnitCapacityDescriptor_Other = new(new VentilationUnitReference("Test Fixture", "Other", null), 150, 150, 20);

            Assert.Equal(
                "Preferred",
                new List<VentilationUnitCapacityDescriptor> { ventilationUnitCapacityDescriptor_Preferred, ventilationUnitCapacityDescriptor_Other }.SelectSmallestCapableVentilationUnit(100, 100).VentilationUnitReference.Model);

            Assert.Equal(
                "Preferred",
                new List<VentilationUnitCapacityDescriptor> { ventilationUnitCapacityDescriptor_Other, ventilationUnitCapacityDescriptor_Preferred }.SelectSmallestCapableVentilationUnit(100, 100).VentilationUnitReference.Model);
        }

        /// <summary>
        /// Two different products of the same size AND the same rank still refuse rather than picking one,
        /// and the refusal still says so in words. That refusal is the one place rank legitimately reaches
        /// an engineer, and it is untouched.
        /// </summary>
        [Fact]
        public void TwoProductsOfTheSameSizeAndRank_StillRefuseAndStillSayWhy()
        {
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "One", null), 150, 150, 10),
                new VentilationUnitCapacityDescriptor(new VentilationUnitReference("Test Fixture", "Two", null), 150, 150, 10),
            ];

            VentilationUnitSelection ventilationUnitSelection = ventilationUnitCapacityDescriptors.SelectSmallestCapableVentilationUnit(100, 100);

            Assert.False(ventilationUnitSelection.IsSelected);
            Assert.Contains("ranked 10", ventilationUnitSelection.Reason);
        }

        /// <summary>
        /// Rank still never overrides compliance: a lower-ranked product that cannot move the duty is not
        /// a candidate at all, so the higher-ranked one that can is chosen.
        /// </summary>
        [Fact]
        public void Rank_StillNeverOverridesCompliance()
        {
            List<VentilationUnitCapacityDescriptor> ventilationUnitCapacityDescriptors =
            [
                new VentilationUnitCapacityDescriptor(MRXBOXReference(), 150, 150, 10),
                new VentilationUnitCapacityDescriptor(XBC15Reference(), 190, 190, 20),
            ];

            VentilationUnitSelection ventilationUnitSelection = ventilationUnitCapacityDescriptors.SelectSmallestCapableVentilationUnit(160, 160);

            Assert.True(ventilationUnitSelection.IsSelected);
            Assert.Equal("XBC15", ventilationUnitSelection.VentilationUnitReference.Model);
        }

        // =================================================================================================
        // Fixtures
        // =================================================================================================

        private static VentilationUnitReference MRXBOXReference()
        {
            return new VentilationUnitReference("Nuaire", "MRXBOXAB-ECO5-AECV", "MR-ECO-COOL-V");
        }

        private static VentilationUnitReference XBC15Reference()
        {
            return new VentilationUnitReference("Nuaire", "XBC15", null);
        }
    }
}
