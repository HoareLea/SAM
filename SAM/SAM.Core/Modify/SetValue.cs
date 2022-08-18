using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool SetValue<T>(XAttribute xAttribute, T value)
        {
            if(xAttribute == null)
            {
                return false;
            }

            string @string = string.Empty;
            if(value == null)
            {
                xAttribute.SetValue(@string);
            }

            xAttribute.SetValue(value);
            return true;
        }

        public static bool SetValue<T>(XElement xElement, string attributeName, T value)
        {
            if(xElement == null || string.IsNullOrWhiteSpace(attributeName))
            {
                return false;
            }

            xElement.SetAttributeValue(attributeName, value);
            return true;
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, string value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, Guid value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, double value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, int value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, bool value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, IJSAMObject value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, JObject value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, DateTime value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, System.Drawing.Color value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, string name, SAMColor value)
        {
            return SetValue(parameterizedSAMObject, null, name, value as object);
        }


        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, string value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, Guid value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, double value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, int value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, bool value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, IJSAMObject value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, JObject value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, DateTime value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, System.Drawing.Color value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, SAMColor value)
        {
            return SetValue(parameterizedSAMObject, assembly, name, value as object);
        }


        private static bool SetValue(this ParameterizedSAMObject parameterizedSAMObject, Assembly assembly, string name, object value)
        {
            if (parameterizedSAMObject == null || string.IsNullOrWhiteSpace(name))
                return false;

            ParameterSet parameterSet = null;

            if (assembly == null)
            {
                List<ParameterSet> parameterSets = parameterizedSAMObject.GetParamaterSets();
                if(parameterSets != null)
                {
                    foreach(ParameterSet parameterSet_Temp in parameterSets)
                    {
                        if(parameterSet_Temp.Contains(name))
                        {
                            parameterSet = parameterSet_Temp;
                            break;
                        }
                    }
                }

                if (parameterSet == null)
                {
                    parameterSet = parameterizedSAMObject.GetParameterSet(parameterizedSAMObject.GetType().Assembly);
                    if(parameterSet == null)
                    {
                        parameterSet = new ParameterSet(parameterizedSAMObject.GetType().Assembly);
                        parameterizedSAMObject.Add(parameterSet);
                    }
                }
            }
            else
            {
                parameterSet = parameterizedSAMObject.GetParameterSet(assembly);
                if (parameterSet == null)
                {
                    parameterSet = new ParameterSet(assembly);
                    parameterizedSAMObject.Add(parameterSet);
                }
            }

            if (value == null)
                return parameterSet.Add(name);

            return parameterSet.Add(name, value as dynamic);
        }
    }
}