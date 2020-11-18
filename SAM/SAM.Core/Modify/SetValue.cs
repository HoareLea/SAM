using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool SetValue(this SAMObject sAMObject, string name, string value)
        {
            return SetValue(sAMObject, null, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, Guid value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, double value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, int value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, bool value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, IJSAMObject value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, JObject value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, DateTime value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, System.Drawing.Color value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, string name, SAMColor value)
        {
            return SetValue(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }


        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, string value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, Guid value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, double value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, int value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, bool value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, IJSAMObject value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, JObject value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, DateTime value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, System.Drawing.Color value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }

        public static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, SAMColor value)
        {
            return SetValue(sAMObject, assembly, name, value as object);
        }


        private static bool SetValue(this SAMObject sAMObject, Assembly assembly, string name, object value)
        {
            if (sAMObject == null || string.IsNullOrWhiteSpace(name))
                return false;

            ParameterSet parameterSet = null;

            if (assembly == null)
            {
                List<ParameterSet> parameterSets = sAMObject.GetParamaterSets();
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
                    parameterSet = new ParameterSet(sAMObject.GetType().Assembly);
                    sAMObject.Add(parameterSet);
                }
            }
            else
            {
                parameterSet = sAMObject.GetParameterSet(assembly);
                if (parameterSet == null)
                {
                    parameterSet = new ParameterSet(assembly);
                    sAMObject.Add(parameterSet);
                }
            }

            if (value == null)
                return parameterSet.Add(name);

            return parameterSet.Add(name, value as dynamic);
        }
    }
}