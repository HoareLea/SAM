using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TypeMergeSettings : IJSAMObject
    {
        private HashSet<string> excludedParameterNames;
        private string typeName;
        public TypeMergeSettings(string typeName, IEnumerable<string> excludedParameterNames)
        {
            this.typeName = typeName;
            this.excludedParameterNames = excludedParameterNames == null ? null : new HashSet<string>(excludedParameterNames);
        }

        public TypeMergeSettings(TypeMergeSettings mergeSettings)
            : this(mergeSettings?.typeName, mergeSettings?.excludedParameterNames)
        {

        }

        public TypeMergeSettings(JObject jObject)
        {
            FromJObject(jObject);
        }

        public TypeMergeSettings(string typeName)
        {
            this.typeName = typeName;
        }

        public HashSet<string> ExcludedParameterNames
        {
            get
            {
                return excludedParameterNames == null ? null : new HashSet<string>(excludedParameterNames);
            }
        }

        public string TypeName
        {
            get
            {
                return typeName;
            }
        }
        
        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("TypeName"))
            {
                typeName = jObject.Value<string>("TypeName");
            }

            if(jObject.ContainsKey("ExcludedParameterNames"))
            {
                excludedParameterNames = new HashSet<string>();
                foreach(string parameterName in jObject.Value<JArray>("ExcludedParameterNames"))
                {
                    excludedParameterNames.Add(parameterName);
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if (typeName != null)
            {
                jObject.Add("TypeName", typeName);
            }

            if(excludedParameterNames != null)
            {
                JArray jArray = new JArray();
                foreach(string parameterName in excludedParameterNames)
                {
                    jArray.Add(parameterName);
                }

                jObject.Add("ExcludedParameterNames", jArray);
            }

            return jObject;
        }
    }
}