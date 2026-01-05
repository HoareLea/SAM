// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public interface IIntersectionResult3D
    {
        bool Intersecting { get; }

        List<ISAMGeometry3D> Geometry3Ds { get; }
    }
}
