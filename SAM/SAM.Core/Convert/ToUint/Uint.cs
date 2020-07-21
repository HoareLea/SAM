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
                return (uint)((color.R << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
            else
                return ToUint(color);
        }
    }
}