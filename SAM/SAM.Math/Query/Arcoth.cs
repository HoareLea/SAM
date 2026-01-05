// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Hyperbolic Cotangent  (Latin: Area cotangens hyperbolicus)
        //https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions#asinh
        //https://mathworld.wolfram.com/InverseHyperbolicCotangent.html
        public static double Arcoth(double x)
        {
            return System.Math.Log((x + 1) / (x - 1)) / 2;
        }
    }
}
