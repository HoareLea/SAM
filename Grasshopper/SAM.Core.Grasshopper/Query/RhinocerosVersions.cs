// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Microsoft.Win32;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static string[] RhinocerosVersions()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\McNeel\Rhinoceros", false);
            if (registryKey == null)
                return null;

            string[] result = registryKey.GetSubKeyNames();
            if (result == null)
                return null;

            return result;
        }
    }
}
