// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void OffsetAperturesOnEdge(this AdjacencyCluster adjacencyCluster, double distance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null || panels.Count == 0)
                return;

            foreach (Panel panel in panels)
            {
                if (panel != null && panel.HasApertures)
                {
                    Panel panel_New = new Panel(panel);

                    panel_New.OffsetAperturesOnEdge(distance, tolerance);
                    adjacencyCluster.AddObject(panel_New);
                }
            }
        }
    }
}
