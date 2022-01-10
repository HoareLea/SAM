using System.Drawing;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Color Brighten(this Color color, double factor)
        {
            double red = color.R;
            double green = color.G;
            double blue = color.B;

            if (factor < 0)
            {
                factor = 1 + factor;
                red *= factor;
                green *= factor;
                blue *= factor;
            }
            else
            {
                red = (255 - red) * factor + red;
                green = (255 - green) * factor + green;
                blue = (255 - blue) * factor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
    }
}