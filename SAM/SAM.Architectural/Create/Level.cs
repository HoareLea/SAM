// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Architectural
{
    public static partial class Create
    {
        public static Level Level(double elevation, double tolerance = Core.Tolerance.MacroDistance)
        {
            return new Level(string.Format("Level {0}", Core.Query.Round(elevation, tolerance)), elevation);
        }
    }
}
