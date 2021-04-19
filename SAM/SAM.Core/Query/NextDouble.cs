using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static double NextDouble(this Random random, double min, double max)
        {
            if (random == null)
                return double.NaN;

            return random.NextDouble() * (max - min) + min;
        }
    }
}