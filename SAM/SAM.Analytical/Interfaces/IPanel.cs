// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Object.Spatial;
using System;

namespace SAM.Analytical
{
    public interface IPanel : IFace3DObject, IAnalyticalObject
    {
        Guid Guid { get; }

        Construction Construction { get; }

        Guid TypeGuid { get; }
    }
}
