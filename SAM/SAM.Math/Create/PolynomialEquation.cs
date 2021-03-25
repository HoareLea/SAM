using MathNet.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Math
{
    public static partial class Create
    {
        public static PolynomialEquation PolynomialEquation(IEnumerable<double> x, IEnumerable<double> y, int order = -1)
        {
            if (x == null || y == null)
                return null;

            int count = x.Count();
            if (count == 0 || y.Count() != count)
                return null;

            int order_Temp = order;
            if(order_Temp == -1)
                order_Temp = count - 1;

            Polynomial polynomial = Polynomial.Fit(x.ToArray(), y.ToArray(), order_Temp, MathNet.Numerics.LinearRegression.DirectRegressionMethod.NormalEquations);
            return polynomial?.ToSAM();
        }
    }
}
