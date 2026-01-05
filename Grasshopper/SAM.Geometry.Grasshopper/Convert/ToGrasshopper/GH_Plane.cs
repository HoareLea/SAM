// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Plane ToGrasshopper(this Spatial.Plane plane)
        {
            return new GH_Plane(Rhino.Convert.ToRhino(plane));
        }
    }
}
