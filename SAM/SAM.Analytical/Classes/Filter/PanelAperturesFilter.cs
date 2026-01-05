// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors


using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class PanelAperturesFilter : MultiRelationFilter<Aperture>
    {
        public PanelAperturesFilter(JObject jObject)
            : base(jObject)
        {

        }

        public PanelAperturesFilter(PanelAperturesFilter panelAperturesFilter)
            : base(panelAperturesFilter)
        {

        }

        public PanelAperturesFilter(IFilter filter)
            : base()
        {
            Filter = filter;
        }

        public override List<Aperture> GetRelatives(IJSAMObject jSAMObject)
        {
            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return null;
            }

            return panel.Apertures;
        }
    }
}
