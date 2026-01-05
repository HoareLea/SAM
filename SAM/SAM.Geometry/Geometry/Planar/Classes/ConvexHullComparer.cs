// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    internal class ConvexHullComparer : IComparer<Point2D>
    {
        public int Compare(Point2D point2D_1, Point2D point2D_2)
        {
            if (point2D_1.X < point2D_2.X)
                return -1;
            else if (point2D_1.X > point2D_2.X)
                return +1;
            else if (point2D_1.Y < point2D_2.Y)
                return -1;
            else if (point2D_1.Y > point2D_2.Y)
                return +1;
            else
                return 0;
        }
    }
}
