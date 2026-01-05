// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Hyperbolic Sine
        //https://mathworld.wolfram.com/HyperbolicSine.html
        public static double Sinh(double angle)
        {
            return (System.Math.Exp(angle) - System.Math.Exp(-angle)) / 2;
        }
    }
}
