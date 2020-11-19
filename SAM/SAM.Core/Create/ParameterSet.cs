using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ParameterSet ParameterSet(this object @object, IEnumerable<string> names)
        {
            if (@object == null || names == null)
                return null;

            return ParameterSet(@object, @object.GetType()?.Assembly, names);
        }

        public static ParameterSet ParameterSet(this object @object, Assembly assembly, IEnumerable<string> names)
        {
            if (@object == null || names == null)
                return null;

            ParameterSet result = new ParameterSet(assembly);

            foreach (string name in names)
            {
                object value = null;

                if (!Query.TryGetValue(@object, name, out value))
                    continue;

                result.Add(name, value as dynamic);
            }

            return result;
        }

        public static ParameterSet ParameterSet(this object @object, Type type_destination, TypeMap typeMap)
        {
            if (@object == null || typeMap == null)
                return null;
            
            return ParameterSet(@object, @object.GetType().Assembly, type_destination, typeMap);
        }

        public static ParameterSet ParameterSet(this object @object, Assembly assembly, Type type_destination, TypeMap typeMap)
        {
            if (@object == null || typeMap == null || assembly == null)
                return null;

            Type type_source = @object.GetType();

            List<string> names = typeMap.GetNames(type_source, type_destination.GetType());
            if (names == null)
                return null;

            ParameterSet result = new ParameterSet(assembly);

            foreach (string name in names)
            {
                string name_destination = typeMap.GetName(type_source, type_destination, name);
                if (string.IsNullOrWhiteSpace(name_destination))
                    continue;

                object value = null;

                if (!Query.TryGetValue(@object, name, out value))
                    continue;

                result.Add(name_destination, value as dynamic);
            }

            return result;
        }

        public static ParameterSet ParameterSet(this object @object, Assembly assembly, string typeName_1, string typeName_2, TypeMap typeMap)
        {
            if (@object == null || assembly == null || typeMap == null || string.IsNullOrWhiteSpace(typeName_1) || string.IsNullOrWhiteSpace(typeName_2))
                return null;

            List<string> names = typeMap.GetNames(typeName_1, typeName_2);
            if (names == null)
                return null;

            ParameterSet result = new ParameterSet(assembly);

            foreach (string name in names)
            {
                string name_destination = typeMap.GetName(typeName_1, typeName_2, name);
                if (string.IsNullOrWhiteSpace(name_destination))
                    continue;

                object value = null;

                if (!Query.TryGetValue(@object, name, out value))
                    continue;

                result.Add(name_destination, value as dynamic);
            }

            return result;
        }
    }
}