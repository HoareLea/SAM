// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelAdiabaticFilter : BooleanFilter, IBooleanFilter
    {
        public PanelAdiabaticFilter(bool value)
            : base(value)
        {

        }

        public PanelAdiabaticFilter(PanelAdiabaticFilter panelAdiabaticFilter)
            : base(panelAdiabaticFilter)
        {

        }

        public PanelAdiabaticFilter(JObject jObject)
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

            boolean = Query.Adiabatic(panel);

            return true;
        }
    }
}
