using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryGetValue<T>(this Dictionary<string, object> dictionary, string key, out T result)
        {
            result = default;

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

        public static bool TryGetValue<T>(this object @object, string name, out T value, bool tryConvert)
        {
            if (!tryConvert)
                return TryGetValue(@object, name, out value);

            value = default;

            object object_Value = null;

            bool result = TryGetValue(@object, name, out object_Value);
            if (!result)
                return result;

            return TryConvert(object_Value, out value);
        }

        public static bool TryGetValue(this object @object, string name, out object value)
        {
            return TryGetValue(@object, name, out value, true, true, true);
        }

        public static bool TryGetValue(this object @object, string name, out object value, bool property, bool method, bool parameterSets)
        {
            value = null;

            if (@object == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (property && TryGetValue_Property(@object, name, out value))
                return true;

            if (method && TryGetValue_Method(@object, name, out value))
                return true;

            if (parameterSets && TryGetValue_ParameterSets(@object, name, out value))
                return true;

            return false;
        }

        public static bool TryGetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, Assembly defaultAssembly, out object value, bool findAny = true)
        {
            value = null;

            if (parameterizedSAMObject == null)
                return false;

            ParameterSet parameterSet = parameterizedSAMObject.GetParameterSet(defaultAssembly);
            if (parameterSet == null)
                return false;

            if(parameterSet.Contains(name))
            {
                value = parameterSet.ToObject(name);
                return true;
            }

            if (!findAny)
                return false;

            return TryGetValue_ParameterSets(parameterizedSAMObject, name, out value);
        }

        public static bool TryGetValue(this object @object, string name, out object value, bool UserFriendlyName)
        {
            value = null;
            if (@object == null || string.IsNullOrEmpty(name))
                return false;

            List<string> names = new () { name };
            if (UserFriendlyName)
                names = Names((string)name);
            else
                names = new List<string>() { name };

            if (names == null || names.Count == 0)
                return false;

            foreach(string name_Temp in names)
            {
                if (TryGetValue(@object, name_Temp, out value))
                    return true;
            }

            return false;
        }

        public static bool TryGetValue<T>(this object @object, string name, out T value)
        {
            value = default;

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

        public static bool TryGetValue<T>(XAttribute xAttribute, out T value)
        {
            value = default;

            if(xAttribute == null)
            {
                return false;
            }

            if (typeof(T) == typeof(double))
            {
                if (string.IsNullOrWhiteSpace(xAttribute.Value))
                {
                    return false;
                }

                if (!TryConvert(xAttribute.Value, out value))
                {
                    return false;
                }

                return true;
            }

            if (typeof(T) == typeof(string))
            {
                if (!TryConvert(xAttribute?.Value, out value))
                {
                    return false;
                }

                return true;
            }

            if (typeof(T) == typeof(bool))
            {
                if (string.IsNullOrWhiteSpace(xAttribute.Value))
                {
                    return false;
                }

                if (!TryConvert(xAttribute.Value, out value))
                {
                    return false;
                }

                string value_Temp = xAttribute.Value.Trim().ToUpper();
                if (value_Temp != "TRUE" && value_Temp != "FALSE")
                {
                    return false;
                }

                value = (T)(object)(value_Temp == "TRUE");
                return true;
            }

            if (typeof(T) == typeof(int))
            {
                if (string.IsNullOrWhiteSpace(xAttribute.Value))
                {
                    return false;
                }

                if (!TryConvert(xAttribute?.Value, out value))
                {
                    return false;
                }

                return true;
            }

            if (TryConvert(xAttribute?.Value, out value))
            {
                return true;
            }

            return false;
        }

        
        private static bool TryGetValue_Property(this object @object, string name, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            PropertyInfo[] propertyInfos = @object.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                if (propertyInfo.Name.Equals(name) && propertyInfo.GetMethod != null)
                {
                    if (TryGetValue_Method(@object, propertyInfo.GetMethod, out value))
                        return true;
                }
            }

            return false;
        }

        private static bool TryGetValue_Method(this object @object, MethodInfo methodInfo, out object value)
        {
            value = null;

            if (@object == null || methodInfo == null || methodInfo.ContainsGenericParameters)
                return false;

            if (methodInfo.ReturnType == typeof(void))
                return false;

            object[] parameters = new object[] { };

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            if (parameterInfos != null && parameterInfos.Length > 0)
            {
                if (!parameterInfos.ToList().TrueForAll(x => x.IsOptional))
                    return false;

                parameters = new object[parameterInfos.Length];
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = System.Type.Missing;
            }

            value = methodInfo.Invoke(@object, parameters);
            return true;
        }

        private static bool TryGetValue_Method(this object @object, string name, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            MethodInfo[] methodInfos = @object.GetType().GetMethods();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo.ReturnType == typeof(void))
                    continue;
                
                if (methodInfo.Name.Equals(name) || (!name.StartsWith("Get") && methodInfo.Name.Equals(string.Format("Get{0}", name))))
                {
                    if (TryGetValue_Method(@object, methodInfo, out value))
                        return true;
                }
            }

            return false;
        }

        private static bool TryGetValue_ParameterSets(this object @object, string name, out object value)
        {
            value = null;

            if (!(@object is ParameterizedSAMObject))
                return false;

            IEnumerable<ParameterSet> parameterSets = ((ParameterizedSAMObject)@object).GetParameterSets();
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