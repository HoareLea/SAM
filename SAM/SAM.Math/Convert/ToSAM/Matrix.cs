// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Convert
    {
        public static Matrix ToSAM(this MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
        {
            if (matrix == null)
                return null;

            return new Matrix(matrix.ToArray());
        }
    }
}
