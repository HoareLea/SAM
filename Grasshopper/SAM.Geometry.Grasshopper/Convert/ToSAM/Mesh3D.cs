// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Mesh3D ToSAM(this GH_Mesh ghMesh)
        {
            return Rhino.Convert.ToSAM(ghMesh?.Value);
        }
    }
}
