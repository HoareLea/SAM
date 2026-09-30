// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    /// <summary>
    /// The outcome of <c>Query.PartOSystemsMaterialisationScope</c>: which ventilation systems a Part O Systems-route
    /// materialisation (SAM_Systems <c>Create.MechanicalVentilation</c>) may be handed - decided by identity and by
    /// nothing else - or why none may.
    ///
    /// <para><b>Fail closed, with no partial-scope cluster</b></para>
    /// <para>
    /// <see cref="AdjacencyCluster"/> is null whenever <see cref="Refusals"/> is non-empty, and the retained, removed
    /// and excluded lists are then empty too. A caller that reads the working copy without reading the refusals cannot
    /// materialise a design that leaves a real mechanical duty out.
    /// </para>
    ///
    /// <para><b>The working copy is a copy, and the model is untouched</b></para>
    /// <para>
    /// The systems out of scope are removed from a copy of the cluster, never from the model supplied. Only the Systems
    /// materialisation input is scoped: the thermal model a caller simulates keeps every authored system, opening and
    /// infiltration, and nothing here changes it.
    /// </para>
    /// </summary>
    public class PartOSystemsMaterialisationScope
    {
        private readonly List<Guid> guids_Retained = [];

        private readonly List<Guid> guids_Removed = [];

        private readonly List<PartOSystemsScopeExclusion> exclusions = [];

        private readonly List<PartOSystemsScopeRefusal> refusals = [];

        private readonly string note_Summary;

        internal PartOSystemsMaterialisationScope(
            AdjacencyCluster adjacencyCluster,
            IEnumerable<Guid> guids_Retained,
            IEnumerable<Guid> guids_Removed,
            IEnumerable<PartOSystemsScopeExclusion> exclusions,
            string note_Summary,
            IEnumerable<PartOSystemsScopeRefusal> refusals)
        {
            foreach (PartOSystemsScopeRefusal refusal in refusals ?? [])
            {
                if (refusal is not null)
                {
                    this.refusals.Add(refusal);
                }
            }

            //Fail closed, structurally - see the class summary.
            if (this.refusals.Count != 0)
            {
                return;
            }

            AdjacencyCluster = adjacencyCluster;

            this.guids_Retained.AddRange(guids_Retained ?? []);
            this.guids_Removed.AddRange(guids_Removed ?? []);

            foreach (PartOSystemsScopeExclusion exclusion in exclusions ?? [])
            {
                if (exclusion is not null)
                {
                    this.exclusions.Add(exclusion);
                }
            }

            this.note_Summary = note_Summary;
        }

        /// <summary>
        /// The working copy to hand to the Systems materialisation, or <b>null</b> whenever <see cref="Refusals"/> is
        /// non-empty. Never the cluster supplied.
        /// </summary>
        public AdjacencyCluster AdjacencyCluster { get; }

        /// <summary>The systems kept - exactly the ones Part O built, by identity, in guid order.</summary>
        public List<Guid> Guids_Retained => [.. guids_Retained];

        /// <summary>The authored systems left out of the working copy, by identity, in guid order.</summary>
        public List<Guid> Guids_Removed => [.. guids_Removed];

        /// <summary>Each authored system left out and why, in the order the model was inspected.</summary>
        public List<PartOSystemsScopeExclusion> Exclusions => [.. exclusions];

        /// <summary>Why no scope could be taken. Ordered, and never empty on a refusal.</summary>
        public List<PartOSystemsScopeRefusal> Refusals => [.. refusals];

        /// <summary>
        /// One sentence per system left out, then a summary - empty on a refusal.
        /// </summary>
        public List<string> Notes
        {
            get
            {
                List<string> result = exclusions.ConvertAll(x => x.Message);
                if (!string.IsNullOrWhiteSpace(note_Summary))
                {
                    result.Add(note_Summary);
                }

                return result;
            }
        }

        /// <summary>Every refusal message, for a caller that only shows text; null where the scope was taken.</summary>
        public string Refusal => refusals.Count == 0 ? null : string.Join(" ", refusals.ConvertAll(x => x.Message));

        /// <summary>Whether a working copy was produced at all.</summary>
        public bool IsScoped => AdjacencyCluster is not null && refusals.Count == 0;

        public override string ToString()
        {
            return IsScoped
                ? string.Format("Part O Systems scope: {0} retained, {1} removed.", guids_Retained.Count, guids_Removed.Count)
                : string.Format("Part O Systems scope REFUSED ({0} reason(s)).", refusals.Count);
        }
    }
}
