// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;

namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// The reference of a case derived from the <b>design model</b> directly - Iteration 1a, 1b, 2 and Mixed Design:
        /// the design's identity (guid, name, state fingerprint) and, where known, the file it was opened from.
        /// Null for a case that derives from a result, for no design, or for a design that has no identity.
        /// </summary>
        /// <param name="partODerivedCase">The case being derived.</param>
        /// <param name="design">The design model, in the state the case is derived from. Not modified.</param>
        /// <param name="path_Design">The design model's file, or null/empty where it has never been saved. A locator only.</param>
        /// <param name="fingerprint">
        /// The design's state fingerprint where the caller already holds it (Mixed Design computes it for its
        /// materialisation record); null to compute it here, which costs in proportion to the size of the project.
        /// </param>
        public static PartOBaselineReference PartOBaselineReferenceFromDesign(PartODerivedCase partODerivedCase, AnalyticalModel design, string path_Design, string fingerprint = null)
        {
            if (design is null || partODerivedCase == PartODerivedCase.Undefined || partODerivedCase == PartODerivedCase.Iteration2B || partODerivedCase == PartODerivedCase.Iteration3)
            {
                return null;
            }

            PartOModelReference partOModelReference = new(PartOModelReferenceKind.Design, design.Guid, design.Name, string.IsNullOrEmpty(fingerprint) ? SimulationResultProvenance.Fingerprint(design) : fingerprint, string.IsNullOrWhiteSpace(path_Design) ? null : path_Design);
            if (!partOModelReference.IsValid)
            {
                return null;
            }

            return new PartOBaselineReference(partODerivedCase, partOModelReference, null);
        }

        /// <summary>
        /// The reference of a case derived from a <b>result</b> - Iteration 2B (the Iteration 2 result) and Iteration 3 (the
        /// 1a or 2 result): the source result's identity and the design the source itself was derived from, inherited
        /// from the reference the source carries. The source's state is its own <c>SimulationResultProvenance.Fingerprint_Model</c>,
        /// so nothing is re-hashed. Null for a case that derives from the design, for no source, or for a source that is
        /// not a proven result (no provenance) or has no identity.
        /// </summary>
        /// <param name="partODerivedCase">The case being derived.</param>
        /// <param name="source">The source result model, as its run returned it. Not modified.</param>
        /// <param name="path_Source">The source result's saved model file, or null/empty where unknown. A locator only.</param>
        public static PartOBaselineReference PartOBaselineReferenceFromResult(PartODerivedCase partODerivedCase, AnalyticalModel source, string path_Source)
        {
            if (source is null || (partODerivedCase != PartODerivedCase.Iteration2B && partODerivedCase != PartODerivedCase.Iteration3))
            {
                return null;
            }

            if (!source.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance simulationResultProvenance) || simulationResultProvenance is null || string.IsNullOrEmpty(simulationResultProvenance.Fingerprint_Model))
            {
                return null;
            }

            PartOModelReference partOModelReference_Source = new(PartOModelReferenceKind.Result, source.Guid, source.Name, simulationResultProvenance.Fingerprint_Model, string.IsNullOrWhiteSpace(path_Source) ? null : path_Source);
            if (!partOModelReference_Source.IsValid)
            {
                return null;
            }

            PartOModelReference partOModelReference_Design = null;
            if (source.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference partOBaselineReference_Source) && partOBaselineReference_Source is not null && partOBaselineReference_Source.Design is not null)
            {
                partOModelReference_Design = new PartOModelReference(partOBaselineReference_Source.Design);
            }

            return new PartOBaselineReference(partODerivedCase, partOModelReference_Design, partOModelReference_Source);
        }
    }
}
