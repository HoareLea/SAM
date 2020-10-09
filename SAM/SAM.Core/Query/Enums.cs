using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Enum> Enums(Type type, string value)
        {
            if (type == null || string.IsNullOrEmpty(value))
                return null;

            List<Enum> result = new List<Enum>();
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null)
                    continue;

                Type[] types = assembly.GetTypes();
                if (types == null || types.Length == 0)
                    continue;

                foreach(Type type_Temp in types)
                {
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