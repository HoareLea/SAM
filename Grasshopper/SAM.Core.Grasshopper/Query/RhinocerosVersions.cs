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