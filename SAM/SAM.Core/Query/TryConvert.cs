using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryConvert(this object @object, out object result, Type type)
        {
            result = default;

            Type type_Object = @object?.GetType();
            if (type_Object == type || type == null)
            {
                result = @object;
                return true;
            }

            if (type == typeof(string))
            {
                if (@object != null)
                {
                    if(@object is IEnumerable)
                    {
                        JArray jArray = new JArray();
                        foreach(object @object_Temp in (IEnumerable)@object)
                        {
                            if (TryConvert(@object_Temp, out string value) && value != null)
                                jArray.Add(value);
                            else
                                jArray.Add(string.Empty);
                        }

                        result = jArray.ToString();
                    }
                    else if (@object is IJSAMObject)
                    {
                        result = ((IJSAMObject)@object).ToJObject()?.ToString();
                    }

                    if (result == default)
                        result = @object.ToString();
                }
                    
                return true;
            }
            else if (type == typeof(bool))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    bool @bool;
                    if (bool.TryParse((string)@object, out @bool))
                    {
                        result = @bool;
                        return true;
                    }

                    string @string = ((string)@object).Trim().ToUpper();
                    result = (@string.Equals("1") || @string.Equals("YES") || @string.Equals("TRUE"));
                    return true;
                }
                else if (IsNumeric(@object))
                {
                    result = (System.Convert.ToInt64(@object) == 1);
                    return true;
                }
            }
            else if (type == typeof(int))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    int @int;
                    if (int.TryParse((string)@object, out @int))
                    {
                        result = @int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToInt32(@object);
                    return true;
                }
                else if (@object is Enum)
                {
                    result = (int)@object;
                    return true;
                }
            }
            else if (type == typeof(double))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    double @double;
                    if (double.TryParse((string)@object, out @double))
                    {
                        result = @double;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToDouble(@object);
                    return true;
                }
                else if (@object is bool)
                {
                    double @double = 0;
                    if ((bool)@object)
                        @double = 1;

                    result = @double;
                    return true;
                }
                else if (@object is int)
                {
                    int @int = 0;
                    if ((bool)@object)
                        @int = 1;

                    result = @int;
                    return true;
                }
            }
            else if (type == typeof(uint))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    uint @uint;
                    if (uint.TryParse((string)@object, out @uint))
                    {
                        result = @uint;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToUInt32(@object);
                    return true;
                }
                else if (@object is SAMColor)
                {
                    result = Convert.ToUint(((SAMColor)@object).ToColor());
                    return true;
                }
            }
            else if (type == typeof(short))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    short @short;
                    if (short.TryParse((string)@object, out @short))
                    {
                        result = @short;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToInt16(@object);
                    return true;
                }
            }
            else if (type == typeof(byte))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    if (byte.TryParse((string)@object, out byte @byte))
                    {
                        result = @byte;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToByte(@object);
                    return true;
                }
            }
            else if (type == typeof(int))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    int @int;
                    if (int.TryParse((string)@object, out @int))
                    {
                        result = @int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToInt16(@object);
                    return true;
                }
            }
            else if (type == typeof(long))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    long @long;
                    if (long.TryParse((string)@object, out @long))
                    {
                        result = @long;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = System.Convert.ToInt32(@object);
                    return true;
                }
            }
            else if (type == typeof(Guid))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    if (System.Guid.TryParse((string)@object, out Guid guid))
                    {
                        result = guid;
                        return true;
                    }
                }
            }
            else if (type == typeof(DateTime))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    DateTime dateTime;
                    if (DateTime.TryParse((string)@object, out dateTime))
                    {
                        result = dateTime;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    if (@object is double)
                        result = DateTime.FromOADate((double)@object);
                    else
                        result = new DateTime(System.Convert.ToInt64(@object));

                    return true;
                }
            }
            else if (type == typeof(System.Drawing.Color))
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    string @string = (string)@object;
                    if (@string.StartsWith("##"))
                    {
                        result = Convert.ToColor(@string);
                        if (!result.Equals(System.Drawing.Color.Empty))
                            return true;
                    }

                    int @int;
                    if (int.TryParse(@string, out @int))
                    {
                        result = Convert.ToColor(@int);
                        return true;
                    }

                    uint @uint;
                    if (uint.TryParse(@string, out @uint))
                    {
                        result = Convert.ToColor(@uint);
                        return true;
                    }

                    result = Convert.ToColor(@string);
                    if (!result.Equals(System.Drawing.Color.Empty))
                        return true;

                }
                else if (@object is SAMColor)
                {
                    result = ((SAMColor)@object).ToColor();
                    return true;
                }
                else if (@object is int)
                {
                    result = Convert.ToColor((int)@object);
                    return true;
                }
                else if (@object is uint)
                {
                    result = Convert.ToColor((uint)@object);
                    return true;
                }
            }
            else if (typeof(IJSAMObject).IsAssignableFrom(type))
            {
                if (@object is string)
                {
                    List<IJSAMObject> sAMObjects = Convert.ToSAM((string)@object);
                    if (sAMObjects != null && sAMObjects.Count != 0)
                    {
                        IJSAMObject jSAMObject = sAMObjects.Find(x => x != null && type.IsAssignableFrom(x.GetType()));
                        if (jSAMObject != null)
                        {
                            result = jSAMObject;
                            return true;
                        }
                    }
                    else if(typeof(SAMColor).IsAssignableFrom(type))
                    {
                        if(int.TryParse((string)@object, out int int_color))
                        {
                            result = new SAMColor(Convert.ToColor(int_color));
                            return true;
                        }
                    }
                }

                if (type_Object == typeof(SAMColor))
                {
                    System.Drawing.Color color = System.Drawing.Color.Empty;
                    if (TryConvert(@object, out color))
                    {
                        if (color == System.Drawing.Color.Empty)
                        {
                            result = default;
                            return true;
                        }

                        result = new SAMColor(color);
                        return true;
                    }
                }
                
                if(type_Object == typeof(System.Drawing.Color))
                {
                    result = new SAMColor((System.Drawing.Color)@object);
                    return true;
                }
            }
            else if (typeof(JObject).IsAssignableFrom(type))
            {
                if (@object is string)
                {
                    result = JObject.Parse((string)@object);
                    return true;
                }
            }
            else if (@object is JToken)
            {
                double value;
                if (TryConvert(((JValue)@object).Value, out value))
                {
                    result = value;
                    return true;
                }
            }
            else if (result is Enum)
            {
                if (@object == null)
                    return false;

                if (@object is string)
                {
                    string @string = (string)@object;

                    Type type_Result = result.GetType();

                    Array array = System.Enum.GetValues(type_Result);
                    if (array != null)
                    {
                        foreach (Enum @enum in array)
                        {
                            if (@enum.ToString().Equals(@string))
                            {
                                result = @enum;
                                return true;
                            }
                        }
                    }

                    int @int;
                    if (int.TryParse(@string, out @int))
                    {
                        if (System.Enum.IsDefined(type, @int))
                        {
                            result = @int;
                            return true;
                        }
                    }
                }
                else if (@object is int)
                {
                    int @int = default;
                    if (System.Enum.IsDefined(result.GetType(), @int))
                    {
                        result = @int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    int @int = System.Convert.ToInt32(@object);
                    if (System.Enum.IsDefined(result.GetType(), @int))
                    {
                        result = @int;
                        return true;
                    }
                }
            }
            else if(type.IsEnum)
            {
                if(@object == null)
                {
                    return false;
                }

                if(@object is string)
                {
                    string @string = ((string)@object).Replace(" ", string.Empty).ToUpper();
                    if(string.IsNullOrEmpty(@string))
                    {
                        return false;
                    }

                    foreach (Enum @enum in System.Enum.GetValues(type))
                    {
                        string name = @enum.ToString().ToUpper();
                        if (@string.Equals(name))
                        {
                            result = @enum;
                            return true;
                        }

                        string description = Description(@enum)?.Replace(" ", string.Empty)?.ToUpper();
                        if(@string.Equals(description))
                        {
                            result = @enum;
                            return true;
                        }
                    }
                }
            }

            result = default;
            return false;
        }

        public static bool TryConvert<T>(this object @object, out T result)
        {
            result = default;

            object result_Object;
            if (!TryConvert(@object, out result_Object, typeof(T)))
                return false;

            result = (T)result_Object;
            return true;
        }
    }
}