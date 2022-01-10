using System.Collections.Generic;
using System.Drawing;

namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Creates list of colors being linearly interpolated between color_1 and color_2 by value
        /// </summary>
        /// <param name="color_1">Start Color</param>
        /// <param name="color_2">End Color</param>
        /// <param name="count">Number of colors</param>
        /// <returns>Lerped Colors</returns>
        public static List<Color> Lerps(this Color color_1, Color color_2, int count)
        {
            if(count < 1)
            {
                return null;
            }

            List<Color> result = new List<Color>();

            double value = 0;
            for (int i = 0; i < count; i++)
            {
                result.Add(Lerp(color_1, color_2, value));
                value += 1.0 / count;
            }

            return result;
        }
    }
}