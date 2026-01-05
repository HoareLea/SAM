// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

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
