using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class TextMapCluster : SAMObject, IJSAMObject
    {
        private Dictionary<string, HashSet<string>> dictionary;

        public TextMapCluster(string name)
            : base(name)
        {

        }

        public TextMapCluster(TextMapCluster textMapCluster)
            : base(textMapCluster)
        {
            if(textMapCluster.dictionary != null)
            {
                dictionary = new Dictionary<string, HashSet<string>>();
                foreach (KeyValuePair<string, HashSet<string>> keyValuePair in textMapCluster.dictionary)
                {
                    HashSet<string> values = new HashSet<string>();
                    foreach (string value in keyValuePair.Value)
                        values.Add(value);

                    dictionary[keyValuePair.Key] = values;
                }
            }
        }

        public TextMapCluster(JObject jObject)
            : base(jObject)
        {

        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray_Map = new JArray();
            foreach (KeyValuePair<string, HashSet<string>> keyValuePair in dictionary)
            {
                JArray jArray = new JArray();
                jArray.Add(keyValuePair.Key);

                foreach (string value in keyValuePair.Value)
                    jArray.Add(value);

                jArray_Map.Add(jArray);
            }

            jObject.Add("Map", jArray_Map);

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            dictionary = new Dictionary<string, HashSet<string>>();

            JArray jArray_Map = jObject.Value<JArray>("Map");
            if (jArray_Map != null)
            {

                foreach (JArray jArray in jArray_Map)
                {
                    if (jArray.Count < 1)
                        continue;

                    HashSet<string> values = new HashSet<string>();

                    for(int i=1; i < jArray.Count; i++)
                        values.Add(jArray[i].ToString());

                    dictionary[jArray[0].ToString()] = values;
                }
            }

            return true;
        }
    }
}
