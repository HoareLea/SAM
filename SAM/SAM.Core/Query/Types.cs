using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Query
    {
        public static List<Type> Types(this IRelationFilter relationFilter)
        {
            List<Type> result = new List<Type>();
            Types(relationFilter, result);

            return result;
        }

        public static void Types(this IRelationFilter relationFilter, List<Type> types)
        {
            if (types == null)
            {
                types = new List<Type>();
            }

            types.Add(relationFilter.GetType());

            IRelationFilter relationFilter_Temp = relationFilter.Filter as IRelationFilter;
            if (relationFilter_Temp == null)
            {
                return;
            }

            Types(relationFilter_Temp, types);
        }

        public static List<Type> Types(object @object, string path)
        {
            if(@object == null || string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                return null;
            }

            Assembly assembly = null;

            try
            {
                assembly = Assembly.LoadFile(path);
            }
            catch
            {
                return null;
            }

            if(assembly == null)
            {
                return null;
            }

            List<Type> result = new List<Type>();

            Type[] types = assembly.GetTypes();
            if(types == null || types.Length == 0)
            {
                return result;
            }

            foreach (Type type in types)
            {
                if(type.IsInstanceOfType(@object))
                {
                    result.Add(type);
                }
            }

            return result;
        }
    }
}