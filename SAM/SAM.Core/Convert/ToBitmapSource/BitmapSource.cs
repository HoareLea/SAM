using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SAM.Core
{
    public static partial class Convert
    {
        /// <summary>
        /// Converts a Bitmap to a BitmapSource
        /// source: <see href="https://thebuildingcoder.typepad.com/blog/2018/05/scaling-a-bitmap-for-the-large-and-small-image-icons.html">Scaling a Bitmap for the Large and Small Image Icons</see>
        /// </summary>
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            
            IntPtr intPtr = bitmap.GetHbitmap();

            BitmapSource bitmapSource;

            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(intPtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Modify.DeleteObject(intPtr);
            }
            return bitmapSource;
        }

        /// <summary>
        /// Converts a BitmapImage to a BitmapSource
        /// source: <see href="https://thebuildingcoder.typepad.com/blog/2018/05/scaling-a-bitmap-for-the-large-and-small-image-icons.html">Scaling a Bitmap for the Large and Small Image Icons</see>
        /// </summary>
        public static BitmapSource ToBitmapSource(this BitmapImage bitmapImage, int width, int height)
        {
            if (bitmapImage == null)
                return null;
            
            return ToBitmapSource(ToBitmap(ToBitmap(bitmapImage), width, height));
        }
    }
}