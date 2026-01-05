// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class SpacePanelsFilter : MultiRelationAdjacencyClusterFilter<Panel>
    {
        public SpacePanelsFilter(IFilter filter)
            : base(filter)
        {

        }

        public SpacePanelsFilter(SpacePanelsFilter spacePanelsFilter)
            : base(spacePanelsFilter)
        {

        }

        public SpacePanelsFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override List<Panel> GetRelatives(IJSAMObject jSAMObject)
        {
            Space space = (jSAMObject as Space);
            if (space == null)
            {
                return null;
            }

            return AdjacencyCluster.GetPanels(space);
        }
    }
}
