// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Rectangle3D ToSAM(this GH_Rectangle rectangle)
        {
            return Rhino.Convert.ToSAM(rectangle.Value);
        }
    }
}
