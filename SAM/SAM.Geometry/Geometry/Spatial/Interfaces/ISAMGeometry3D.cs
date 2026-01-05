// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Spatial
{
    public interface ISAMGeometry3D : ISAMGeometry
    {
        ISAMGeometry3D GetMoved(Vector3D vector3D);

        ISAMGeometry3D GetTransformed(Transform3D transform3D);
    }
}
