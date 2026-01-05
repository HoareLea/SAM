// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static bool Normalize(this AdjacencyCluster adjacencyCluster, bool includeApertures = true, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return false;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
            {
                return false;
            }

            foreach (Panel panel in panels)
            {
                panel.Normalize(includeApertures, orientation, edgeOrientationMethod, tolerance_Angle, tolerance_Distance);
                adjacencyCluster.AddObject(panel);
            }

            return true;
        }
    }
}
