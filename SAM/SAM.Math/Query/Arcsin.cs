// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Query
    {
        // Inverse Sine
        public static double Arcsin(double angle)
        {
            return System.Math.Atan(angle / System.Math.Sqrt(-angle * angle + 1));
        }
    }
}
