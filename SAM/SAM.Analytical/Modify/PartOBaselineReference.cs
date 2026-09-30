// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.IO;

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
        /// Completes the <b>locators</b> of the reference a result model carries, once the folder the result model is
        /// written to is known: the relative path of each referenced model from that folder, and - where the reference
        /// records no file for its design - <paramref name="path_Design"/>. Identity is never touched.
        ///
        /// <para>
        /// A relative path is what lets a whole case tree that is copied or moved still find its design. It is written
        /// only where both ends are on the same root (a path on another drive has no relative form) and is otherwise
        /// left absent, so the absolute path alone is tried.
        /// </para>
        /// <para>
        /// A model with no reference, or only an invalid one, is left exactly as it is. Like the stamp, call it before
        /// the provenance record is constructed.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">The result model. Its reference is replaced by the located copy.</param>
        /// <param name="directory_Result">The folder the result model (<c>.sam</c>) will be written to.</param>
        /// <param name="path_Design">The design model's file, used only where the reference names none. Null or empty adds nothing.</param>
        /// <returns>Whether the reference was updated.</returns>
        public static bool LocatePartOBaselineReference(this AnalyticalModel analyticalModel, string directory_Result, string path_Design = null)
        {
            if (analyticalModel is null || !analyticalModel.TryGetValue(AnalyticalModelParameter.PartOBaselineReference, out PartOBaselineReference partOBaselineReference) || partOBaselineReference is null || !partOBaselineReference.IsValid)
            {
                return false;
            }

            PartOBaselineReference result = new(partOBaselineReference);

            if (result.Design is not null && string.IsNullOrWhiteSpace(result.Design.Path_Absolute) && !string.IsNullOrWhiteSpace(path_Design))
            {
                result.Design.Path_Absolute = path_Design;
            }

            Locate(result.Design, directory_Result);
            Locate(result.Source, directory_Result);

            analyticalModel.SetValue(AnalyticalModelParameter.PartOBaselineReference, result);

            return true;
        }

        private static void Locate(PartOModelReference partOModelReference, string directory_Result)
        {
            if (partOModelReference is null || string.IsNullOrWhiteSpace(partOModelReference.Path_Absolute) || string.IsNullOrWhiteSpace(directory_Result))
            {
                return;
            }

            partOModelReference.Path_Relative = Query.PartOBaselineRelativePath(directory_Result, partOModelReference.Path_Absolute);
        }
    }
}
