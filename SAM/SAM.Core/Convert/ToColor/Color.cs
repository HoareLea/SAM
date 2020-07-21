using System.Drawing;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static Color ToColor(this uint @uint)
        {
            byte a = (byte)(@uint >> 24);
            byte b = (byte)(@uint >> 16);
            byte g = (byte)(@uint >> 8);
            byte r = (byte)(@uint >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color ToColor(this uint @uint, byte alpha)
        {
            byte b = (byte)(@uint >> 16);
            byte g = (byte)(@uint >> 8);
            byte r = (byte)(@uint >> 0);
            return Color.FromArgb(alpha, r, g, b);
        }
    }
}