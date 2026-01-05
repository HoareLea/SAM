// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Rectangle ToGrasshopper(this Spatial.Rectangle3D rectangle3D)
        {
            if (rectangle3D == null)
            {
                return null;
            }

            return new GH_Rectangle(Rhino.Convert.ToRhino(rectangle3D));
        }
    }
}
