// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public interface IBoundable2D : ISAMGeometry2D
    {
        BoundingBox2D GetBoundingBox(double offset = 0);
    }
}
