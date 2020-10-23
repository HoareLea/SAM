using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public static partial class Query
    {
        public static bool TryConvert<T>(this object @object, out T result)
        {
            result = default;
                
            if(@object is T)
            {
                result = (T)@object;
                return true;
            }

            if(typeof(T) == typeof(string))
            {
                if (@object != null)
                    result = (T)(object)@object.ToString();
                
                return true;
            }
            else if(typeof(T) == typeof(bool))
            {
                if(@object is string)
                {
                    bool @bool;
                    if(bool.TryParse((string)@object, out @bool))
                    {
                        result = (T)(object)@bool;
                        return true;
                    }
                    
                    string @string = ((string)@object).Trim().ToUpper();
                    result = (T)(object)(@string.Equals("1") || @string.Equals("YES") || @string.Equals("TRUE"));
                    return true;
                } 
                else if(IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToInt64(@object) == 1);
                    return true;
                }
            }
            else if(typeof(T) == typeof(int))
            {
                if (@object is string)
                {
                    int @int;
                    if (int.TryParse((string)@object, out @int))
                    {
                        result = (T)(object)@int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToInt32(@object));
                    return true;
                }
                else if(@object is Enum)
                {
                    result = (T)(object)(int)@object;
                    return true;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (@object is string)
                {
                    double @double;
                    if (double.TryParse((string)@object, out @double))
                    {
                        result = (T)(object)@double;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToDouble(@object));
                    return true;
                }
                else if(@object is bool)
                {
                    double @double = 0;
                    if ((bool)@object)
                        @double = 1;
                    
                    result = (T)(object)@double;
                    return true;
                }
                else if(@object is int)
                {
                    int @int = 0;
                    if ((bool)@object)
                        @int = 1;

                    result = (T)(object)@int;
                    return true;
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                if (@object is string)
                {
                    uint @uint;
                    if (uint.TryParse((string)@object, out @uint))
                    {
                        result = (T)(object)@uint;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToUInt32(@object));
                    return true;
                }
            }
            else if (typeof(T) == typeof(short))
            {
                if (@object is string)
                {
                    short @short;
                    if (short.TryParse((string)@object, out @short))
                    {
                        result = (T)(object)@short;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToInt16(@object));
                    return true;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (@object is string)
                {
                    int @int;
                    if (int.TryParse((string)@object, out @int))
                    {
                        result = (T)(object)@int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToInt16(@object));
                    return true;
                }
            }
            else if (typeof(T) == typeof(long))
            {
                if (@object is string)
                {
                    long @long;
                    if (long.TryParse((string)@object, out @long))
                    {
                        result = (T)(object)@long;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    result = (T)(object)(System.Convert.ToInt32(@object));
                    return true;
                }
            }
            else if(typeof(T) == typeof(Guid))
            {
                if(@object is string)
                {
                    Guid guid;
                    if(System.Guid.TryParse((string)@object, out guid))
                    {
                        result = (T)(object)guid;
                        return true;
                    }
                }
            }
            else if(typeof(T) == typeof(DateTime))
            {
                if (@object is string)
                {
                    DateTime dateTime;
                    if (DateTime.TryParse((string)@object, out dateTime))
                    {
                        result = (T)(object)dateTime;
                        return true;
                    }
                }
                else if(IsNumeric(@object))
                {
                    if(@object is double)
                        result = (T)(object)DateTime.FromOADate((double)@object);
                    else
                        result = (T)(object) (new DateTime(System.Convert.ToInt64(@object)));

                    return true;
                }
            }
            else if(typeof(IJSAMObject).IsAssignableFrom(typeof(T)))
            {
                if(@object is string)
                {
                    result = (T)(object)Convert.ToSAM((string)@object);
                    return true;
                }
            }
            else if(typeof(JObject).IsAssignableFrom(typeof(T)))
            {
                if(@object is string)
                {
                    result = (T)(object)JObject.Parse((string)@object);
                    return true;
                }
            }
            else if(result is Enum)
            {
                if (@object is string)
                {
                    string @string = (string)@object;

                    Type type = result.GetType();

                    Array array = Enum.GetValues(type);
                    if(array != null)
                    {
                        foreach(Enum @enum in array)
                        {
                            if(@enum.ToString().Equals(@string))
                            {
                                result = (T)(object)@enum;
                                return true;
                            }
                        }
                    }

                    int @int;
                    if(int.TryParse(@string, out @int))
                    {
                        if(Enum.IsDefined(type, @int))
                        {
                            result = (T)(object)@int;
                            return true;
                        }
                    }
                }
                else if (@object is int)
                {
                    int @int = default;
                    if (Enum.IsDefined(result.GetType(), @int))
                    {
                        result = (T)(object)@int;
                        return true;
                    }
                }
                else if (IsNumeric(@object))
                {
                    int @int = System.Convert.ToInt32(@object);
                    if (Enum.IsDefined(result.GetType(), @int))
                    {
                        result = (T)(object)@int;
                        return true;
                    }
                }
            }

            result = default;
            return false;
        }
    }
}