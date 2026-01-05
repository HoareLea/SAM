// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Linq;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Polyline3D ToSAM(this global::Rhino.Geometry.Polyline polyline)
        {
            return new Spatial.Polyline3D(polyline.ToList().ConvertAll(x => x.ToSAM()));
        }
    }
}
