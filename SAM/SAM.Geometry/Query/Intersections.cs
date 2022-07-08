using SAM.Geometry.Planar;
using SAM.Math;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Query
    {
        public static List<Point2D> Intersections(this PolynomialEquation polynomialEquation_1, PolynomialEquation polynomialEquation_2)
        {
            if(polynomialEquation_1 == null ||polynomialEquation_2 == null)
            {
                return null;
            }

            MathNet.Numerics.Polynomial polynomial_1 = Math.Convert.ToMathNet(polynomialEquation_1);
            if(polynomial_1 == null)
            {
                return null;
            }

            MathNet.Numerics.Polynomial polynomial_2 = Math.Convert.ToMathNet(polynomialEquation_2);
            if (polynomial_2 == null)
            {
                return null;
            }

            MathNet.Numerics.Polynomial polynomial = polynomial_1 - polynomial_2;
            if(polynomial == null)
            {
                return null;
            }

            System.Numerics.Complex[] complexes = polynomial.Roots();
            if(complexes == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            foreach(System.Numerics.Complex complex in complexes)
            {
                Point2D point2D = complex.ToSAM();
                if(point2D == null)
                {
                    continue;
                }

                result.Add(point2D);
            }

            return result;
        }
    }
}