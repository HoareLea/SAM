// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Removes air panels that separate nothing. An air panel is a virtual boundary between two spaces,
        /// so one that bounds a single space, or none, carries no meaning and is dropped.
        /// </summary>
        /// <remarks>
        /// Only <see cref="PanelType.Air"/> panels are considered. A missing construction used to be treated
        /// as air-like and made a panel a removal candidate in its own right, which was unsafe: a panel's
        /// construction comes from the default construction library, and that library is read from the
        /// installed SAM resources directory rather than from anything the build produces. On any machine
        /// without that installation every panel is created with a null construction, so the old rule deleted
        /// every external boundary in the model - each bounds one space - and
        /// <see cref="Create.AdjacencyCluster(System.Collections.Generic.IEnumerable{Geometry.Spatial.Shell}, double, double, double, double, double, double, double, double)"/>
        /// returned an empty cluster. A panel with no construction is incomplete, not virtual, and removing
        /// it silently destroyed the model rather than cleaning it.
        /// </remarks>
        /// <param name="adjacencyCluster">Cluster to clean. Modified in place.</param>
        /// <returns>The guids actually removed, or null when <paramref name="adjacencyCluster"/> is null.</returns>
        public static List<System.Guid> RemoveInvalidAirPanels(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            List<System.Guid> guids = new List<System.Guid>();

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
            {
                return guids;
            }

            foreach (Panel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                if (panel.PanelType != PanelType.Air)
                {
                    continue;
                }

                List<Space> spaces = adjacencyCluster.GetSpaces(panel);
                if (spaces != null && spaces.Count > 1)
                {
                    continue;
                }

                guids.Add(panel.Guid);
            }

            return adjacencyCluster.Remove(typeof(Panel), guids);
        }
    }
}
