// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Circle3D ToSAM(this GH_Circle circle)
        {
            return Rhino.Convert.ToSAM(circle.Value);
        }
    }
}
