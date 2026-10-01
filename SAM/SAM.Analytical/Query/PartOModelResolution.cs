// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>The most files beside a recorded location that are opened to find a renamed model.</summary>
        private const int Count_Neighbours_Max = 16;

        /// <summary>
        /// Where a model a Part O result was derived from is <b>now</b>, found by identity and never by name.
        ///
        /// <para><b>The order</b></para>
        /// <list type="number">
        /// <item>The recorded <see cref="PartOModelReference.Path_Relative"/>, from <paramref name="path_Result"/>'s folder.</item>
        /// <item>The caller's <paramref name="path_Hint"/>, if it gave one: a place it knows to look <b>now</b>. It is not persisted anywhere, and
        /// a file there is accepted only by identity like any other.</item>
        /// <item>Only when neither holds the model: the other <c>.sam</c> files beside those two locations, so a design that
        /// was renamed is still found. At most <c>16</c> files are opened.</item>
        /// </list>
        ///
        /// <para><b>What counts as the model</b></para>
        /// <para>
        /// A file is accepted only when it is the same model by <see cref="PartOModelReference.Guid"/> <b>and</b> is of the
        /// referenced kind: a <b>design</b> carries no Part O result state (<c>SimulationResultProvenance</c> or
        /// overheating scenarios - a result shares its design's guid, so a result is never taken for the design), and a
        /// <b>result</b> carries a simulation provenance. Its state then decides <see cref="PartOBaselineResolutionStatus.Resolved"/>
        /// (the fingerprint of the model as it is now - <b>recomputed</b>, for a result as for a design - is the recorded one) or
        /// <see cref="PartOBaselineResolutionStatus.Changed"/> (it is not - the design was edited, the result was overwritten by a
        /// later run, or its content was edited after it was saved). A file with another guid is ignored whatever it is
        /// called. A file name is never compared, and no absolute path takes part: none is persisted.
        /// </para>
        /// <para>
        /// More than one file beside the recorded location that is the model in the same state is
        /// <see cref="PartOBaselineResolutionStatus.Ambiguous"/>: none is chosen. Nothing is written or opened in the application.
        /// </para>
        /// </summary>
        /// <param name="partOModelReference">The reference to resolve.</param>
        /// <param name="path_Result">The result model's own file, from which the relative locator is resolved. May be null, then only the hint is tried.</param>
        /// <param name="path_Hint">A file the caller knows of now (the open model, say), tried after the relative locator. Never persisted; null for none.</param>
        public static PartOBaselineResolution PartOModelResolution(this PartOModelReference partOModelReference, string path_Result, string path_Hint = null)
        {
            if (partOModelReference is null || !partOModelReference.IsValid)
            {
                return new PartOBaselineResolution(PartOBaselineResolutionStatus.Unknown, null, "The result does not say which model it was derived from.");
            }

            string directory_Result = null;
            try
            {
                directory_Result = string.IsNullOrWhiteSpace(path_Result) ? null : Path.GetDirectoryName(Path.GetFullPath(path_Result));
            }
            catch
            {
                directory_Result = null;
            }

            List<string> paths_Candidate = [];

            if (directory_Result is not null && !string.IsNullOrWhiteSpace(partOModelReference.Path_Relative))
            {
                try
                {
                    paths_Candidate.Add(Path.GetFullPath(Path.Combine(directory_Result, partOModelReference.Path_Relative)));
                }
                catch
                {
                }
            }

            if (!string.IsNullOrWhiteSpace(path_Hint))
            {
                try
                {
                    paths_Candidate.Add(Path.GetFullPath(path_Hint));
                }
                catch
                {
                }
            }

            paths_Candidate = [.. paths_Candidate.Distinct(StringComparer.OrdinalIgnoreCase)];

            string path_Result_Full = null;
            try
            {
                path_Result_Full = string.IsNullOrWhiteSpace(path_Result) ? null : Path.GetFullPath(path_Result);
            }
            catch
            {
            }

            string path_Changed = null;
            foreach (string path in paths_Candidate)
            {
                if (!File.Exists(path) || string.Equals(path, path_Result_Full, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool? state = Inspect(path, partOModelReference);
                if (state is true)
                {
                    return Found(partOModelReference, path, true);
                }

                if (state is false && path_Changed is null)
                {
                    path_Changed = path;
                }
            }

            //Found by identity at a recorded place, but not as it was: the design moved on, or the result was overwritten.
            if (path_Changed is not null)
            {
                return Found(partOModelReference, path_Changed, false);
            }

            //Not where it was recorded: the .sam files beside those places, in case it was renamed.
            List<string> directories = [];
            foreach (string path in paths_Candidate)
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory) && !directories.Contains(directory, StringComparer.OrdinalIgnoreCase))
                {
                    directories.Add(directory);
                }
            }

            List<string> paths_Resolved = [];
            List<string> paths_Changed = [];
            int count = 0;
            foreach (string directory in directories)
            {
                IEnumerable<string> paths_Neighbour;
                try
                {
                    paths_Neighbour = Directory.EnumerateFiles(directory, "*.sam", SearchOption.TopDirectoryOnly).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    continue;
                }

                foreach (string path in paths_Neighbour)
                {
                    if (paths_Candidate.Contains(path, StringComparer.OrdinalIgnoreCase) || string.Equals(path, path_Result_Full, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (count++ >= Count_Neighbours_Max)
                    {
                        break;
                    }

                    bool? state = Inspect(path, partOModelReference);
                    if (state is null)
                    {
                        continue;
                    }

                    (state.Value ? paths_Resolved : paths_Changed).Add(path);
                }
            }

            List<string> paths_Found = paths_Resolved.Count != 0 ? paths_Resolved : paths_Changed;
            if (paths_Found.Count > 1)
            {
                return new PartOBaselineResolution(PartOBaselineResolutionStatus.Ambiguous, null, string.Format("More than one file beside where the {0} model was recorded is that model, so none was chosen.", Kind(partOModelReference)));
            }

            if (paths_Found.Count == 1)
            {
                return Found(partOModelReference, paths_Found[0], paths_Resolved.Count != 0);
            }

            return new PartOBaselineResolution(PartOBaselineResolutionStatus.NotFound, null, string.Format("The {0} model '{1}' this result was derived from was not found where it was recorded or beside it.", Kind(partOModelReference), partOModelReference.Name));
        }

        /// <summary>
        /// A relative locator re-expressed for another folder: <paramref name="path_Relative"/> is relative to <paramref name="directory_From"/>, and the
        /// result is the same file relative to <paramref name="directory_To"/>. Null where any part is unknown or there is no relative form.
        /// How a result derived from another result keeps pointing at the design the first one came from.
        /// </summary>
        public static string PartOBaselineRebasedPath(string path_Relative, string directory_From, string directory_To)
        {
            if (string.IsNullOrWhiteSpace(path_Relative) || string.IsNullOrWhiteSpace(directory_From) || string.IsNullOrWhiteSpace(directory_To))
            {
                return null;
            }

            try
            {
                return PartOBaselineRelativePath(directory_To, Path.GetFullPath(Path.Combine(directory_From, path_Relative)));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The path of <paramref name="path"/> relative to the folder <paramref name="directory"/>, or null where it has no relative form
        /// (another root) or either is not a usable path. Separators are the platform's.
        /// </summary>
        public static string PartOBaselineRelativePath(string directory, string path)
        {
            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            try
            {
                string directory_Full = Path.GetFullPath(directory);
                if (!directory_Full.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    directory_Full += Path.DirectorySeparatorChar;
                }

                string path_Full = Path.GetFullPath(path);

                if (!string.Equals(Path.GetPathRoot(directory_Full), Path.GetPathRoot(path_Full), StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string result = Uri.UnescapeDataString(new Uri(directory_Full).MakeRelativeUri(new Uri(path_Full)).ToString());

                return result.Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Null when <paramref name="path"/> is not the referenced model (unreadable, another guid, or the wrong kind);
        /// otherwise whether it is in the recorded state.
        /// </summary>
        private static bool? Inspect(string path, PartOModelReference partOModelReference)
        {
            AnalyticalModel analyticalModel;
            try
            {
                analyticalModel = Core.Convert.ToSAM<AnalyticalModel>(path)?.FirstOrDefault();
            }
            catch
            {
                return null;
            }

            if (analyticalModel is null || analyticalModel.Guid != partOModelReference.Guid)
            {
                return null;
            }

            bool hasProvenance = analyticalModel.TryGetValue(AnalyticalModelParameter.SimulationResultProvenance, out SimulationResultProvenance simulationResultProvenance) && simulationResultProvenance is not null;

            if (partOModelReference.Kind == PartOModelReferenceKind.Result)
            {
                //Recomputed from the model as it is now, never read from the record the file carries: that record is what the result said of itself
                //when it was saved, and a result edited since still says it.
                return hasProvenance ? string.Equals(SimulationResultProvenance.Fingerprint(analyticalModel), partOModelReference.Fingerprint, StringComparison.Ordinal) : null;
            }

            //A result shares its design's guid, so a model that is a Part O result is never the design.
            if (hasProvenance || analyticalModel.HasValue(AnalyticalModelParameter.OverheatingScenarios))
            {
                return null;
            }

            return string.Equals(SimulationResultProvenance.Fingerprint(analyticalModel), partOModelReference.Fingerprint, StringComparison.Ordinal);
        }

        private static PartOBaselineResolution Found(PartOModelReference partOModelReference, string path, bool unchanged)
        {
            return unchanged
                ? new PartOBaselineResolution(PartOBaselineResolutionStatus.Resolved, path, string.Format("The {0} model '{1}' was found ({2}) and is as this result was derived from it.", Kind(partOModelReference), partOModelReference.Name, Path.GetFileName(path)))
                : new PartOBaselineResolution(PartOBaselineResolutionStatus.Changed, path, string.Format("The {0} model '{1}' was found ({2}) but has changed since this result was derived from it.", Kind(partOModelReference), partOModelReference.Name, Path.GetFileName(path)));
        }

        private static string Kind(PartOModelReference partOModelReference)
        {
            return partOModelReference.Kind == PartOModelReferenceKind.Result ? "source result" : "design";
        }
    }
}
