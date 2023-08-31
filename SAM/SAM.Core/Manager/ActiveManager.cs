using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class ActiveManager
    {
        private static Dictionary<Type, AssociatedTypes> associatedTypesDictionary;
        private static Dictionary<string, TextMap> specialCharacterMapDictionary;
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
            return manager.TryGetValue(assembly, name, out value);
        }

        public static bool TryGetValue<T>(string name, out T value)
        {
            return manager.TryGetValue(name, out value);
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

        public static List<AssociatedTypes> GetAssociatedTypes()
        {
            if (associatedTypesDictionary == null)
                associatedTypesDictionary = Query.AssociatedTypesDictionary();

            List<AssociatedTypes> result = new List<AssociatedTypes>();
            foreach (KeyValuePair<Type, AssociatedTypes> keyValuePair in associatedTypesDictionary)
            {
                if (keyValuePair.Value == null || keyValuePair.Value == null)
                    continue;

                result.Add(keyValuePair.Value);
            }
            return result;
        }

        public static Dictionary<Type, AssociatedTypes> GetAssociatedTypesDictionary(IEnumerable<Type> types = null)
        {
            if (associatedTypesDictionary == null)
                associatedTypesDictionary = Query.AssociatedTypesDictionary();

            Dictionary<Type, AssociatedTypes> result = new Dictionary<Type, AssociatedTypes>();
            foreach (KeyValuePair<Type, AssociatedTypes> keyValuePair in associatedTypesDictionary)
            {
                if (types != null)
                {
                    List<Type> types_Valid = keyValuePair.Value?.ValidTypes(types);
                    if (types_Valid == null || types_Valid.Count == 0)
                        continue;
                }

                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return result;
        }

        public static TextMap GetSpecialCharacterMap(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string name_Temp = name.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (specialCharacterMapDictionary == null)
                specialCharacterMapDictionary = new Dictionary<string, TextMap>();

            TextMap result = null;
            if (specialCharacterMapDictionary.TryGetValue(name_Temp, out result))
                return result;

            string directoryName = ActiveSetting.Setting?.GetValue<string>(CoreSettingParameter.SpecialCharacterMapsDirectoryName);
            if (directoryName == null)
                return null;

            string directory = System.IO.Path.Combine(Query.ResourcesDirectory(Assembly.GetExecutingAssembly()), directoryName);
            if (!System.IO.Directory.Exists(directory))
                return null;

            string[] paths = System.IO.Directory.GetFiles(directory);
            if (paths == null || paths.Length == 0)
                return null;

            foreach(string path in paths)
            {
                if (path == null)
                    continue;

                string name_Path = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(name_Path))
                    continue;

                name_Path = name_Path.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(name_Path))
                    continue;

                if (!name_Temp.Equals(name_Path))
                    continue;

                result = Convert.ToSAM(path, SAMFileType.Json).FirstOrDefault() as TextMap;
                if (result == null)
                    continue;

                specialCharacterMapDictionary[name_Temp] = result;
                return result;
            }

            return null;
        }

        public static List<string> GetSpecialCharacterMapNames()
        {
            string directoryName = ActiveSetting.Setting?.GetValue<string>(CoreSettingParameter.SpecialCharacterMapsDirectoryName);
            if (directoryName == null)
                return null;

            string directory = System.IO.Path.Combine(Query.ResourcesDirectory(Assembly.GetExecutingAssembly()), directoryName);
            if (!System.IO.Directory.Exists(directory))
                return null;

            string[] paths = System.IO.Directory.GetFiles(directory);
            if (paths == null || paths.Length == 0)
                return null;

            List<string> result = new List<string>();
            foreach(string path in paths)
            {
                if (path == null)
                    continue;

                string name_Path = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(name_Path))
                    continue;

                name_Path = name_Path.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(name_Path))
                    continue;

                result.Add(name_Path);
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

        public static List<Enum> GetParameterEnums(Type type, string name = null)
        {
            if (type == null)
                return null;

            List<Type> types = GetParameterTypes(type);
            if (types == null)
                return null;

            List<Enum> result = new List<Enum>();
            foreach (Type type_Temp in types)
            {
                foreach (Enum @enum in Enum.GetValues(type_Temp))
                {
                    if(name == null)
                    {
                        result.Add(@enum);
                        continue;
                    }

                    if (@enum.ToString().Equals(name))
                    {
                        result.Add(@enum);
                        continue;
                    }

                    ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
                    if (parameterProperties == null)
                        continue;

                    string name_Temp = parameterProperties.Name;
                    if (string.IsNullOrEmpty(name_Temp))
                        continue;

                    if (name.Equals(name_Temp))
                        result.Add(@enum);
                }
            }
            return result;
        }

        public static List<Enum> GetParameterEnums(ParameterizedSAMObject parameterizedSAMObject, string name = null)
        {
            return GetParameterEnums(parameterizedSAMObject?.GetType(), name);
        }
    }
}