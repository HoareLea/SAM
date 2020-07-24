using System.Drawing;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static uint ToUint(this Color color)
        {
            return (uint)((color.R << 16) | (color.G << 8) | (color.B << 0));
        }

        public static uint ToUint(this Color color, bool includeAlpha)
        {
            if (includeAlpha)
                return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
            else
                return ToUint(color);
        }

        public static uint ToUint(this byte a, byte r, byte g, byte b)
        {
            return (uint)((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        public static uint ToUint(this byte r, byte g, byte b)
        {
            return (uint)((r << 16) | (g << 8) | (b << 0));
        }
    }
}