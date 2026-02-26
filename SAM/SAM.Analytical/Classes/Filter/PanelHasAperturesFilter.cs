// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelHasAperturesFilter : BooleanFilter, IBooleanFilter
    {
        public PanelHasAperturesFilter(bool value)
            : base(value)
        {

        }

        public PanelHasAperturesFilter(PanelHasAperturesFilter panelHasApertureFilter)
            : base(panelHasApertureFilter)
        {

        }

        public PanelHasAperturesFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool TryGetBoolean(IJSAMObject jSAMObject, out bool boolean)
        {
            boolean = false;

            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            boolean = panel.HasApertures;

            return true;
        }
    }
}
