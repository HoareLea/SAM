namespace SAM.Core
{
    public static partial class Query
    {

        /// <summary>
        /// Creates color with corrected brightness
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="factor">Brightness correction factor between -1 and 1. Negative factor produce darker color</param>
        /// <returns>Color</returns>
        public static System.Drawing.Color Brightness(this System.Drawing.Color color, double factor)
        {
            double red = (double)color.R;
            double green = (double)color.G;
            double blue = (double)color.B;

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

            return System.Drawing.Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
    }
}