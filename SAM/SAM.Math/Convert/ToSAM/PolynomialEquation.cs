// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Math
{
    public static partial class Convert
    {
        public static PolynomialEquation ToSAM(this MathNet.Numerics.Polynomial polynomial)
        {
            if (polynomial == null)
                return null;

            return new PolynomialEquation(polynomial.Coefficients);
        }
    }
}
