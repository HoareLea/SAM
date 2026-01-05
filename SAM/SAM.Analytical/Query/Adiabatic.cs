// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Adiabatic(this Panel panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (panel.PanelType == Analytical.PanelType.Air || panel.PanelType == Analytical.PanelType.Shade)
            {
                return false;
            }

            if (Adiabatic(panel.Construction))
            {
                return true;
            }

            if (!panel.TryGetValue(PanelParameter.Adiabatic, out bool result))
            {
                return false;
            }

            return result;
        }

        public static bool Adiabatic(this Construction construction)
        {
            if (construction == null)
            {
                return false;
            }

            double thickness = construction.GetThickness();

            return thickness == 0 || double.IsNaN(thickness);
        }
    }
}
