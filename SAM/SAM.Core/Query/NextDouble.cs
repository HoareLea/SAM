// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double NextDouble(this Random random, double min, double max)
        {
            if (random == null)
                return double.NaN;

            return random.NextDouble() * (max - min) + min;
        }
    }
}
