using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Dictionary<Type, ParameterTypes> ParameterTypesDictionary(IEnumerable<Type> types = null, bool enumsOnly = true, bool notPublic = false)
        {
            Dictionary<Type, ParameterTypes> result = new Dictionary<Type, ParameterTypes>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null)
                    continue;

                Type[] types_Assembly = null;
                try
                {
                    types_Assembly = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException reflectionTypeLoadException)
                {
                    types_Assembly = reflectionTypeLoadException.Types;
                }

                if (types_Assembly == null || types_Assembly.Length == 0)
                    continue;

                foreach (Type type_Temp in types_Assembly)
                {
                    if (type_Temp == null)
                        continue;

                    if (enumsOnly && !type_Temp.IsEnum)
                        continue;

                    if (!notPublic && type_Temp.IsNotPublic)
                        continue;

                    ParameterTypes parameterTypes = ParameterTypes.Get(type_Temp);
                    if (parameterTypes == null)
                        continue;

                    if(types != null)
                    {
                        List<Type> types_Valid = parameterTypes.ValidTypes(types);
                        if (types_Valid == null || types_Valid.Count == 0)
                            continue;
                    }

                    result[type_Temp] = parameterTypes;
                }
            }

            return result;
        }
    }
}