using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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

        public static bool TryGetValue<T>(this object @object, string name, out T value, bool tryConvert)
        {
            if (!tryConvert)
                return TryGetValue(@object, name, out value);

            value = default;

            object object_Value = null;

            bool result = TryGetValue(@object, name, out object_Value);
            if (!result)
                return result;
                
            if(object_Value is T)
            {
                value = (T)object_Value;
                return true;
            }

            if(value is bool)
            {
                if(object_Value is string)
                {
                    bool @bool;
                    if(bool.TryParse((string)object_Value, out @bool))
                    {
                        value = (T)(object)@bool;
                        return true;
                    }
                    
                    string @string = ((string)object_Value).Trim().ToUpper();
                    value = (T)(object)(@string.Equals("1") || @string.Equals("YES") || @string.Equals("TRUE"));
                    return true;
                } 
                else if(IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToInt64(object_Value) == 1);
                    return true;
                }
            }
            else if(value is int)
            {
                if (object_Value is string)
                {
                    int @int;
                    if (int.TryParse((string)object_Value, out @int))
                    {
                        value = (T)(object)@int;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToInt32(object_Value));
                    return true;
                }
            }
            else if (value is double)
            {
                if (object_Value is string)
                {
                    double @double;
                    if (double.TryParse((string)object_Value, out @double))
                    {
                        value = (T)(object)@double;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToDouble(object_Value));
                    return true;
                }
                else if(object_Value is bool)
                {
                    double @double = 0;
                    if ((bool)object_Value)
                        @double = 1;
                    
                    value = (T)(object)@double;
                    return true;
                }
                else if(object_Value is int)
                {
                    int @int = 0;
                    if ((bool)object_Value)
                        @int = 1;

                    value = (T)(object)@int;
                    return true;
                }
            }
            else if (value is uint)
            {
                if (object_Value is string)
                {
                    uint @uint;
                    if (uint.TryParse((string)object_Value, out @uint))
                    {
                        value = (T)(object)@uint;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToUInt32(object_Value));
                    return true;
                }
            }
            else if (value is short)
            {
                if (object_Value is string)
                {
                    short @short;
                    if (short.TryParse((string)object_Value, out @short))
                    {
                        value = (T)(object)@short;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToInt16(object_Value));
                    return true;
                }
            }
            else if (value is int)
            {
                if (object_Value is string)
                {
                    int @int;
                    if (int.TryParse((string)object_Value, out @int))
                    {
                        value = (T)(object)@int;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToInt16(object_Value));
                    return true;
                }
            }
            else if (value is long)
            {
                if (object_Value is string)
                {
                    long @long;
                    if (long.TryParse((string)object_Value, out @long))
                    {
                        value = (T)(object)@long;
                        return true;
                    }
                }
                else if (IsNumeric(object_Value))
                {
                    value = (T)(object)(System.Convert.ToInt32(object_Value));
                    return true;
                }
            }
            else if(value is Guid)
            {
                if(object_Value is string)
                {
                    Guid guid;
                    if(System.Guid.TryParse((string)object_Value, out guid))
                    {
                        value = (T)(object)guid;
                        return true;
                    }
                }
            }
            else if(value is DateTime)
            {
                if (object_Value is string)
                {
                    DateTime dateTime;
                    if (System.DateTime.TryParse((string)object_Value, out dateTime))
                    {
                        value = (T)(object)dateTime;
                        return true;
                    }
                }
                else if(IsNumeric(object_Value))
                {
                    if(object_Value is double)
                        value = (T)(object)DateTime.FromOADate((double)object_Value);
                    else
                        value = (T)(object) (new DateTime(System.Convert.ToInt64(object_Value)));

                    return true;
                }
            }
            else if(typeof(IJSAMObject).IsAssignableFrom(typeof(T)))
            {
                if(object_Value is string)
                {
                    value = (T)(object)Convert.ToSAM((string)object_Value);
                    return true;
                }
            }
            else if(typeof(JObject).IsAssignableFrom(typeof(T)))
            {
                if(object_Value is string)
                {
                    value = (T)(object)JObject.Parse((string)object_Value);
                    return true;
                }
            }

            value = default;
            return false;
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

        public static bool TryGetValue(this object @object, string name, out object value, bool UserFriendlyName)
        {
            value = null;
            if (@object == null || string.IsNullOrEmpty(name))
                return false;

            List<string> names = new List<string>() { name };
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
                    if (TryGetValue_Method(@object, propertyInfo.GetMethod, out value))
                        return true;
                }
            }

            return false;
        }

        private static bool TryGetValue_Method(this object @object, System.Reflection.MethodInfo methodInfo, out object value)
        {
            value = null;

            if (@object == null || methodInfo == null || methodInfo.ContainsGenericParameters)
                return false;

            if (methodInfo.ReturnType == typeof(void))
                return false;

            object[] parameters = new object[] { };

            System.Reflection.ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            if (parameterInfos != null && parameterInfos.Length > 0)
            {
                if (!parameterInfos.ToList().TrueForAll(x => x.IsOptional))
                    return false;

                parameters = new object[parameterInfos.Length];
                for (int i = 0; i < parameters.Length; i++)
                    parameters[i] = Type.Missing;
            }

            value = methodInfo.Invoke(@object, parameters);
            return true;
        }

        private static bool TryGetValue_Method(this object @object, string name, out object value)
        {
            value = null;

            if (string.IsNullOrWhiteSpace(name) || @object == null)
                return false;

            System.Reflection.MethodInfo[] methodInfos = @object.GetType().GetMethods();
            foreach (System.Reflection.MethodInfo methodInfo in methodInfos)
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