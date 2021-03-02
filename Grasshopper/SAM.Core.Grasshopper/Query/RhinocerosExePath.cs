using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Query
    {
        public static string RhinocerosExePath(string version = null)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\McNeel\Rhinoceros", false);
            if (registryKey == null)
                return null;

            List<string> versions = registryKey.GetSubKeyNames()?.ToList();
            if (versions == null || versions.Count == 0)
                return null;

            string version_Temp = null;
            if (string.IsNullOrWhiteSpace(version))
            {
                version_Temp = versions.Last();
            }
            else
            {
                version_Temp = versions.Find(x => x.Trim().ToUpper() == version.Trim().ToUpper());
                if(version_Temp == null)
                    version_Temp = versions.Find(x => x.Trim().ToUpper().StartsWith(version.Trim().ToUpper()));

                if (version_Temp == null)
                    return null;
            }

            registryKey = registryKey.OpenSubKey(string.Format(@"{0}\Install", version_Temp), false);
            if (registryKey == null)
                return null;

            return registryKey.GetValue("ExePath") as string;
        }
    }
}