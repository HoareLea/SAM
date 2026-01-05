// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical
{
    public interface ISpace : ISAMGeometry3DObject, IAnalyticalObject
    {
        Point3D Location { get; }

        string Name { get; }

        Guid Guid { get; }
    }
}
