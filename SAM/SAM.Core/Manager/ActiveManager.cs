using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class ActiveManager
    {
        private static Dictionary<Type, AssociatedTypes> associatedTypesDictionary;
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


        public static List<Type> GetParameterTypes()
        {
            if (associatedTypesDictionary == null)
                associatedTypesDictionary = Query.AssociatedTypesDictionary();


            List<Type> result = new List<Type>();
            foreach (KeyValuePair<Type, AssociatedTypes> keyValuePair in associatedTypesDictionary)
            {
                if (keyValuePair.Key == null || keyValuePair.Value == null)
                    continue;

                result.Add(keyValuePair.Key);
            }
            return result;
        }
        
        public static List<Type> GetParameterTypes(Type type)
        {
            if (type == null)
                return null;

            if (associatedTypesDictionary == null)
                associatedTypesDictionary = Query.AssociatedTypesDictionary();

            if (associatedTypesDictionary == null || associatedTypesDictionary.Count == 0)
                return null;

            List<Type> result = new List<Type>();
            foreach(KeyValuePair<Type, AssociatedTypes> keyValuePair in associatedTypesDictionary)
            {
                if (keyValuePair.Key == null || keyValuePair.Value == null)
                    continue;

                if (keyValuePair.Value.IsValid(type))
                    result.Add(keyValuePair.Key);
            }

            return result;
        }

        public static List<Enum> GetParameterEnums(Type type)
        {
            if (type == null)
                return null;

            List<Type> types = GetParameterTypes(type);
            if (types == null)
                return null;

            List<Enum> result = new List<Enum>();
            foreach(Type type_Temp in types)
            {
                foreach (Enum @enum in Enum.GetValues(type_Temp))
                    result.Add(@enum);
            }
            return result;
        }

        public static List<Enum> GetParameterEnums(SAMObject sAMObject)
        {
            return GetParameterEnums(sAMObject?.GetType());
        }
    }
}