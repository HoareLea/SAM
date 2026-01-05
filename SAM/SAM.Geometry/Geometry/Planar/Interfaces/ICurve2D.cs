// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public interface ICurve2D : ISAMGeometry2D, IBoundable2D
    {
        Point2D GetStart();

        Point2D GetEnd();

        double GetLength();

        void Reverse();
    }
}
