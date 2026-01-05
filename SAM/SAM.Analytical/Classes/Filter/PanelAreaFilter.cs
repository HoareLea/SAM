// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class PanelAreaFilter : NumberFilter
    {
        public PanelAreaFilter(NumberComparisonType numberComparisonType, double value)
            : base(numberComparisonType, value)
        {

        }

        public PanelAreaFilter(PanelAreaFilter panelAreaFilter)
            : base(panelAreaFilter)
        {

        }

        public PanelAreaFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool TryGetNumber(IJSAMObject jSAMObject, out double number)
        {
            number = double.NaN;
            Panel panel = jSAMObject as Panel;
            if (panel == null)
            {
                return false;
            }

            number = panel.GetAreaNet();
            return !double.IsNaN(number);
        }
    }
}
