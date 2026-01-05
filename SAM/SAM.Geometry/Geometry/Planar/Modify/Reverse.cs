// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static void Reverse(this List<Point2D> point2Ds, bool close)
        {
            if (point2Ds == null || point2Ds.Count < 3)
                return;

            if (close)
            {
                Point2D point2D = point2Ds[0];
                point2Ds.RemoveAt(0);
                point2Ds.Reverse();
                point2Ds.Insert(0, point2D);
            }
            else
            {
                point2Ds.Reverse();
            }
        }
    }
}
