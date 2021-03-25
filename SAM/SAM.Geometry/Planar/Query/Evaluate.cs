using SAM.Math;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Evaluate(this PolynomialEquation polynominalEquation, double x)
        {
            if (polynominalEquation == null || double.IsNaN(x))
                return null;

            double y = polynominalEquation.Evaluate(x);
            if (double.IsNaN(y))
                return null;

            return new Point2D(x, y);
        }
   }
}