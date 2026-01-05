// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Vector ToGrasshopper(this Spatial.Vector3D vector3D)
        {
            return new GH_Vector(Rhino.Convert.ToRhino(vector3D));
        }

        public static GH_Vector ToGrasshopper(this Planar.Vector2D vector2D)
        {
            return new GH_Vector(Rhino.Convert.ToRhino(vector2D));
        }
    }
}
