// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelPanelTypeFilter : EnumFilter<PanelType>
    {

        public PanelPanelTypeFilter(PanelType panelType)
            : base()
        {
            Value = panelType;
        }

        public PanelPanelTypeFilter(PanelPanelTypeFilter panelPanelTypeFilter)
            : base(panelPanelTypeFilter)
        {

        }

        public PanelPanelTypeFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool TryGetEnum(IJSAMObject jSAMObject, out PanelType panelType)
        {
            panelType = PanelType.Undefined;

            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            panelType = panel.PanelType;
            return true;
        }
    }
}
