// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel.Types;

namespace SAM.Math.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Matrix ToGrasshopper(this Matrix matrix)
        {
            return new GH_Matrix(Rhino.Convert.ToRhino(matrix));
        }
    }
}
