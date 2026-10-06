// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// <b>Whether this panel is adiabatic because an isolation cut it off from the space on its other
        /// side</b> - as opposed to being adiabatic because a TBD, a gbXML or a person said so.
        /// <para>
        /// Only <c>AdjacencyCluster.Filter</c> writes it, at the moment it makes the cut. Nothing infers it:
        /// where the parameter is absent the answer is no, which is what every model built before this
        /// existed correctly reports.
        /// </para>
        /// <para>
        /// This is a statement about PROVENANCE and never a licence to skip work. The cut itself is decided
        /// by comparing the two adjacency states and by nothing else; this only lets a run that can no
        /// longer make that comparison - because the model it was handed had already been isolated - still
        /// say truthfully what the model carries.
        /// </para>
        /// </summary>
        public static bool IsolationCut(this Panel panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (!panel.TryGetValue(PanelParameter.IsolationCut, out bool result))
            {
                return false;
            }

            return result;
        }
    }
}
