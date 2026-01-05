// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Space> InRange(this IEnumerable<Space> spaces, Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            if (shell == null || spaces == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach (Space space in spaces)
            {
                Point3D point3D = space?.Location;
                if (point3D == null)
                {
                    continue;
                }

                if (shell.InRange(point3D, tolerance))
                {
                    result.Add(space);
                }

            }

            return result;
        }
    }
}
