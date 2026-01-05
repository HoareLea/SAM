// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Cotangent
        //https://mathworld.wolfram.com/Cotangent.html
        public static double Cotan(double angle)
        {
            return 1 / System.Math.Tan(angle);
        }
    }
}
