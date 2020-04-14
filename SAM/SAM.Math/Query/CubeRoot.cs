namespace SAM.Math
{
    public static partial class Query
    {
        private const double oneThird = 1 / 3;

        // Cube Root
        //https://wiki.unity3d.com/index.php/3d_Math_functions
        public static double CubeRoot(double value)
        {
            if (value < 0)
                return -System.Math.Pow(-value, oneThird);

            return System.Math.Pow(value, oneThird);
        }
    }
}