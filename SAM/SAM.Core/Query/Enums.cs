using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Enum> Enums(Type type, string value, bool notPublic = false)
        {
            if (type == null || string.IsNullOrEmpty(value))
                return null;

            List<Enum> result = new List<Enum>();
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null)
                    continue;

                Type[] types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    types = reflectionTypeLoadException.Types;
                }
                    
                if (types == null || types.Length == 0)
                    continue;

                foreach(Type type_Temp in types)
                {
                    if (type_Temp == null)
                        continue;

                    if (!notPublic && type_Temp.IsNotPublic)
                        continue;
                    
                    ParameterTypes parameterTypes = ParameterTypes.Get(type_Temp);
                    if (parameterTypes == null)
                        continue;

                    if (!parameterTypes.IsValid(type))
                        continue;

                    foreach(Enum @enum in Enum.GetValues(type_Temp))
                    {
                        if(@enum.ToString().Equals(value))
                        {
                            result.Add(@enum);
                            continue;
                        }

                        ParameterProperties parameterProperties = ParameterProperties.Get(@enum);
                        if (parameterProperties == null)
                            continue;

                        string name = parameterProperties.Name;
                        if (string.IsNullOrEmpty(name))
                            continue;

                        if (name.Equals(value))
                            result.Add(@enum);
                    }
                }
            }

            return result;
        }
    }
}