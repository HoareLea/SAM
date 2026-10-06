// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Realizes the Approved Document F continuous ventilation requirements of the given spaces as
        /// design <see cref="VentilationTerminal"/>s, related to the spaces they serve.
        /// <para>
        /// <b>The requirement is read and never written.</b> Nothing here touches
        /// <c>PartFSpaceData</c> or any <see cref="PartFVentilationTerminalRequirement"/> on it. A design
        /// realization is a separate statement about the same room, and the two have to be able to
        /// disagree - that is what makes the disagreement reportable instead of invisible.
        /// </para>
        ///
        /// <para><b>The initial realization strategy, and what it is not</b></para>
        /// <para>
        /// Where a requirement has no design terminal yet, <b>one</b> terminal is created carrying the
        /// room's whole continuous duty. That is a starting point, not a rule: a designer may replace one
        /// 20 l/s terminal with two of 10 or four of 5, and re-running this changes nothing, because the
        /// second pass only creates terminals for requirements that have <i>none</i>. Nowhere is
        /// one-terminal-per-space an invariant, and a subdivision must never be pushed back into
        /// <c>PartFSpaceData.Terminals</c>, which is the regulatory statement and not a place to record
        /// design choices.
        /// </para>
        ///
        /// <para><b>Re-linking after a Part F recalculation, and why it is two passes</b></para>
        /// <para>
        /// <c>PartFCalculator</c> constructs a brand new requirement object for every terminal on every
        /// run, so a design terminal's <c>RequirementGuid</c> is stale after any recalculation. The first
        /// pass therefore re-links every existing terminal to the requirement that <i>replaced</i> the one
        /// it was made from, matched on the requirement's regulatory identity - room, Approved Document
        /// role and source paragraph - held in <see cref="PartFTerminalReference"/>.
        /// </para>
        /// <para>
        /// The matching is terminal to requirement, which is many-to-one, so a subdivided room re-links
        /// all of its terminals to the one requirement they share. It is explicit in every direction:
        /// exactly one match re-links and reports; <b>no match refuses</b>, because a terminal whose
        /// requirement no longer exists is a lineage question for an engineer, not something to quietly
        /// drop or quietly keep; and <b>more than one match refuses as ambiguous</b>. Nothing is guessed
        /// and nothing is silently repaired.
        /// </para>
        /// </summary>
        /// <param name="adjacencyCluster">
        /// The model to add terminals to. <b>Modified in place</b>, so hand it a cluster you already own -
        /// <c>AnalyticalModel.AdjacencyCluster</c> returns a copy, and mutating a second copy would lose
        /// the work.
        /// </param>
        /// <param name="spaces">
        /// The spaces to realize. Null means every space in the cluster. A space with no
        /// <c>PartFSpaceData</c> is skipped, not refused - circulation and plant are legitimately unsized.
        /// </param>
        /// <param name="notes">What was created and what was re-linked.</param>
        /// <param name="refusals">Every lineage question that could not be answered, one sentence each.</param>
        /// <returns>Every design terminal now related to those spaces, or null where nothing could be done.</returns>
        public static List<VentilationTerminal> RealizePartFVentilationTerminals(this AdjacencyCluster adjacencyCluster, IEnumerable<Space> spaces, out List<string> notes, out List<string> refusals)
        {
            notes = [];
            refusals = [];

            if (adjacencyCluster is null)
            {
                refusals.Add("No model was supplied, so no design ventilation terminals could be realized.");

                return null;
            }

            //ONE snapshot of the model's space identities, taken before any terminal is realized. Resolving
            //each space of the scope against the whole space list one at a time made this quadratic in the
            //model.
            PartFIndex partFIndex = new(adjacencyCluster);

            List<Space> spaces_Cluster = partFIndex.Spaces;

            List<Space> spaces_Temp = [];
            foreach (Space space in spaces ?? spaces_Cluster)
            {
                if (space is not null)
                {
                    //Taken from the cluster rather than trusted as handed in: the caller may be holding
                    //space objects from before the Part F rates were applied, and those carry a different
                    //internal condition and a different parameter set.
                    Space space_Cluster = partFIndex.Space(space.Guid);
                    if (space_Cluster is not null)
                    {
                        spaces_Temp.Add(space_Cluster);
                    }
                }
            }

            if (spaces_Temp.Count == 0)
            {
                refusals.Add("No space of the model was resolved, so there are no Approved Document F requirements to realize.");

                return null;
            }

            List<VentilationTerminal> result = [];

            foreach (Space space in spaces_Temp)
            {
                PartFSpaceData partFSpaceData = space.GetValue<PartFSpaceData>(SpaceParameter.PartFSpaceData);
                if (partFSpaceData is null)
                {
                    //Not a refusal: circulation, storage, a plant room or an unclassified space is
                    //legitimately unsized, exactly as Modify.ApplyPartFVentilationRates treats it.
                    continue;
                }

                List<PartFVentilationTerminalRequirement> requirements = partFSpaceData.Terminals ?? [];

                List<VentilationTerminal> ventilationTerminals = adjacencyCluster.VentilationTerminals(space) ?? [];

                // ---- Pass 1: re-link what is already there ------------------------------------------

                for (int i = 0; i < ventilationTerminals.Count; i++)
                {
                    VentilationTerminal ventilationTerminal = ventilationTerminals[i];

                    PartFTerminalReference partFTerminalReference = ventilationTerminal?.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference);
                    if (partFTerminalReference is null)
                    {
                        //A terminal a designer added themselves, realizing nothing regulatory. It has no
                        //lineage to keep and none to lose.
                        continue;
                    }

                    List<PartFVentilationTerminalRequirement> requirements_Matched = requirements.FindAll(partFTerminalReference.Matches);

                    if (requirements_Matched.Count == 0)
                    {
                        refusals.Add(string.Format(
                            "Design ventilation terminal '{0}' in space '{1}' realizes {2}, and the current Approved Document F data for that space contains no such requirement. Either the terminal is left over from a design the Part F calculation no longer describes, or the space has been reclassified. Nothing was prepared: re-running the Part F calculation or deleting the terminal are different answers and only an engineer can choose between them.",
                            ventilationTerminal.Name,
                            space.Name,
                            partFTerminalReference.Description()));

                        continue;
                    }

                    if (requirements_Matched.Count > 1)
                    {
                        refusals.Add(string.Format(
                            "Design ventilation terminal '{0}' in space '{1}' realizes {2}, and the current Approved Document F data for that space contains {3} requirements that answer to the same regulatory identity, so which one it now realizes is ambiguous. Nothing was prepared.",
                            ventilationTerminal.Name,
                            space.Name,
                            partFTerminalReference.Description(),
                            requirements_Matched.Count));

                        continue;
                    }

                    PartFVentilationTerminalRequirement requirement = requirements_Matched[0];
                    if (requirement.Guid == partFTerminalReference.RequirementGuid)
                    {
                        continue;
                    }

                    //A COPY with the same guid, so the cluster this call was given is updated while the
                    //model the caller still holds is not reached through the shared object instance.
                    VentilationTerminal ventilationTerminal_Relinked = new(ventilationTerminal.Guid, ventilationTerminal);
                    ventilationTerminal_Relinked.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(requirement));

                    adjacencyCluster.AddObject(ventilationTerminal_Relinked);

                    //Substituted in place rather than re-read. AddObject REPLACES the terminal stored under
                    //this guid, so re-asking the cluster for the space's terminals returned the very same
                    //list with this very same element swapped - the relation set did not move and the
                    //dictionary entry kept its slot. Doing that per space made this three relation reads per
                    //room rather than two, and a relation read walks every object related to the room -
                    //every bounding panel included - across every stored type.
                    ventilationTerminals[i] = ventilationTerminal_Relinked;

                    notes.Add(string.Format(
                        "Design ventilation terminal '{0}' in space '{1}' was re-linked from Approved Document F requirement {2} to {3}, which is the same requirement recalculated - {4}.",
                        ventilationTerminal.Name,
                        space.Name,
                        partFTerminalReference.RequirementGuid,
                        requirement.Guid,
                        partFTerminalReference.Description()));
                }

                //Pass 2 sees the re-linked references rather than the stale ones because pass 1 substituted
                //each one it wrote - see there.

                // ---- Pass 2: create what is missing ---------------------------------------------------

                foreach (PartFVentilationTerminalRequirement requirement in requirements)
                {
                    if (requirement is null)
                    {
                        continue;
                    }

                    //Continuous operation is the whole criterion. An intermittent cooker hood or separate
                    //extract fan carries no ContinuousDesignFlowRate_Lps, does not run at the Approved
                    //Document F sizing condition, and is already outside every continuous total - giving
                    //it a design-rate terminal would credit the balanced system with air it does not move.
                    double? continuous_Lps = requirement.ContinuousDesignFlowRate_Lps;
                    if (!continuous_Lps.HasValue || double.IsNaN(continuous_Lps.Value))
                    {
                        continue;
                    }

                    if (ventilationTerminals.Exists(x => x?.GetValue<PartFTerminalReference>(VentilationTerminalParameter.PartFTerminalReference)?.RequirementGuid == requirement.Guid))
                    {
                        //Already realized - by one terminal, or by four the designer subdivided it into.
                        //Either way this requirement is not missing a realization and nothing is added.
                        continue;
                    }

                    FlowClassification flowClassification = requirement.IsExtract ? FlowClassification.Extract : FlowClassification.Supply;

                    VentilationTerminal ventilationTerminal = new(string.Format("{0} terminal", requirement.Name), flowClassification, continuous_Lps.Value);
                    ventilationTerminal.SetValue(VentilationTerminalParameter.PartFTerminalReference, new PartFTerminalReference(requirement));

                    adjacencyCluster.AddObject(ventilationTerminal);
                    adjacencyCluster.AddRelation(ventilationTerminal, space);

                    notes.Add(string.Format(
                        "Space '{0}': realized a {1} terminal of {2:0.###} l/s from the {3} requirement of Approved Document F.",
                        space.Name,
                        Core.Query.Description(flowClassification),
                        continuous_Lps.Value,
                        Core.Query.Description(requirement.TerminalRole)));
                }

                result.AddRange(adjacencyCluster.VentilationTerminals(space) ?? []);
            }

            return result;
        }
    }
}
