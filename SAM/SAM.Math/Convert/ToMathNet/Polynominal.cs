// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Convert
    {
        public static MathNet.Numerics.Polynomial ToMathNet(this PolynomialEquation polynomialEquation)
        {
            if (polynomialEquation == null)
                return null;

            return new MathNet.Numerics.Polynomial(polynomialEquation.Coefficients);
        }
    }
}
