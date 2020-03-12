using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json.Linq;


namespace SAM.Core
{
    public class Manager : IJSAMObject
    {
        private string mainDirectoryPath;
        private string sAMDirectoryName;
        private string settingsFileName;

        private List<Setting> settings;

        public Manager()
        {
            mainDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            sAMDirectoryName = "SAM";
            settingsFileName = "settings";
        }

        public Manager(string mainDirectoryPath, string sAMDirectoryName, string settingsFileName)
        {
            this.mainDirectoryPath = mainDirectoryPath;
            this.sAMDirectoryName = sAMDirectoryName;
            this.settingsFileName = settingsFileName;
        }

        public string SettingsFilePath
        {
            get
            {
                return System.IO.Path.Combine(SAMDirectoryPath, settingsFileName);
            }
        }

        public string SAMDirectoryPath
        {
            get
            {
                return System.IO.Path.Combine(mainDirectoryPath, sAMDirectoryName);
            }
        }

        public string MainDirectoryPath
        {
            get
            {
                return mainDirectoryPath;
            }
        }

        public T GetValue<T>(Assembly assembly, string name)
        {
            T value = default;
            TryGetValue<T>(assembly, name, out value);
            return value;
        }

        public T GetValue<T>(string name)
        {
            return GetValue<T>(GetType().Assembly, name);
        }

        public bool TryGetValue<T>(Assembly assembly, string name, out T value)
        {
            value = default;
            
            Setting setting = GetSetting(assembly);
            ParameterSet parameterSet = setting.GetParameterSet(assembly);
            if (parameterSet == null)
                return false;

            if (!parameterSet.Contains(name))
                return false;

            object @object = parameterSet.ToObject(name);
            if (@object == null)
                return false;

            if (@object is T)
            {
                value = (T)@object;
                return true;
            }

            return false;
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            return TryGetValue<T>(GetType().Assembly, name, out value);
        }

        public bool SetValue(Assembly assembly, string name, string value)
        {
            return SetValue(assembly, name, value as object);
        }

        public bool SetValue(Assembly assembly, string name, double value)
        {
            return SetValue(assembly, name, value as object);
        }

        public bool SetValue(Assembly assembly, string name, IJSAMObject value)
        {
            return SetValue(assembly, name, value as object);
        }

        public bool SetValue(Assembly assembly, string name, bool value)
        {
            return SetValue(assembly, name, value as object);
        }

        public bool SetValue(Assembly assembly, string name, DateTime value)
        {
            return SetValue(assembly, name, value as object);
        }

        public bool SetValue(string name, string value)
        {
            return SetValue(GetType().Assembly, name, value as object);
        }

        public bool SetValue(string name, double value)
        {
            return SetValue(GetType().Assembly, name, value as object);
        }

        public bool SetValue(string name, IJSAMObject value)
        {
            return SetValue(GetType().Assembly, name, value as object);
        }

        public bool SetValue(string name, bool value)
        {
            return SetValue(GetType().Assembly, name, value as object);
        }

        public bool SetValue(string name, DateTime value)
        {
            return SetValue(GetType().Assembly, name, value as object);
        }

        public Setting GetSetting(Assembly assembly)
        {
            if (assembly == null)
                return null;

            Guid guid = Query.Guid(assembly);
            string name = Query.Name(assembly);

            if (settings == null)
                settings = new List<Setting>();

            Setting setting = settings.Find(x => x.Guid.Equals(guid) && x.Name.Equals(name));
            if (setting == null)
                setting = settings.Find(x => x.Name.Equals(name));

            if(setting == null)
            {
                setting = new Setting(assembly);
                settings.Add(setting);
            }

            return setting;
        }

        public Setting GetSetting()
        {
            return GetSetting(GetType().Assembly);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("settings"))
                settings = Create.IJSAMObjects<Setting>(jObject.Value<JArray>("settings"));

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            if(settings != null)

            jObject.Add("settings", Create.JArray(settings));

            return jObject;
        }

        public void Clear(Assembly assembly)
        {
            if (settings == null || settings.Count == 0)
                return;
            
            Guid guid = Query.Guid(assembly);
            string name = Query.Name(assembly);

            Setting setting = settings.Find(x => x.Guid.Equals(guid) && x.Name.Equals(name));
            if (setting == null)
                setting = settings.Find(x => x.Name.Equals(name));

            if (setting != null)
                settings.Remove(setting);
        }

        public void Clear()
        {
            Clear(GetType().Assembly);
        }

        public void ClearAll()
        {
            settings = null;
        }

        public bool Read()
        {
            try
            {


                string settingsFilePath = SettingsFilePath;
                if(System.IO.File.Exists(settingsFilePath))
                    settings = new SAMJsonCollection<Setting>(settingsFilePath).ToList();
            }
            catch(Exception exception)
            {
                return false;
            }
            
            return true;
        }

        public bool Write()
        {
            if (!System.IO.Directory.Exists(mainDirectoryPath))
                return false;

            string sAMDirectoryPath = SAMDirectoryPath;

            try
            {
                if (!System.IO.Directory.Exists(sAMDirectoryPath))
                    System.IO.Directory.CreateDirectory(sAMDirectoryPath);

                if(settings != null)
                {
                    new SAMJsonCollection<Setting>(settings).ToJson(SettingsFilePath);
                }


            }
            catch(Exception exception)
            {
                return false;
            }

            return true;
        }


        private bool SetValue(Assembly assembly, string name, object value)
        {
            if (assembly == null || string.IsNullOrWhiteSpace(name))
                return false;

            Setting setting = GetSetting(assembly);
            ParameterSet parameterSet = setting.GetParameterSet(assembly);
            if (parameterSet == null)
            {
                parameterSet = new ParameterSet(assembly);
                setting.Add(parameterSet);
            }

            if (value == null)
            {
                parameterSet.Remove(name);
                return true;
            }
                
            return parameterSet.Add(name, value as dynamic);
        }
    }
}
