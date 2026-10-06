// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// One project test product as a <see cref="VentilationUnitTemplate"/>, so that everything
        /// downstream of a catalogue read treats it exactly as it treats a manufacturer entry.
        ///
        /// <para><b>Why go through a template at all</b></para>
        /// <para>
        /// Because the mapping from "a product" to "a thing a selection can use" already exists, once, in
        /// <see cref="CapacityDescriptor(VentilationUnitTemplate)"/>, and that is the whole seam the
        /// selection kernel sees. Building a descriptor here instead would be a second mapping, free to
        /// drift from the first, and the day they disagreed a project test product would be sufficient by
        /// one rule and insufficient by the other.
        /// </para>
        ///
        /// <para><b>What it deliberately does not carry</b></para>
        /// <para>
        /// No performance table and no control curve. A what-if capacity has no published fan data, and
        /// inventing some would be a fabrication that later reads as measurement. Iteration 3 resolving
        /// this template therefore finds no performance data and says so, which is the correct answer.
        /// <see cref="VentilationUnitTemplate.Source"/> states in words that these are not manufacturer
        /// figures, and <see cref="VentilationUnitTemplate.Rank"/> is
        /// <c>PartOProjectTestVentilationUnit.Rank</c> - far beyond any shipped rank, so a test product
        /// the same size as a real one loses the tie instead of making it ambiguous.
        /// </para>
        /// </summary>
        /// <param name="partOProjectTestVentilationUnit">The project's test product. Null, or one that states nothing usable, yields null.</param>
        public static VentilationUnitTemplate VentilationUnitTemplate(this PartOProjectTestVentilationUnit partOProjectTestVentilationUnit)
        {
            if (partOProjectTestVentilationUnit is null || !partOProjectTestVentilationUnit.IsValid)
            {
                return null;
            }

            return new VentilationUnitTemplate(partOProjectTestVentilationUnit.VentilationUnitReference, PartOProjectTestVentilationUnit.Source)
            {
                MaximumSupplyFlowRate_Lps = partOProjectTestVentilationUnit.MaximumSupplyFlowRate_Lps,
                MaximumExtractFlowRate_Lps = partOProjectTestVentilationUnit.MaximumExtractFlowRate_Lps,
                Rank = PartOProjectTestVentilationUnit.Rank,
            };
        }

        /// <summary>
        /// The project test product as the selection-facing descriptors it contributes: one, or none.
        /// <para>
        /// A list rather than a single descriptor because that is the shape every caller wants - the
        /// capability lookup and
        /// <see cref="PartOEquipmentSelection.CandidateDescriptors(IEnumerable{VentilationUnitCapacityDescriptor}, IEnumerable{VentilationUnitCapacityDescriptor})"/>
        /// both take a sequence - and because "the project states no test product" and "the project
        /// states an unusable one" are then the same empty answer at the point of use, which is what they
        /// mean there. Never null, so no caller has to distinguish them.
        /// </para>
        /// </summary>
        public static List<VentilationUnitCapacityDescriptor> CapacityDescriptors(this PartOProjectTestVentilationUnit partOProjectTestVentilationUnit)
        {
            VentilationUnitTemplate ventilationUnitTemplate = VentilationUnitTemplate(partOProjectTestVentilationUnit);

            return ventilationUnitTemplate is null ? [] : CapacityDescriptors([ventilationUnitTemplate]);
        }
    }
}
