// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static List<Spatial.ISAMGeometry3D> ToSAM(this GH_Brep brep, bool simplify = true)
        {
            return Rhino.Convert.ToSAM(brep.Value, simplify);
        }
    }
}
