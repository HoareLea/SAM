using System.Drawing;
using System.IO;

namespace SAM.Core
{
    public static partial class Convert
    {
        public static Bitmap ToBitmap(this byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                // Clone to make sure Bitmap isn't tied to the MemoryStream's lifetime
                using (var temp = new Bitmap(ms))
                {
                    return new Bitmap(temp);
                }
            }
        }
    }
}