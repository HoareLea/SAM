// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Convert
    {
        public static MathNet.Numerics.LinearAlgebra.Matrix<double> ToMathNet(this Matrix matrix)
        {
            if (matrix == null)
                return null;

            return MathNet.Numerics.LinearAlgebra.CreateMatrix.DenseOfArray(matrix.ToArray());
        }
    }
}
