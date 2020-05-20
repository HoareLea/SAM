using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class MapCluster : SAMObject, IJSAMObject
    {
        private List<Tuple<string, string, string, string>> tuples;

        public MapCluster()
        {
            tuples = new List<Tuple<string, string, string, string>>();
        }

        public MapCluster(JObject jObject)
        {
            FromJObject(jObject);
        }

        public bool Add(Type type_1, Type type_2, string name_1, string name_2)
        {
            if (type_1 == null || type_2 == null || string.IsNullOrEmpty(name_1) || string.IsNullOrEmpty(name_2))
                return false;

            return Add(type_1.FullName, type_2.FullName, name_1, name_2);
        }
        
        public bool Add(string typeName_1, string typeName_2, string name_1, string name_2)
        {
            if (string.IsNullOrEmpty(typeName_1) || string.IsNullOrEmpty(typeName_2) || string.IsNullOrEmpty(name_1) || string.IsNullOrEmpty(name_2))
                return false;

            List<Tuple<string, string, string, string>> tuples_Temp = tuples.FindAll(x => x.Item1.Equals(typeName_1));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string>(typeName_1, typeName_2, name_1, name_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item2.Equals(typeName_2));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string>(typeName_1, typeName_2, name_1, name_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item3.Equals(name_1));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string>(typeName_1, typeName_2, name_1, name_2));
                return true;
            }

            tuples_Temp = tuples_Temp.FindAll(x => x.Item4.Equals(name_2));
            if (tuples_Temp.Count == 0)
            {
                tuples.Add(new Tuple<string, string, string, string>(typeName_1, typeName_2, name_1, name_2));
                return true;
            }

            return false;
        }

        public string GetName(string typeName_1, string typeName_2, string name_1)
        {
            if (string.IsNullOrEmpty(typeName_1) || string.IsNullOrEmpty(typeName_2) || string.IsNullOrEmpty(name_1))
                return null;

            return tuples.Find(x => x.Item1.Equals(typeName_1) && x.Item2.Equals(typeName_2) && x.Item3.Equals(name_1))?.Item4;
        }

        public string GetName(Type type_1, Type type_2, string name_1)
        {
            if (type_1 == null || type_2 == null || string.IsNullOrEmpty(name_1))
                return null;

            return GetName(type_1.FullName, type_2.FullName, name_1);
        }

        public List<string> GetNames(Type type_1, Type type_2)
        {
            if (type_1 == null || type_2 == null)
                return null;

            return GetNames(type_1.FullName, type_2.FullName);
        }

        public List<string> GetNames(string typeName_1, string typeName_2)
        {
            if (string.IsNullOrWhiteSpace(typeName_1) || string.IsNullOrWhiteSpace(typeName_2))
                return null;

            return tuples.FindAll(x => x.Item1.Equals(typeName_1) && x.Item2.Equals(typeName_2)).ConvertAll(x => x.Item3);
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray_Map = new JArray();
            foreach (Tuple<string, string, string, string> tuple in tuples)
            {
                JArray jArray = new JArray();
                jArray.Add(tuple.Item1);
                jArray.Add(tuple.Item2);
                jArray.Add(tuple.Item3);
                jArray.Add(tuple.Item4);
                jArray_Map.Add(jArray);
            }

            jObject.Add("Map", jArray_Map);

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            tuples = new List<Tuple<string, string, string, string>>();

            JArray jArray_Map = jObject.Value<JArray>("Map");
            if(jArray_Map != null)
            {
                
                foreach(JArray jArray in jArray_Map)
                {
                    if (jArray.Count < 4)
                        continue;

                    Tuple<string, string, string, string> tuple = new Tuple<string, string, string, string>(jArray[0].Value<string>(), jArray[1].Value<string>(), jArray[2].Value<string>(), jArray[3].Value<string>());

                    if (tuple != null)
                        tuples.Add(tuple);
                }

 
            }

            return true;
        }
    }
}
