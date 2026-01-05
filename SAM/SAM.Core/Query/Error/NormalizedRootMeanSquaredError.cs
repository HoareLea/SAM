// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double NormalizedRootMeanSquaredError(this List<double> m, List<double> r, double tolerance = SAM.Core.Tolerance.Distance)
        {
            double rootMeanSquaredError = RootMeanSquaredError(m, r);
            if (double.IsNaN(rootMeanSquaredError))
            {
                return rootMeanSquaredError;
            }

            double range = r.Max() - r.Min();

            if (range < tolerance)
            {
                return double.NaN;
            }

            return rootMeanSquaredError / range;

        }
    }
}
