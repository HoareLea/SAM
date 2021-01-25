using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace SAM.Core
{
    public static partial class Convert
    {
        /// <summary>
        /// Convert a BitmapImage to Bitmap
        /// source: <see href="https://thebuildingcoder.typepad.com/blog/2018/05/scaling-a-bitmap-for-the-large-and-small-image-icons.html">Scaling a Bitmap for the Large and Small Image Icons</see>
        /// </summary>
        public static Bitmap ToBitmap(this BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
                return null;
            
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                bitmapEncoder.Save(memoryStream);
                Bitmap bitmap = new Bitmap(memoryStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// Converts the image to Bitmap with the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="compositingMode">Value that specifies how composited images are drawn to this System.Drawing.Graphics</param>
        /// <param name="compositingQuality">The rendering quality of composited images drawn to this System.Drawing.Graphics</param>
        /// <param name="interpolationMode">The interpolation mode associated with this System.Drawing.Graphics</param>
        /// <param name="smoothingMode">The rendering quality for this System.Drawing.Graphics</param>
        /// <param name="pixelOffsetMode">Value specifying how pixels are offset during rendering of this</param>
        /// <returns>Converted Bitmap image with given size</returns>
        public static Bitmap ToBitmap(
            this Image image, 
            int width, 
            int height, 
            CompositingMode compositingMode = CompositingMode.SourceCopy, 
            CompositingQuality compositingQuality = CompositingQuality.HighQuality,
            InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic,
            SmoothingMode smoothingMode = SmoothingMode.HighQuality,
            PixelOffsetMode pixelOffsetMode = PixelOffsetMode.HighQuality
            )
        {
            if (image == null)
                return null;
            
            Rectangle rectangle = new Rectangle(0, 0, width, height);

            Bitmap result = new Bitmap(width, height);
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.CompositingMode = compositingMode;
                graphics.CompositingQuality = compositingQuality;
                graphics.InterpolationMode = interpolationMode;
                graphics.SmoothingMode = smoothingMode;
                graphics.PixelOffsetMode = pixelOffsetMode;

                using (ImageAttributes imageAttributes = new ImageAttributes())
                {
                    imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, rectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }

            result.MakeTransparent();

            return result;
        }
    }
}