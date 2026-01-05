// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Shell ToSAM_Shell(this GH_Extrusion ghExtrusion)
        {
            return Rhino.Convert.ToSAM_Shell(ghExtrusion?.Value);
        }
    }
}
