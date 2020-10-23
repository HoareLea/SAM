using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core
{
    public class SAMObject : ISAMObject
    {
        private Guid guid;
        private string name;
        private List<ParameterSet> parameterSets;

        public SAMObject(SAMObject sAMObject)
        {
            guid = sAMObject.Guid;
            name = sAMObject.Name;

            if (sAMObject.parameterSets != null)
                parameterSets = sAMObject.parameterSets.Clone();
        }

        public SAMObject(string name, SAMObject sAMObject)
        {
            this.guid = sAMObject.Guid;
            this.name = name;

            if (sAMObject.parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in sAMObject.parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public SAMObject(string name, Guid guid, SAMObject sAMObject)
        {
            this.guid = guid;
            this.name = name;

            if (sAMObject.parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in sAMObject.parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public SAMObject(Guid guid, SAMObject sAMObject)
        {
            this.guid = guid;
            this.name = sAMObject.Name;

            if (sAMObject.parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in sAMObject.parameterSets)
                    this.parameterSets.Add(parameterSet.Clone());
            }
        }

        public SAMObject(Guid guid, string name, IEnumerable<ParameterSet> parameterSets)
        {
            this.guid = guid;
            this.name = name;

            if (parameterSets != null)
            {
                this.parameterSets = new List<ParameterSet>();
                foreach (ParameterSet parameterSet in parameterSets)
                {
                    ParameterSet parameterSet_Temp = parameterSet.Clone();
                    this.parameterSets.Add(parameterSet_Temp);
                }
            }
        }

        public SAMObject(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }

        public SAMObject()
        {
            guid = Guid.NewGuid();
        }

        public SAMObject(JObject jObject)
        {
            FromJObject(jObject);
        }

        public SAMObject(Guid guid)
        {
            this.guid = guid;
        }

        public SAMObject(string name)
        {
            this.name = name;
            guid = Guid.NewGuid();
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Guid Guid
        {
            get
            {
                return guid;
            }
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
            if (!Query.TryGetValue(this, name, out result))
                return false;

            value = result;
            return true;
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
                if (!parameterValue.IsValid(value))
                    return false;

                value_Temp = parameterValue.Convert(value);
            }
                
            return Modify.SetParameter(this, name, value_Temp as dynamic);
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
                return new List<ParameterSet>(parameterSets);
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            name = Query.Name(jObject);
            guid = Query.Guid(jObject);
            parameterSets = Create.ParameterSets(jObject.Value<JArray>("ParameterSets"));
            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            if (name != null)
                jObject.Add("Name", name);

            jObject.Add("Guid", guid);

            if (parameterSets != null)
                jObject.Add("ParameterSets", Create.JArray(parameterSets));

            return jObject;
        }
    }
}