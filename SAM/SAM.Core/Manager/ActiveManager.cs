using System;
using System.Reflection;

namespace SAM.Core
{
    public static partial class ActiveManager
    {
        private static Manager manager = Load();

        private static Manager Load()
        {
            if (manager == null)
            {
                manager = new Manager();
                manager.Read();
            }

            return manager;
        }

        public static Setting GetSetting(Assembly assembly)
        {
            return manager.GetSetting(assembly);
        }

        public static bool SetValue(Assembly assembly, string name, string value)
        {
            return manager.SetValue(assembly, name, value);
        }

        public static bool SetValue(Assembly assembly, string name, double value)
        {
            return manager.SetValue(assembly, name, value);
        }

        public static bool SetValue(Assembly assembly, string name, bool value)
        {
            return manager.SetValue(assembly, name, value);
        }

        public static bool SetValue(Assembly assembly, string name, IJSAMObject value)
        {
            return manager.SetValue(assembly, name, value);
        }

        public static bool SetValue(Assembly assembly, string name, DateTime value)
        {
            return manager.SetValue(assembly, name, value);
        }

        public static bool SetValue(string name, string value)
        {
            return manager.SetValue(name, value);
        }

        public static bool SetValue(string name, double value)
        {
            return manager.SetValue(name, value);
        }

        public static bool SetValue(string name, bool value)
        {
            return manager.SetValue(name, value);
        }

        public static bool SetValue(string name, IJSAMObject value)
        {
            return manager.SetValue(name, value);
        }

        public static bool SetValue(string name, DateTime value)
        {
            return manager.SetValue(name, value);
        }

        public static bool TryGetValue<T>(Assembly assembly, string name, out T value)
        {
            return manager.TryGetValue<T>(assembly, name, out value);
        }

        public static bool TryGetValue<T>(string name, out T value)
        {
            return manager.TryGetValue<T>(name, out value);
        }

        public static T GetValue<T>(Assembly assembly, string name)
        {
            return manager.GetValue<T>(assembly, name);
        }

        public static T GetValue<T>(string name)
        {
            return manager.GetValue<T>(name);
        }

        public static bool Write()
        {
            return manager.Write();
        }
    }
}