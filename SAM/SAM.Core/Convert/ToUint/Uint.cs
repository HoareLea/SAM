using System.Drawing;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static uint ToUint(this Color color)
        {
            return (uint)((color.B << 16) | (color.G << 8) | (color.R << 0));
        }

        public static uint ToUint(this Color color, bool includeAlpha)
        {
            if (includeAlpha)
                return (uint)((color.A << 24) | (color.B << 16) | (color.G << 8) | (color.R << 0));
            else
                return ToUint(color);
        }

        public static uint ToUint(this byte a, byte r, byte g, byte b)
        {
            return (uint)((a << 24) | (b << 16) | (g << 8) | (r << 0));
        }

        public static uint ToUint(this byte r, byte g, byte b)
        {
            return (uint)((b << 16) | (g << 8) | (r << 0));
        }
    }
}