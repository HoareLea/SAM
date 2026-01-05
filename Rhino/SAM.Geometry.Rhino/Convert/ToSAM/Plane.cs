// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Plane ToSAM(this global::Rhino.Geometry.Plane plane)
        {
            return new Spatial.Plane(plane.Origin.ToSAM(), plane.XAxis.ToSAM(), plane.YAxis.ToSAM());
        }
    }
}
