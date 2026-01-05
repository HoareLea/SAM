// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public interface ICurvable2D : IBoundable2D
    {
        List<ICurve2D> GetCurves();
    }
}
