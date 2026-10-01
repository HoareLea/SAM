// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Stamps <paramref name="partOBaselineReference"/> on a Part O <b>result</b> model as
        /// <c>AnalyticalModelParameter.PartOBaselineReference</c>, replacing any reference the model inherited from the
        /// model it was copied from (an Iteration 2B round copies its Iteration 2 result, so it carries that result's
        /// reference until this replaces it).
        ///
        /// <para>
        /// Stamp it <b>before</b> the model's <c>SimulationResultProvenance</c> is constructed: the provenance
        /// fingerprints every model parameter except its own exclusions, so a reference stamped afterwards would make the
        /// saved model look edited.
        /// </para>
        /// <para>
        /// Never call this on a design model. The reference is what marks a model as a result.
        /// </para>
        /// </summary>
        /// <returns>Whether the model now carries the reference; false, and the model is left as it was, for a null model or an invalid reference.</returns>
        public static bool StampPartOBaselineReference(this AnalyticalModel analyticalModel, PartOBaselineReference partOBaselineReference)
        {
            if (analyticalModel is null || partOBaselineReference is null || !partOBaselineReference.IsValid)
            {
                return false;
            }

            analyticalModel.SetValue(AnalyticalModelParameter.PartOBaselineReference, new PartOBaselineReference(partOBaselineReference));

            return true;
        }

        /// <summary>
        /// Gives the reference a result model carries its <b>design</b> locator where it has none, once the folder the result is written to is
        /// known: the relative path from that folder to <paramref name="path_Design"/>. For a case whose materialiser knows the design by identity
        /// but not by file (Mixed Design). A locator already recorded is never replaced, and identity is never touched.
        ///
        /// <para>
        /// Only the relative path is recorded; <paramref name="path_Design"/> itself is not kept. A path with no relative form (another root) records
        /// nothing. A model with no reference, or only an invalid one, is left exactly as it is. Like the stamp, call it before the provenance record
        /// is constructed.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">The result model.</param>
        /// <param name="directory_Result">The folder the result model (<c>.sam</c>) will be written to.</param>
        /// <param name="path_Design">The design model's file. Null or empty adds nothing.</param>
        /// <returns>Whether the reference was updated.</returns>
        public static bool LocatePartOBaselineReference(this AnalyticalModel analyticalModel, string directory_Result, string path_Design)
        {
            if (analyticalModel is null || !analyticalModel.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference partOBaselineReference) || partOBaselineReference is null || !partOBaselineReference.IsValid)
            {
                return false;
            }

            if (partOBaselineReference.Design is null || !string.IsNullOrWhiteSpace(partOBaselineReference.Design.Path_Relative))
            {
                return false;
            }

            string path_Relative = Query.PartOBaselineRelativePath(directory_Result, path_Design);
            if (string.IsNullOrEmpty(path_Relative))
            {
                return false;
            }

            PartOBaselineReference result = new(partOBaselineReference);
            result.Design.Path_Relative = path_Relative;

            analyticalModel.SetValue(AnalyticalModelParameter.PartOBaselineReference, result);

            return true;
        }
    }
}
