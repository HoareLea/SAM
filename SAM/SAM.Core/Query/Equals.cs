using System.Drawing;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool Equals(this Color value_1, Color value_2)
        {
            return value_1.R == value_2.R && value_1.G == value_2.G && value_1.B == value_2.B && value_1.A == value_2.A;
        }
    }
}