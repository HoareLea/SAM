namespace SAM.Math.TODO
{


    public  static partial class Query
    {
        // Secant 
        public static double Sec(double x)
        {
            return 1 / System.Math.Cos(x);
        }

        // Cosecant
        public static double Cosec(double x)
        {
            return 1 / System.Math.Sin(x);
        }

        // Cotangent 
        public static double Cotan(double x)
        {
            return 1 / System.Math.Tan(x);
        }

        // Inverse Sine 
        public static double Arcsin(double x)
        {
            return System.Math.Atan(x / System.Math.Sqrt(-x * x + 1));
        }

        // Inverse Cosine 
        public static double Arccos(double x)
        {
            return System.Math.Atan(-x / System.Math.Sqrt(-x * x + 1)) + 2 * System.Math.Atan(1);
        }


        // Inverse Secant 
        public static double Arcsec(double x)
        {
            return 2 * System.Math.Atan(1) - System.Math.Atan(System.Math.Sign(x) / System.Math.Sqrt(x * x - 1));
        }

        // Inverse Cosecant 
        public static double Arccosec(double x)
        {
            return System.Math.Atan(System.Math.Sign(x) / System.Math.Sqrt(x * x - 1));
        }

        // Inverse Cotangent 
        public static double Arccotan(double x)
        {
            return 2 * System.Math.Atan(1) - System.Math.Atan(x);
        }

        // Hyperbolic Sine 
        public static double HSin(double x)
        {
            return (System.Math.Exp(x) - System.Math.Exp(-x)) / 2;
        }

        // Hyperbolic Cosine 
        public static double HCos(double x)
        {
            return (System.Math.Exp(x) + System.Math.Exp(-x)) / 2;
        }

        // Hyperbolic Tangent 
        public static double HTan(double x)
        {
            return (System.Math.Exp(x) - System.Math.Exp(-x)) / (System.Math.Exp(x) + System.Math.Exp(-x));
        }

        // Hyperbolic Secant 
        public static double HSec(double x)
        {
            return 2 / (System.Math.Exp(x) + System.Math.Exp(-x));
        }

        // Hyperbolic Cosecant 
        public static double HCosec(double x)
        {
            return 2 / (System.Math.Exp(x) - System.Math.Exp(-x));
        }

        // Hyperbolic Cotangent 
        public static double HCotan(double x)
        {
            return (System.Math.Exp(x) + System.Math.Exp(-x)) / (System.Math.Exp(x) - System.Math.Exp(-x));
        }

        // Inverse Hyperbolic Sine 
        public static double HArcsin(double x)
        {
            return System.Math.Log(x + System.Math.Sqrt(x * x + 1));
        }

        // Inverse Hyperbolic Cosine 
        public static double HArccos(double x)
        {
            return System.Math.Log(x + System.Math.Sqrt(x * x - 1));
        }

        // Inverse Hyperbolic Tangent 
        public static double HArctan(double x)
        {
            return System.Math.Log((1 + x) / (1 - x)) / 2;
        }

        // Inverse Hyperbolic Secant 
        public static double HArcsec(double x)
        {
            return System.Math.Log((System.Math.Sqrt(-x * x + 1) + 1) / x);
        }

        // Inverse Hyperbolic Cosecant 
        public static double HArccosec(double x)
        {
            return System.Math.Log((System.Math.Sign(x) * System.Math.Sqrt(x * x + 1) + 1) / x);
        }

        // Inverse Hyperbolic Cotangent 
        public static double HArccotan(double x)
        {
            return System.Math.Log((x + 1) / (x - 1)) / 2;
        }

        // Logarithm to base N 
        public static double LogN(double x, double n)
        {
            return System.Math.Log(x) / System.Math.Log(n);
        }
    }
}
