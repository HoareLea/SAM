// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public interface IRectangle3DObject : ISAMGeometry3DObject
    {
        Rectangle3D Rectangle3D { get; }
    }
}
