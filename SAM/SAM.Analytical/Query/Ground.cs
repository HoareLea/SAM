// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Ground(this PanelType panelType)
        {
            switch (panelType)
            {
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.UndergroundCeiling:
                case Analytical.PanelType.UndergroundSlab:
                    return true;
                default:
                    return false;
            }
        }
    }
}
