// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelGroundFilter : BooleanFilter, IAdjacencyClusterFilter
    {
        public PanelGroundFilter(bool value)
            : base(value)
        {

        }

        public PanelGroundFilter(PanelGroundFilter panelGroundFilter)
            : base(panelGroundFilter)
        {

        }

        public PanelGroundFilter(JObject jObject)
            : base(jObject)
        {

        }

        public AdjacencyCluster AdjacencyCluster { get; set; }

        public override bool TryGetBoolean(IJSAMObject jSAMObject, out bool boolean)
        {
            boolean = false;
            if(AdjacencyCluster is null)
            {
                return false;
            }


            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            boolean = AdjacencyCluster.Ground(panel);

            return true;
        }
    }
}
