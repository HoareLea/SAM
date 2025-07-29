using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class MergeSettings : IJSAMObject
    {
        private Dictionary<string, TypeMergeSettings> dictionary;
        
        public MergeSettings(IEnumerable<TypeMergeSettings> typeMergeSettings)
        {
            if(typeMergeSettings != null)
            {
                dictionary = new Dictionary<string, TypeMergeSettings>();

                foreach (TypeMergeSettings typeMergeSettings_Temp in typeMergeSettings)
                {
                    if(typeMergeSettings_Temp?.TypeName == null)
                    {
                        continue;
                    }

                    dictionary[typeMergeSettings_Temp.TypeName] = typeMergeSettings_Temp.Clone();
                }
            }
        }

        public MergeSettings(MergeSettings mergeSettings)
            : this(mergeSettings?.dictionary?.Values)
        {

        }

        public MergeSettings(JObject jObject)
        {
            FromJObject(jObject);
        }

        public TypeMergeSettings this[string name]
        {
            get
            {
                if(dictionary == null || !dictionary.TryGetValue(name, out TypeMergeSettings result))
                {
                    return null;
                }

                return result;
            }
        }
        
        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("TypeMergeSettings"))
            {
                JArray jArray = jObject.Value<JArray>("TypeMergeSettings");
                if(jArray != null)
                {
                    dictionary = new Dictionary<string, TypeMergeSettings>();
                    foreach(JObject jObject_TypeMergeSettings in jArray)
                    {
                        TypeMergeSettings typeMergeSettings = Core.Query.IJSAMObject<TypeMergeSettings>(jObject_TypeMergeSettings);
                        if(typeMergeSettings?.TypeName == null)
                        {
                            continue;
                        }

                        dictionary[typeMergeSettings.TypeName] = typeMergeSettings;
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
                      
            if(dictionary != null)
            {
                JArray jArray = new JArray();
                foreach(TypeMergeSettings typeMergeSettings in dictionary.Values)
                {
                    jArray.Add(typeMergeSettings.ToJObject());
                }

                jObject.Add("TypeMergeSettings", jArray);
            }

            return jObject;
        }
    }
}