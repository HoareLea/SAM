// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Units
{
    public static partial class Convert
    {
        public static double ToRadians(double value)
        {
            return value * Factor.DegreesToRadians;
        }
    }
}
