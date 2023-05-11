using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static Dictionary<Type, AssociatedTypes> AssociatedTypesDictionary(IEnumerable<Type> types = null, bool enumsOnly = true, bool notPublic = false)
        {
            Dictionary<Type, AssociatedTypes> result = new ();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == null)
                {
                    continue;
                }

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
                {
                    continue;
                }

                foreach (Type type_Temp in types_Assembly)
                {
                    if (type_Temp == null)
                    {
                        continue;
                    }

                    if (enumsOnly && !type_Temp.IsEnum)
                    {
                        continue;
                    }

                    if (!notPublic && type_Temp.IsNotPublic)
                    {
                        continue;
                    }

                    AssociatedTypes associatedTypes = AssociatedTypes.Get(type_Temp);
                    if (associatedTypes == null)
                    {
                        continue;
                    }

                    if(types != null)
                    {
                        List<Type> types_Valid = associatedTypes.ValidTypes(types);
                        if (types_Valid == null || types_Valid.Count == 0)
                        {
                            continue;
                        }
                    }

                    result[type_Temp] = associatedTypes;
                }
            }

            return result;
        }
    }
}