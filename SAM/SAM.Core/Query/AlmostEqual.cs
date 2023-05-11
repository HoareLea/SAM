using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool AlmostEqual(this double value_1, double value_2, double tolerance = Core.Tolerance.Distance)
        {
            bool isNaN_1 = double.IsNaN(value_1);
            bool isNaN_2 = double.IsNaN(value_2);

            if (isNaN_1 && isNaN_2)
            {
                return true;
            }

            if (isNaN_1 || isNaN_2)
            {
                return false;
            }

            return Math.Abs(value_1 - value_2) <= tolerance;
        }
    }
}