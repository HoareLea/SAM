// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Planar
{
    public interface ISAMGeometry2D : ISAMGeometry
    {
        ISAMGeometry2D GetTransformed(ITransform2D transform2D);

        bool Transform(ITransform2D transform2D);
    }
}
