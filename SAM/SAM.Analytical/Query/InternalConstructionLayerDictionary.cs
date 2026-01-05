// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, ConstructionLayer> InternalConstructionLayerDictionary(this Space space, AdjacencyCluster adjacencyCluster, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            List<IPanel> panels = adjacencyCluster?.UpdateNormals(space, false, true, false, silverSpacing, tolerance);
            if (panels == null || panels.Count == 0)
                return null;

            Dictionary<Panel, ConstructionLayer> result = new Dictionary<Panel, ConstructionLayer>();
            foreach (IPanel panel in panels)
            {
                Panel panel_Temp = adjacencyCluster.GetObject<Panel>(panel.Guid);
                if (panel_Temp == null)
                    continue;

                ConstructionLayer constructionLayer = FirstConstructionLayer(panel_Temp, panel?.Face3D?.GetPlane()?.Normal);
                result[panel_Temp] = constructionLayer;
            }

            return result;
        }
    }
}
