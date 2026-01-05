// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Internal(this PanelType panelType)
        {
            switch (panelType)
            {
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.Air:
                case Analytical.PanelType.Ceiling:
                case Analytical.PanelType.UndergroundCeiling:
                case Analytical.PanelType.WallInternal:
                    return true;

                default:
                    return false;
            }
        }
    }
}
