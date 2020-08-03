using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public static partial class Modify
    {
        public static bool SetParameter(this SAMObject sAMObject, string name, string value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, Guid value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, double value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, int value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, bool value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, IJSAMObject value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, JObject value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, DateTime value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, System.Drawing.Color value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, string name, SAMColor value)
        {
            return SetParameter(sAMObject, sAMObject?.GetType().Assembly, name, value as object);
        }


        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, string value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, Guid value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, double value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, int value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, bool value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, IJSAMObject value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, JObject value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, DateTime value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, System.Drawing.Color value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }

        public static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, SAMColor value)
        {
            return SetParameter(sAMObject, assembly, name, value as object);
        }


        private static bool SetParameter(this SAMObject sAMObject, Assembly assembly, string name, object value)
        {
            if (sAMObject == null || string.IsNullOrWhiteSpace(name) || assembly == null)
                return false;

            ParameterSet parameterSet = sAMObject.GetParameterSet(assembly);
            if (parameterSet == null)
                return false;

            return parameterSet.Add(name, value as dynamic);
        }
    }
}