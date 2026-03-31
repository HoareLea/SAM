// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelTransparentFilter : BooleanFilter, IAnalyticalModelFilter
    {
        public PanelTransparentFilter(bool value)
            : base(value)
        {

        }

        public PanelTransparentFilter(PanelTransparentFilter panelTransparentFilter)
            : base(panelTransparentFilter)
        {

        }

        public PanelTransparentFilter(JObject jObject)
            : base(jObject)
        {

        }

        public AnalyticalModel AnalyticalModel { get; set; }

        public override bool TryGetBoolean(IJSAMObject jSAMObject, out bool boolean)
        {
            boolean = false;
            if(AnalyticalModel is null)
            {
                return false;
            }

            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            boolean = Query.Transparent(panel, AnalyticalModel.MaterialLibrary);

            return true;
        }
    }
}
