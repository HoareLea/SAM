using System;

namespace SAM.Core
{
    public static partial class Modify
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr intPtr);
    }
}