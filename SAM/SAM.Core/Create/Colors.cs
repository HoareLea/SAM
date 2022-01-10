using System.Collections.Generic;
using System.Drawing;

namespace SAM.Core
{
    public static partial class Create
    {
        public static List<Color> Colors(this Color color, int count, double minBrightness = 0, double maxBrightness = 1)
        {
            if(color == Color.Empty || count < 1)
            {
                return null;
            }

            Color color_1 = Query.Brighten(color, maxBrightness);
            Color color_2 = Query.Brighten(color, minBrightness);

            return Query.Lerps(color_1, color_2, count);
        }
    }
}