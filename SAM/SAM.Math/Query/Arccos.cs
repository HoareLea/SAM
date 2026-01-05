// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Cosine
        //https://mathworld.wolfram.com/InverseCosine.html
        public static double Arccos(double angle)
        {
            return System.Math.Atan(-angle / System.Math.Sqrt(-angle * angle + 1)) + 2 * System.Math.Atan(1);
        }
    }
}
