// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static void District(this List<double> values, double tolerance = Tolerance.Distance)
        {
            if (values == null)
            {
                return;
            }

            List<double> result = new List<double>();
            foreach (double value in values)
            {
                int index = result.FindIndex(x => System.Math.Abs(x - value) < tolerance);
                if (index != -1)
                {
                    continue;
                }

                result.Add(value);
            }

            values.Clear();
            values.AddRange(result);
        }
    }
}
