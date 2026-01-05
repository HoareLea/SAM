// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Line ToGrasshopper(this Spatial.Segment3D segment3D)
        {
            return new GH_Line(Rhino.Convert.ToRhino(segment3D));
        }

        public static GH_Line ToGrasshopper(this Planar.Segment2D segment2D)
        {
            return new GH_Line(Rhino.Convert.ToRhino(segment2D));
        }
    }
}
