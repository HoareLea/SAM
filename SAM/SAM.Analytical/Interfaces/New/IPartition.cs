// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using SAM.Geometry.Object.Spatial;

namespace SAM.Analytical
{
    public interface IPartition : IAnalyticalObject, IParameterizedSAMObject, IFace3DObject, IBuildingElement, ISAMObject
    {
    }
}
