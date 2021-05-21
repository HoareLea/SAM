using System.Drawing;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static string ToHexadecimal(this Color color)
        {
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }
    }
}