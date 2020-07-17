using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static void ReleaseCOMObject(this object cOMObject)
        {
            if (cOMObject == null)
                return;

            int referenceCount = 0;
            do
            {
                referenceCount = Marshal.FinalReleaseComObject(cOMObject);
            }
            while (referenceCount > 0);

            cOMObject = null;
        }
    }
}