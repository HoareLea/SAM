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
                    if (type_Temp == null)
                        continue;

                    if (!type_Temp.IsEnum)
                        continue;

                    object[] objects = type_Temp.GetCustomAttributes(typeof(Attributes.Types), true);
                    if (objects == null || objects.Length == 0)
                        continue;

                    Attributes.Types types_Attribute = null;
                    foreach(object @object in objects)
                    {
                        types_Attribute = @object as Attributes.Types;
                        if (types_Attribute != null)
                            break;
                    }

                    if (types_Attribute == null)
                        continue;

                    if (!types_Attribute.IsValid(type))
                        continue;

                    foreach(Enum @enum in Enum.GetValues(type_Temp))
                    {
                        if(@enum.ToString().Equals(value))
                        {
                            result.Add(@enum);
                            continue;
                        }
                        
                        string name = Attributes.ParameterName.Get(@enum);
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