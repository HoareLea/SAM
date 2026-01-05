// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry.Object.Spatial;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public interface IBuildingElement : IAnalyticalObject, IParameterizedSAMObject, IFace3DObject, ISAMObject
    {
        void Transform(Transform3D transform3D);

        void Move(Vector3D vector3D);

        BoundingBox3D GetBoundingBox(double offset = 0);

    }
}
