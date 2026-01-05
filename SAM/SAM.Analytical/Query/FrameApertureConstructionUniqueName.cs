// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string FrameApertureConstructionUniqueName(this ApertureConstruction apertureConstruction)
        {
            return FrameApertureConstructionUniqueName(apertureConstruction?.UniqueName());
        }

        public static string FrameApertureConstructionUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return string.Format("{0} -frame", name);
        }
    }
}
