using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetValue<T>(this Dictionary<string, object> dictionary, string key, out T result)
        {
            result = default(T);

            if (dictionary == null || key == null)
                return false;

            object value;
            if (!dictionary.TryGetValue(key, out value))
                return false;

            if (value == null)
                return false;

            if (!typeof(T).IsAssignableFrom(value.GetType()))
                return false;

            result = (T)(object)(value);
            return true;
        }

        public static bool TryGetValue(this object @object, string name, out object value)
        {
            value = null;

            if (@object == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (TryGetValue_Property(@object, name, out value))
                return true;

            if (TryGetValue_Method(@object, name, out value))
                return true;

            if (TryGetValue_PropertySets(@object, name, out value))
                return true;

            return false;
        }

        public static bool TryGetValue<T>(this object @object, string name, out T value)
        {
            value = default(T);

            object object_value = null;
            if (!TryGetValue(@object, name, out object_value))
                return false;

            if (object_value == null)
                return Nullable.GetUnderlyingType(typeof(T)) != null;

            if(typeof(T).IsAssignableFrom(object_value.GetType()))
            {
                value = (T)object_value;
                return true;
            }

            return false;
        }

        private static bool TryGetValue_Property(this object @object, string name, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            System.Reflection.PropertyInfo[] propertyInfos = @object.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name.Equals(name) && propertyInfo.GetMethod != null)
                {
                    value = propertyInfo.GetValue(@object);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValue_Method(this object @object, string name, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            System.Reflection.MethodInfo[] methodInfos = @object.GetType().GetMethods();
            foreach (System.Reflection.MethodInfo methodInfo in methodInfos)
            {
                object[] parameters = new object[] { };

                System.Reflection.ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos != null && parameterInfos.Length > 0)
                {
                    if (!parameterInfos.ToList().TrueForAll(x => x.IsOptional))
                        continue;

                    parameters = new object[parameterInfos.Length];
                    for (int i = 0; i < parameters.Length; i++)
                        parameters[i] = System.Type.Missing;
                }

                if (methodInfo.Name.Equals(name) || (!name.StartsWith("Get") && methodInfo.Name.Equals(string.Format("Get{0}", name))))
                {
                    value = methodInfo.Invoke(@object, parameters);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValue_PropertySets(this object @object, string name, out object value)
        {
            value = null;

            if (!(@object is SAMObject))
                return false;

            IEnumerable<ParameterSet> parameterSets = ((SAMObject)@object).GetParamaterSets();
            if (parameterSets == null || parameterSets.Count() == 0)
                return false;

            foreach (ParameterSet parameterSet in parameterSets)
            {
                if (parameterSet.Contains(name))
                {
                    value = parameterSet.ToObject(name);
                    return true;
                }
            }

            return false;
        }
    }
}