// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Logarithm to base N
        //https://mathworld.wolfram.com/Logarithm.html
        public static double LogN(double x, double n)
        {
            return System.Math.Log(x) / System.Math.Log(n);
        }
    }
}
