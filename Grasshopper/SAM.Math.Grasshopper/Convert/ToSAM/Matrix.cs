// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {

        public static Matrix ToSAM(this GH_Matrix matrix)
        {
            return Rhino.Convert.ToSAM(matrix?.Value);
        }
    }
}
