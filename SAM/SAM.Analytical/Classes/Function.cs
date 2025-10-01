using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Analytical
{
    public class Function : IJSAMObject, IAnalyticalObject
    {
        private string name;
        private List<double> values;

        public Function(string name, IEnumerable<double> values)
        {
            this.name = name;
            this.values = values == null ? null : [.. values];
        }

        public Function(Function function)
        {
            if(function != null)
            {
                name = function.name;
                values = function.values == null ? null : [.. function.values];
            }
        }

        public Function(JObject jObject)
        {
            FromJObject(jObject);
        }

        public double Count
        {
            get
            {
                return values?.Count ?? 0;
            }
        }

        public double this[int index]
        {
            get
            {
                if(values is null || index >= values.Count)
                {
                    return double.NaN;
                }
                
                return values[index];
            }

            set
            {
                if (values is null || index >= values.Count)
                {
                    return;
                }

                values[index] = value;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Name"))
            {
                name = jObject.Value<string>("Name");
            }

            if (jObject.ContainsKey("Values"))
            {
                values = [];
                foreach (object @object in jObject.Value<JArray>("Values"))
                {
                    if (Core.Query.TryConvert(@object, out double value))
                    {
                        values.Add(value);
                    }
                }
            }

            return true;
        }

        public FunctionType GetFunctionType()
        {
            string name_Temp = name?.Trim().ToLower(); 
            if(string.IsNullOrWhiteSpace(name_Temp))
            {
                return FunctionType.Undefined;
            }

            foreach(FunctionType functionType in Enum.GetValues(typeof(FunctionType)))
            {
                if(name_Temp.Equals(functionType.ToString()))
                {
                    return functionType;
                }
            }

            return FunctionType.Other;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (name != null)
            {
                jObject.Add("Name", name);
            }

            if (values != null)
            {
                JArray jArray = [];
                foreach(double value in values)
                {
                    jArray.Add(value);
                }

                jObject.Add("Values", jArray);
            }

            return jObject;
        }

        public override string ToString()
        {
            List<string> strings = [ name ?? string.Empty];
            
            if(values != null)
            {
                foreach(double value in values)
                {
                    strings.Add(value.ToString());
                }
            }

            return string.Join(",", strings);
        }

        public override bool Equals(object obj)
        {

            return base.Equals(obj);
        }
    }
}