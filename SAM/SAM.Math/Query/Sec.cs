// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Secant
        //https://mathworld.wolfram.com/Secant.html
        public static double Sec(double angle)
        {
            return 1 / System.Math.Cos(angle);
        }
    }
}
