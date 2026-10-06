// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>Named apertures beyond this many are counted rather than listed, so a refusal stays readable.</summary>
        private const int partOOpeningEvidenceLimit = 5;

        /// <summary>
        /// Whether a model's authored opening behaviour <b>satisfies</b> the <see cref="OpeningsRestricted"/>
        /// assumption a Part O mitigation stage states. <b>Nothing is changed.</b>
        /// <para>
        /// <b>This validates; it does not reconcile.</b> An <see cref="OpeningRestriction"/> - or a
        /// <see cref="ProfileOpeningProperties"/> availability schedule - is authored building data.
        /// <c>SAMAnalytical.AddOpeningPropertiesByPartO</c> records only <i>that</i> an opening is restricted,
        /// never <i>why</i>, so a night-closed aperture may be shut for noise, for security, or because it is
        /// an internal door. A stage's assumptions are a different kind of statement: a label the result is
        /// attributed under, participating in <c>OverheatingScenario.Key</c>. Rewriting the building to match
        /// the label would simulate a building the modeller never described - and, because
        /// <see cref="PartOOpeningProperties.Schedule"/> is <b>derived</b> from
        /// <see cref="PartOOpeningProperties.OpeningRestriction"/> rather than stored beside it, would delete
        /// the aperture's availability schedule as a side effect of "only" changing a restriction state.
        /// So the disagreement is <b>reported</b> and the modeller decides which of the two statements is
        /// wrong.
        /// </para>
        /// <para>
        /// <b>Advisory, and deliberately not a gate.</b> Nothing here blocks a run. An
        /// <see cref="OpeningRestriction"/> is authored building behaviour that is <i>orthogonal</i> to the
        /// mitigation stage - a base case may legitimately mix restricted and unrestricted openings, because
        /// a window shut for noise or security is a fact about the building rather than a mitigation
        /// somebody added. Making the stage's <see cref="OpeningsRestricted"/> assumption an assertion the
        /// model must satisfy is a known defect in the iteration model, and it is being fixed by moving that
        /// assumption from the stage to the model. Until then this reports the disagreement without acting
        /// on it.
        /// </para>
        /// <para>
        /// <b>Unknown is not unrestricted.</b> Openings authored through the legacy general-valued
        /// <see cref="ProfileOpeningProperties.Profile"/> carry availability in a form no deterministic
        /// classification exists for, and are reported as <see cref="PartOOpeningCompatibility.Unknown"/>
        /// rather than assumed compatible - otherwise the same silent mislabelling returns through the other
        /// authoring path. See <see cref="PartOOpeningRestricted(Aperture, out string)"/> for exactly what is
        /// and is not classifiable.
        /// </para>
        /// <para>
        /// <b>What each direction means.</b> Where the stage assumes openings are operated WITHOUT
        /// restriction, every operable opening must be positively unrestricted - one restricted opening is a
        /// proven disagreement. Where the stage assumes openings ARE restricted, the model must positively
        /// restrict at least one opening; which openings those are is the modeller's business, so a model
        /// that restricts some and not others satisfies the assumption. A model with no operable opening at
        /// all is compatible with either: the assumption has no subject to contradict.
        /// </para>
        /// </summary>
        /// <param name="analyticalModel">The model to validate. Never modified.</param>
        /// <param name="partOIteration">The mitigation stage whose assumptions are being stated.</param>
        /// <param name="summary">
        /// How the model and the stage disagree - naming the apertures, what they state, and what the stage
        /// assumes - or null when the verdict is <see cref="PartOOpeningCompatibility.Compatible"/>.
        /// </param>
        /// <param name="evidence">
        /// One line per aperture that carries the finding, for the record. Empty where there is nothing to
        /// report. Never null.
        /// </param>
        /// <returns>
        /// <see cref="PartOOpeningCompatibility.Compatible"/>,
        /// <see cref="PartOOpeningCompatibility.Incompatible"/> (a proven disagreement, which is reported in
        /// preference to an unknown because it is the stronger finding) or
        /// <see cref="PartOOpeningCompatibility.Unknown"/>. A stage that states no opening assumption at all -
        /// <see cref="PartOIteration.Undefined"/>, or a stage whose assumptions refuse - is
        /// <see cref="PartOOpeningCompatibility.Compatible"/>: it cannot disagree about something it does not
        /// state.
        /// </returns>
        public static PartOOpeningCompatibility PartOIterationOpeningCompatibility(this AnalyticalModel analyticalModel, PartOIteration partOIteration, out string summary, out List<string> evidence)
        {
            summary = null;
            evidence = new List<string>();

            if (analyticalModel == null)
            {
                return PartOOpeningCompatibility.Compatible;
            }

            OverheatingOperatingAssumptions overheatingOperatingAssumptions = partOIteration.PartOOperatingAssumptions(out string _);
            if (overheatingOperatingAssumptions == null || !overheatingOperatingAssumptions.Contains(OpeningsRestricted))
            {
                return PartOOpeningCompatibility.Compatible;
            }

            bool openingsRestricted = overheatingOperatingAssumptions.Value(OpeningsRestricted) == OverheatingOperatingAssumptions.Text(true);

            List<string> names_Disagreeing = new List<string>();
            List<string> names_Unknown = new List<string>();

            int count_Openings = 0;
            int count_Restricted = 0;

            List<Panel> panels = analyticalModel.AdjacencyCluster?.GetPanels();
            if (panels != null)
            {
                foreach (Panel panel in panels)
                {
                    List<Aperture> apertures = panel?.Apertures;
                    if (apertures == null)
                    {
                        continue;
                    }

                    foreach (Aperture aperture in apertures)
                    {
                        bool? restricted = aperture.PartOOpeningRestricted(out string evidence_Aperture);

                        if (!restricted.HasValue && evidence_Aperture == null)
                        {
                            //Not an operable opening - it states no opening properties at all.
                            continue;
                        }

                        count_Openings++;

                        if (!restricted.HasValue)
                        {
                            names_Unknown.Add(Named(aperture.Name, evidence_Aperture));

                            evidence.Add(string.Format(
                                "Aperture '{0}' (panel '{1}') states {2}, so it cannot be checked against the {3} stage's assumption that openings are operated {4} restriction.",
                                aperture.Name,
                                panel.Name,
                                evidence_Aperture,
                                partOIteration,
                                openingsRestricted ? "under" : "without"));

                            continue;
                        }

                        if (restricted.Value)
                        {
                            count_Restricted++;
                        }

                        if (!openingsRestricted && restricted.Value)
                        {
                            names_Disagreeing.Add(Named(aperture.Name, evidence_Aperture));

                            evidence.Add(string.Format(
                                "Aperture '{0}' (panel '{1}') states {2}, but the {3} stage assumes openings are operated without restriction.",
                                aperture.Name,
                                panel.Name,
                                evidence_Aperture,
                                partOIteration));
                        }
                        else if (openingsRestricted && !restricted.Value)
                        {
                            //Recorded, but only a disagreement if NO opening in the model restricts - see the remarks.
                            names_Disagreeing.Add(Named(aperture.Name, null));
                        }
                    }
                }
            }

            if (!openingsRestricted)
            {
                if (names_Disagreeing.Count != 0)
                {
                    summary = string.Format(
                        "The {0} stage states that openings are operated without restriction, but the model restricts {1} of its {2} operable opening(s): {3}. Nothing was changed: an opening restriction is authored building data, and resetting it would delete that aperture's availability schedule from the simulated model. Reported rather than reconciled - check that the stage you selected is the one you meant.",
                        partOIteration,
                        names_Disagreeing.Count,
                        count_Openings,
                        Named(names_Disagreeing));

                    return PartOOpeningCompatibility.Incompatible;
                }
            }
            else if (count_Openings != 0 && count_Restricted == 0 && names_Unknown.Count == 0)
            {
                summary = string.Format(
                    "The {0} stage states that openings are restricted, but none of the model's {1} operable opening(s) restricts anything: {2}. Nothing was changed. Reported rather than reconciled - check that the stage you selected is the one you meant.",
                    partOIteration,
                    count_Openings,
                    Named(names_Disagreeing));

                return PartOOpeningCompatibility.Incompatible;
            }

            if (names_Unknown.Count != 0)
            {
                summary = string.Format(
                    "{0} of the model's {1} operable opening(s) state availability SAM cannot classify as restricted or unrestricted: {2}. Their agreement with the {3} stage is therefore unknown, and an unknown is not read as unrestricted - guessing is how an opening that restricts the simulation ends up labelled as one that does not. Nothing was changed. Re-author those openings through SAMAnalytical.AddOpeningPropertiesByPartO's restriction_, or through a first-class 24-hour availability schedule, if the comparison needs to be conclusive.",
                    names_Unknown.Count,
                    count_Openings,
                    Named(names_Unknown),
                    partOIteration);

                return PartOOpeningCompatibility.Unknown;
            }

            return PartOOpeningCompatibility.Compatible;
        }

        /// <summary>
        /// One aperture, and what it states - so a refusal identifies the opening AND the behaviour that made
        /// it incompatible, not merely a name the modeller then has to go and look up.
        /// </summary>
        private static string Named(string name, string evidence)
        {
            return evidence == null ? string.Format("'{0}'", name) : string.Format("'{0}' ({1})", name, evidence);
        }

        /// <summary>The apertures for a refusal to quote, capped so a whole-building mismatch stays readable.</summary>
        private static string Named(List<string> names)
        {
            if (names == null || names.Count == 0)
            {
                return "none";
            }

            if (names.Count <= partOOpeningEvidenceLimit)
            {
                return string.Join(", ", names);
            }

            return string.Format("{0} and {1} more", string.Join(", ", names.GetRange(0, partOOpeningEvidenceLimit)), names.Count - partOOpeningEvidenceLimit);
        }
    }
}
