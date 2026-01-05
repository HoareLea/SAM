// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string ConstructionName(this PanelType panelType)
        {
            Construction construction = DefaultConstruction(panelType);
            if (construction == null)
                return null;

            return construction.Name;
        }
    }
}
