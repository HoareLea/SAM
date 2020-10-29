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

        public static Color ToColor(this int @int, byte alpha = 255)
        {
            byte b = (byte)(@int >> 16);
            byte g = (byte)(@int >> 8);
            byte r = (byte)(@int >> 0);
            return Color.FromArgb(alpha, r, g, b);
        }

        public static Color ToColor(string @string)
        {
            if (string.IsNullOrWhiteSpace(@string))
                return Color.Empty;

            object @object = new ColorConverter().ConvertFromString(@string);
            if (!(@object is Color))
                return Color.Empty;

            return (Color)@object;
        }
    }
}