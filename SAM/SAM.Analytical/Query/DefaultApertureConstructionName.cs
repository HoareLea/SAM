// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultApertureConstructionName(this PanelType panelType, ApertureType apertureType)
        {
            ApertureConstruction apertureConstruction = DefaultApertureConstruction(panelType, apertureType);
            if (apertureConstruction == null)
                return null;

            return apertureConstruction.Name;
        }
    }
}
