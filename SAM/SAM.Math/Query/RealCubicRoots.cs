namespace SAM.Math
{
    public static partial class Query
    {
        //Solve cubic equation: ax^3 + bx^2 + cx + d = 0 according to Cardano.
        //Source: https://www.cs.rit.edu/~ark/pj/lib/edu/rit/numeric/Cubic.shtml
        public static double[] RealCubricRoots(double a, double b, double c, double d)
        {
            double two_PI = 2 * System.Math.PI;
            double four_PI = 4 * System.Math.PI;

            // Normalize coefficients.
            double denom = a;
            a = b / denom;
            b = c / denom;
            c = d / denom;

            // Commence solution.
            double a_over_3 = a / 3;
            double q = (3 * b - a * a) / 9;
            double q_Qube = q * q * q;
            double r = (9 * a * b - 27 * c - 2 * a * a * a) / 54;
            double r_Square = r * r;
            double D = q_Qube + r_Square;

            if (D < 0)
            {
                // Three unequal real roots.
                double theta = System.Math.Acos(r / System.Math.Sqrt(-q_Qube));
                double q_Sqrt = System.Math.Sqrt(-q);
                double x = 2f * q_Sqrt * System.Math.Cos(theta / 3f) - a_over_3;
                double y = 2f * q_Sqrt * System.Math.Cos((theta + two_PI) / 3f) - a_over_3;
                double z = 2f * q_Sqrt * System.Math.Cos((theta + four_PI) / 3f) - a_over_3;
                return new double[] { x, y, z };
            }
            else if (D > 0)
            {
                // One real root.
                double d_Sqrt = System.Math.Sqrt(D);
                double s = CubeRoot(r + d_Sqrt);
                double t = CubeRoot(r - d_Sqrt);
                double x = (s + t) - a_over_3;
                return new double[] { x };
            }
            else
            {
                // Three real roots, at least two equal.
                double r_CubeRoot = CubeRoot(r);
                double x = 2 * r_CubeRoot - a_over_3;
                double y = r_CubeRoot - a_over_3;
                double z = y;
                return new double[] { x, y, z };
            }
        }

        //Solve ax^3 + bx^2 + cx + d = 0 following
        //Source: http://www.code-kings.com/2013/11/cubic-equation-roots-in-csharp-code.html
        public static double[] RealCubicRoots_ThreeRootsOnly(double a, double b, double c, double d, double tolerance = Core.Tolerance.Distance)
        {
            double f = (3 * c / a - b * b / (a * a)) / 3;
            double g = (2 * System.Math.Pow(b, 3) / System.Math.Pow(a, 3) - (9 * b * c) / System.Math.Pow(a, 2) + 27 * d / a) / 27;
            double h = Core.Query.Round(System.Math.Pow(g, 2) * 0.25 + System.Math.Pow(f, 3) / 27, tolerance);

            if (h <= 0)
            {
                double i = System.Math.Pow(System.Math.Pow(g, 2) * 0.25 - h, 0.5);
                double j = CubeRoot(i);//System.Math.Pow(i, 0.333333333333333333333333);
                double k = System.Math.Acos(-g / (2 * i));
                double l = -j;
                double m = System.Math.Cos(k / 3);
                double n = System.Math.Pow(3, 0.5) * System.Math.Sin(k / 3);
                double p = -b / (3 * a);
                double x = 2 * j * System.Math.Cos(k / 3) - b / (3 * a);
                double y = l * (m + n) + p;
                double z = l * (m - n) + p;
                return new double[] { x, y, z };
            }
            else
                return null;
        }
    }
}