using System.Drawing;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Linearly interpolates between color_1 and color_2 by value
        /// Source: http://www.java2s.com/example/csharp/system.drawing/lerp-between-two-color.html
        /// </summary>
        /// <param name="color_1">Start Color</param>
        /// <param name="color_2">End Color</param>
        /// <param name="value">Lerp value (0-1)</param>
        /// <returns>Lerp Color</returns>
        public static Color Lerp(this Color color_1, Color color_2, double value)
        {
            double value_Temp = 1 - value;
            int a = System.Convert.ToInt32(Clamp(color_1.A * value_Temp + color_2.A * value, 0, 255));
            int r = System.Convert.ToInt32(Clamp(color_1.R * value_Temp + color_2.R * value, 0, 255));
            int g = System.Convert.ToInt32(Clamp(color_1.G * value_Temp + color_2.G * value, 0, 255));
            int b = System.Convert.ToInt32(Clamp(color_1.B * value_Temp + color_2.B * value, 0, 255));

            return Color.FromArgb(a, r, g, b);
        }
    }
}