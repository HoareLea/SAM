// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Enums;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Every system offered that can do everything required - <b>suitability, and nothing else</b>.
        /// <para>
        /// This is the whole of what <c>SAM.Analytical</c> can honestly decide. Whether a system meets a
        /// requirement follows from the requirement and the system's capabilities; <b>which</b> of several
        /// suitable systems is the right answer does not. That is a judgement about a particular set of
        /// shipped templates, and the assembly that ships them is the one that knows.
        /// </para>
        /// <para>
        /// Returned in the supplied preference order - <c>Rank</c> first, then identity - so a caller that
        /// wants the preferred one can take the first, and a caller with a different policy has the whole
        /// suitable set to apply it to. The ordering is deterministic and independent of the order the
        /// descriptors arrived in.
        /// </para>
        /// <para>
        /// <b>A pure function.</b> It reads no file, opens no template and consults no library - the
        /// systems available are an argument, because which systems exist is a fact about whoever is
        /// asking. That is what keeps this side of the boundary free of any particular repository's
        /// shipping set, and it is why choosing a system can never cost the megabyte and a half a
        /// <c>SystemEnergyCentre</c> weighs.
        /// </para>
        /// </summary>
        /// <param name="systemCapabilityDescriptors">The systems available to choose from.</param>
        /// <param name="systemCapabilityRequirement">What the system has to be able to do.</param>
        public static List<SystemCapabilityDescriptor> CapableSystems(this IEnumerable<SystemCapabilityDescriptor> systemCapabilityDescriptors, SystemCapabilityRequirement systemCapabilityRequirement)
        {
            List<SystemCapabilityDescriptor> result = [];

            if (systemCapabilityRequirement == null || !systemCapabilityRequirement.IsValid)
            {
                return result;
            }

            foreach (SystemCapabilityDescriptor systemCapabilityDescriptor in systemCapabilityDescriptors ?? [])
            {
                if (systemCapabilityDescriptor != null && systemCapabilityDescriptor.IsValid && systemCapabilityRequirement.IsMetBy(systemCapabilityDescriptor.Capabilities))
                {
                    result.Add(systemCapabilityDescriptor);
                }
            }

            //Sorted through an index list so the insertion index is available as the FINAL tie-break: two
            //descriptors with the same rank AND the same identity then keep the order they arrived in
            //instead of being ordered arbitrarily. Sorting an index list without that last comparison is
            //exactly as unstable as sorting the list itself, which an earlier revision's comment claimed
            //otherwise - the same trap Solver2D was hardened for.
            List<int> indices = [];
            for (int i = 0; i < result.Count; i++)
            {
                indices.Add(i);
            }

            indices.Sort((x, y) =>
            {
                //CompareTo, not subtraction: ranks come from a file, and int.MaxValue - int.MinValue
                //overflows to a wrong sign, which both mis-orders the list and can make List.Sort throw on
                //an inconsistent comparator.
                int compare = result[x].Rank.CompareTo(result[y].Rank);
                if (compare != 0)
                {
                    return compare;
                }

                compare = SystemCapabilityDescriptor.CompareIdentity(result[x], result[y]);

                return compare != 0 ? compare : x.CompareTo(y);
            });

            List<SystemCapabilityDescriptor> result_Ordered = [];
            foreach (int index in indices)
            {
                result_Ordered.Add(result[index]);
            }

            return result_Ordered;
        }

        /// <summary>
        /// Chooses the system a supplier's own preference order puts first among those that meet the
        /// requirement.
        /// <para>
        /// <b>The preference is read, never invented.</b> An earlier revision of this chose the system with
        /// the fewest capabilities, on the reasoning that anything more implies plant nobody required.
        /// That was rejected, and rightly: it is a policy about a particular library of templates rather
        /// than something that follows from Approved Document F, and a capability a system happens to have
        /// may cost nothing to specify. The order now comes from
        /// <see cref="SystemCapabilityDescriptor.Rank"/>, which the catalogue supplies.
        /// </para>
        /// <para>
        /// <b>It refuses rather than approximates.</b> Where nothing offered can do everything required the
        /// result says so and names what was missing - there is no nearest match and no default, because
        /// returning a system that cannot meet the requirement would assess a building that was never
        /// designed.
        /// </para>
        /// <para>
        /// <b>And it refuses rather than guesses.</b> Where two suitable systems share the lowest rank the
        /// catalogue has not said which is preferred, so neither is returned. Breaking that tie on a name
        /// would let an alphabetical accident pick a building's plant, and refusing on ambiguity is the
        /// same rule <c>SimulationSpaceMap</c> already follows for the same reason.
        /// </para>
        /// </summary>
        public static SystemCapabilitySelection SelectPreferredCapableSystem(this IEnumerable<SystemCapabilityDescriptor> systemCapabilityDescriptors, SystemCapabilityRequirement systemCapabilityRequirement)
        {
            if (systemCapabilityRequirement == null || !systemCapabilityRequirement.IsValid)
            {
                //Nothing was asked for. Returning "the first system" would be inventing a requirement.
                return SystemCapabilitySelection.Refused("No system capability was required, so no system can be chosen.");
            }

            List<SystemCapabilityDescriptor> systemCapabilityDescriptors_Valid = [];

            //Everything the offered systems can do between them, so a refusal can say what none of them had
            //rather than merely that none of them fitted.
            SystemCapability systemCapability_Available = SystemCapability.None;

            foreach (SystemCapabilityDescriptor systemCapabilityDescriptor in systemCapabilityDescriptors ?? [])
            {
                if (systemCapabilityDescriptor != null && systemCapabilityDescriptor.IsValid)
                {
                    systemCapabilityDescriptors_Valid.Add(systemCapabilityDescriptor);
                    systemCapability_Available |= systemCapabilityDescriptor.Capabilities;
                }
            }

            if (systemCapabilityDescriptors_Valid.Count == 0)
            {
                return SystemCapabilitySelection.Refused(string.Format("No systems were offered to meet {0}.", systemCapabilityRequirement));
            }

            List<SystemCapabilityDescriptor> systemCapabilityDescriptors_Capable = CapableSystems(systemCapabilityDescriptors_Valid, systemCapabilityRequirement);

            if (systemCapabilityDescriptors_Capable.Count == 0)
            {
                SystemCapability systemCapability_Missing = systemCapabilityRequirement.Missing(systemCapability_Available);

                //Every capability exists somewhere, just never together on one system. Saying "nothing was
                //missing" would read as a contradiction, so say the real thing instead.
                string reason = systemCapability_Missing == SystemCapability.None
                    ? string.Format("No single system offers {0}, although between them the {1} systems offered do.", systemCapabilityRequirement, systemCapabilityDescriptors_Valid.Count)
                    : string.Format("No system offered provides {0}, required by {1}.", new SystemCapabilityRequirement(systemCapability_Missing), systemCapabilityRequirement);

                return SystemCapabilitySelection.Refused(reason, systemCapability_Missing);
            }

            SystemCapabilityDescriptor systemCapabilityDescriptor_Result = systemCapabilityDescriptors_Capable[0];

            //A tie on rank is only ambiguous when the two are different SYSTEMS. One system listed twice is
            //a duplicated entry, not a choice - the answer is unambiguous and reporting it as a preference
            //ambiguity would send somebody looking for the wrong defect.
            //
            //Checked across every entry at the lowest rank, not only index 1: the list is sorted by rank
            //then identity, so a genuinely different system at the same rank can sit at index 2 or later
            //behind a DUPLICATE of the preferred entry at index 1. Stopping at index 1 would then see two
            //identical identities, correctly call that a duplicate rather than an ambiguity, and never look
            //far enough to find the real alternative one place further on.
            for (int i = 1; i < systemCapabilityDescriptors_Capable.Count && systemCapabilityDescriptors_Capable[i].Rank == systemCapabilityDescriptor_Result.Rank; i++)
            {
                if (SystemCapabilityDescriptor.CompareIdentity(systemCapabilityDescriptors_Capable[i], systemCapabilityDescriptor_Result) != 0)
                {
                    return SystemCapabilitySelection.Refused(string.Format("'{0}' and '{1}' both meet {2} and are both ranked {3}, so the catalogue has not said which is preferred.", systemCapabilityDescriptor_Result.SystemTemplate, systemCapabilityDescriptors_Capable[i].SystemTemplate, systemCapabilityRequirement, systemCapabilityDescriptor_Result.Rank));
                }
            }

            return SystemCapabilitySelection.Selected(systemCapabilityDescriptor_Result);
        }
    }
}
