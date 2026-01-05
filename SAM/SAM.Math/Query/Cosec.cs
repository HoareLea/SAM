// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Cosecant
        //https://mathworld.wolfram.com/Cosecant.html
        public static double Cosec(double angle)
        {
            return 1 / System.Math.Sin(angle);
        }
    }
}
