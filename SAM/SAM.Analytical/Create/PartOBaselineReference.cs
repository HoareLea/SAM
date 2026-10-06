// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.IO;

namespace SAM.Analytical
{
    public static partial class Create
    {
        /// <summary>
        /// The reference of a case derived from the <b>design model</b> directly - Iteration 1a, 1b, 2 and Mixed Design:
        /// the design's identity (guid, name, state fingerprint) and, where its file and the result's folder are known, the
        /// relative path from one to the other. Null for a case that derives from a result, for no design, or for a design that
        /// has no identity.
        ///
        /// <para>
        /// The design's <b>state fingerprint is taken here, from <paramref name="design"/> as it is handed in</b> - nothing about the
        /// result being derived can reach it. The reference is stamped on a different model (the result), and no reference ever
        /// holds the fingerprint of the model that carries it.
        /// </para>
        /// <para>No absolute path is kept: <paramref name="path_Design"/> is used here to compute the relative locator and is not stored.</para>
        /// </summary>
        /// <param name="partODerivedCase">The case being derived.</param>
        /// <param name="design">The design model, in the state the case is derived from. Not modified.</param>
        /// <param name="path_Design">The design model's file, or null/empty where it has never been saved. Used only for the relative locator.</param>
        /// <param name="directory_Result">The folder the result model will be written to, or null where not yet known (then no locator is recorded).</param>
        /// <param name="fingerprint">
        /// The design's state fingerprint where the caller already holds it (Mixed Design computes it for its
        /// materialisation record); null to compute it here, which costs in proportion to the size of the project.
        /// </param>
        public static PartOBaselineReference PartOBaselineReferenceFromDesign(PartODerivedCase partODerivedCase, AnalyticalModel design, string path_Design, string directory_Result, string fingerprint = null)
        {
            if (design is null || partODerivedCase == PartODerivedCase.Undefined || partODerivedCase == PartODerivedCase.Iteration2B || partODerivedCase == PartODerivedCase.Iteration3)
            {
                return null;
            }

            PartOModelReference partOModelReference = new(PartOModelReferenceKind.Design, design.Guid, design.Name, string.IsNullOrEmpty(fingerprint) ? SimulationResultProvenance.Fingerprint(design) : fingerprint, Query.PartOBaselineRelativePath(directory_Result, path_Design));
            if (!partOModelReference.IsValid)
            {
                return null;
            }

            return new PartOBaselineReference(partODerivedCase, partOModelReference, null);
        }

        /// <summary>
        /// The reference of a case derived from a <b>result</b> - Iteration 2B (the Iteration 2 result) and Iteration 3 (the
        /// 1a or 2 result): the source result's identity and the design the source itself was derived from, inherited
        /// from the reference the source carries. Null for a case that derives from the design, for no source, or for a source that is
        /// not a proven result (no provenance) or has no identity.
        ///
        /// <para>
        /// The source's state is its own <c>SimulationResultProvenance.Fingerprint_Model</c> <b>as the source recorded it</b>, read, never
        /// recomputed and never touched: the model being stamped is another model, so stamping cannot move it. Resolution later compares
        /// this recorded state with the source as it then is.
        /// </para>
        /// <para>
        /// Only relative locators are kept. The source's is computed from <paramref name="path_Source"/> to <paramref name="directory_Result"/>; the
        /// inherited design's is <b>rebased</b> from the source's folder to <paramref name="directory_Result"/> (it was relative to the source), and is
        /// left absent when the source's own file is not known.
        /// </para>
        /// </summary>
        /// <param name="partODerivedCase">The case being derived.</param>
        /// <param name="source">The source result model, as its run returned it. Not modified.</param>
        /// <param name="path_Source">The source result's saved model file, or null/empty where unknown. Used only for relative locators.</param>
        /// <param name="directory_Result">The folder the result model will be written to, or null where not yet known (then no locator is recorded).</param>
        public static PartOBaselineReference PartOBaselineReferenceFromResult(PartODerivedCase partODerivedCase, AnalyticalModel source, string path_Source, string directory_Result)
        {
            if (source is null || (partODerivedCase != PartODerivedCase.Iteration2B && partODerivedCase != PartODerivedCase.Iteration3))
            {
                return null;
            }

            if (!source.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance simulationResultProvenance) || simulationResultProvenance is null || string.IsNullOrEmpty(simulationResultProvenance.Fingerprint_Model))
            {
                return null;
            }

            PartOModelReference partOModelReference_Source = new(PartOModelReferenceKind.Result, source.Guid, source.Name, simulationResultProvenance.Fingerprint_Model, Query.PartOBaselineRelativePath(directory_Result, path_Source));
            if (!partOModelReference_Source.IsValid)
            {
                return null;
            }

            PartOModelReference partOModelReference_Design = null;
            if (source.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference partOBaselineReference_Source) && partOBaselineReference_Source is not null && partOBaselineReference_Source.Design is not null)
            {
                partOModelReference_Design = new PartOModelReference(partOBaselineReference_Source.Design)
                {
                    Path_Relative = string.IsNullOrWhiteSpace(path_Source) ? null : Query.PartOBaselineRebasedPath(partOBaselineReference_Source.Design.Path_Relative, Path.GetDirectoryName(path_Source), directory_Result),
                };
            }

            return new PartOBaselineReference(partODerivedCase, partOModelReference_Design, partOModelReference_Source);
        }
    }
}
