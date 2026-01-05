// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Geometry.Planar;

namespace SAM.Geometry.Object.Planar
{
    public interface ISegment2DObject : IBoundable2DObject
    {
        Segment2D Segment2D { get; }
    }
}
