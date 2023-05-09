using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAM.Core
{
    public class ParameterizedSAMObject : IParameterizedSAMObject
    {
        private List<ParameterSet> parameterSets;

        public ParameterizedSAMObject(ParameterizedSAMObject parameterizedSAMObject)
        {
            if (parameterizedSAMObject?.parameterSets != null)
            {
                parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in parameterizedSAMObject.parameterSets)
                    parameterSets.Add(parameterSet.Clone());
            }
        }

        public ParameterizedSAMObject(IEnumerable<ParameterSet> parameterSets)
        {
            if (parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public ParameterizedSAMObject()
        {
        }

        public ParameterizedSAMObject(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool RemoveValue(Enum @enum)
        {
            if (!Query.IsValid(GetType(), @enum))
                return false;

            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if (parameterProperties == null)
                return false;

            if (!parameterProperties.WriteAccess())
                return false;

            string name = parameterProperties.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            return Modify.RemoveValue(this, name, @enum.GetType().Assembly);
        }

        public bool RemoveValue(string name, Assembly assembly = null)
        {
            return Modify.RemoveValue(this, name, assembly == null ? Assembly.GetExecutingAssembly(): assembly);
        }

        public bool TryGetValue(Enum @enum, out object value)
        {
            value = null;
            
            if (!Query.IsValid(GetType(), @enum))
                return false;

            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if (parameterProperties == null)
                return false;

            if (!parameterProperties.ReadAccess())
                return false;

            string name = parameterProperties.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            object result = null;
            if (!Query.TryGetValue(this, name, @enum.GetType().Assembly, out result))
                return false;

            value = result;
            return true;
        }

        public bool HasValue(Enum @enum)
        {
            return TryGetValue(@enum, out object value);
        }

        public bool HasParameter(Enum @enum)
        {
            if (!Query.IsValid(GetType(), @enum))
            {
                return false;
            }

            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if (parameterProperties == null)
            {
                return false;
            }

            if (!parameterProperties.ReadAccess())
            {
                return false;
            }

            return true;
        }

        public bool TryGetValue(string name, out object value)
        {
            value = null;

            object result = null;
            if (!Query.TryGetValue(this, name, out result))
                return false;

            value = result;
            return true;
        }

        public bool TryGetValue<T>(string name, out T value, bool tryConvert = true)
        {
            value = default;

            object result;
            if (!TryGetValue(name, out result))
                return false;

            if (result is T)
            {
                value = (T)result;
                return true;
            }

            if (!tryConvert)
                return false;

            return Query.TryConvert(result, out value);
        }

        public bool TryGetValue<T>(Enum @enum, out T value, bool tryConvert = true)
        {
            value = default;

            object result;
            if (!TryGetValue(@enum, out result))
                return false;

            if (result is T)
            {
                value = (T)result;
                return true;
            }

            if (!tryConvert)
                return false;

            return Query.TryConvert(result, out value);
        }
        
        public object GetValue(Enum @enum)
        {
            object result = null;
            if (!TryGetValue(@enum, out result))
                return null;

            return result;
        }

        public T GetValue<T>(Enum @enum)
        {
            T result = default;
            if (!TryGetValue(@enum, out result, true))
                return default;

            return result;
        }

        public bool SetValue(Enum @enum, object value)
        {
            if (!Query.IsValid(GetType(), @enum))
                return false;

            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if (parameterProperties == null)
                return false;

            if (!parameterProperties.WriteAccess())
                return false;

            string name = parameterProperties.Name;
            if (string.IsNullOrEmpty(name))
                return false;

            object value_Temp = value;

            Attributes.ParameterValue parameterValue = Query.CustomAttribute<Attributes.ParameterValue>(@enum);
            if (parameterValue != null)
            {
                if (!parameterValue.TryConvert(value, out value_Temp))
                    return false;
            }

            if (value_Temp == null)
            {
                switch (parameterValue.ParameterType)
                {
                    case ParameterType.IJSAMObject:
                        SAMObject sAMObject = null;
                        return Modify.SetValue((IJSAMObject)this, @enum.GetType().Assembly, name, sAMObject);

                    case ParameterType.String:
                        string @string = null;
                        return Modify.SetValue(this, @enum.GetType().Assembly, name, @string);
                }
            }

            return Modify.SetValue(this, @enum.GetType().Assembly, name, value_Temp as dynamic);
        }

        public ParameterSet GetParameterSet(string name)
        {
            return Query.ParameterSet(parameterSets, name);
        }

        public ParameterSet GetParameterSet(Assembly assembly)
        {
            return Query.ParameterSet(parameterSets, assembly);
        }

        public ParameterSet GetParameterSet(Guid guid)
        {
            return Query.ParameterSet(parameterSets, guid);
        }

        public bool Add(ParameterSet parameterSet)
        {
            if (parameterSet == null)
                return false;

            if (parameterSets == null)
                parameterSets = new List<ParameterSet>();

            return Modify.Add(parameterSets, parameterSet);
        }

        public List<ParameterSet> GetParamaterSets()
        {
            if (parameterSets == null)
                return null;
            else
                return parameterSets.ConvertAll(x => x == null ? null : new ParameterSet(x)); //Updated 25.05.2022
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            parameterSets = Create.ParameterSets(jObject.Value<JArray>("ParameterSets"));
            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName((IJSAMObject)this));

            if (parameterSets != null)
                jObject.Add("ParameterSets", Create.JArray(parameterSets));

            return jObject;
        }

        public static implicit operator JObject?(ParameterizedSAMObject parameterizedSAMObject) => parameterizedSAMObject?.ToJObject();
    }
}