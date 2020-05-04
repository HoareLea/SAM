using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class SAMRelationCluster : SAMObject, IJSAMObject
    {
        private Dictionary<string, Dictionary<string, HashSet<SAMRelation>>> dictionary;

        public SAMRelationCluster(SAMRelationCluster sAMRelationCluster)
            : base(sAMRelationCluster)
        {
            if (sAMRelationCluster == null || sAMRelationCluster.dictionary == null)
                return;

            dictionary = new Dictionary<string, Dictionary<string, HashSet<SAMRelation>>>();

            foreach (KeyValuePair<string, Dictionary<string, HashSet<SAMRelation>>> keyValuePair_1 in sAMRelationCluster.dictionary)
            {
                dictionary[keyValuePair_1.Key] = new Dictionary<string, HashSet<SAMRelation>>();
                foreach (KeyValuePair<string, HashSet<SAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                {
                    dictionary[keyValuePair_1.Key][keyValuePair_2.Key] = new HashSet<SAMRelation>(keyValuePair_2.Value);
                }
            }
        }

        public SAMRelationCluster()
            : base()
        {
            dictionary = new Dictionary<string, Dictionary<string, HashSet<SAMRelation>>>();
        }

        public SAMRelationCluster(IEnumerable<SAMRelation> sAMRelations)
            : base()
        {
            dictionary = new Dictionary<string, Dictionary<string, HashSet<SAMRelation>>>();

            if (sAMRelations != null)
            {
                foreach (SAMRelation sAMRelation in sAMRelations)
                    Add(sAMRelation);
            }
        }

        public SAMRelationCluster(JObject jObject)
            : base()
        {
            FromJObject(jObject);
        }

        public virtual bool Add(SAMRelation sAMRelation)
        {
            object @object = sAMRelation.GetObject<object>();
            if (@object == null)
                return false;

            object relatedObject = sAMRelation.GetRelatedObject<object>();
            if (relatedObject == null)
                return false;

            return Add(@object.GetType().FullName, relatedObject.GetType().FullName, sAMRelation);
        }

        public virtual bool Add(System.Type type_Object, System.Type type_RelatedObject, SAMRelation sAMRelation)
        {
            if (type_Object == null || type_RelatedObject == null)
                return false;

            return Add(type_Object.FullName, type_RelatedObject.FullName, sAMRelation);
        }

        public virtual bool Add(string typeName_Object, string typeName_RelatedObject, SAMRelation sAMRelation)
        {
            if (string.IsNullOrWhiteSpace(typeName_Object) || string.IsNullOrWhiteSpace(typeName_RelatedObject))
                return false;

            Dictionary<string, HashSet<SAMRelation>> dictionary_Temp;

            if (!dictionary.TryGetValue(typeName_Object, out dictionary_Temp))
            {
                dictionary_Temp = new Dictionary<string, HashSet<SAMRelation>>();
                dictionary[typeName_Object] = dictionary_Temp;
            }

            HashSet<SAMRelation> sAMRelations_Temp;
            if (!dictionary_Temp.TryGetValue(typeName_RelatedObject, out sAMRelations_Temp))
            {
                sAMRelations_Temp = new HashSet<SAMRelation>();
                dictionary_Temp[typeName_RelatedObject] = sAMRelations_Temp;
            }

            sAMRelations_Temp.Add(sAMRelation);
            return true;
        }

        public List<SAMRelation> GetSAMRelations<X, Z>()
        {
            if (dictionary == null)
                return null;

            string fullName_1 = typeof(X).FullName;
            string fullName_2 = typeof(Z).FullName;

            List<SAMRelation> result = new List<SAMRelation>();

            Dictionary<string, HashSet<SAMRelation>> dictionary_Type = null;
            if (!dictionary.TryGetValue(fullName_1, out dictionary_Type))
                return result;

            HashSet<SAMRelation> sAMRelations = null;
            if (!dictionary_Type.TryGetValue(fullName_2, out sAMRelations))
                return result;

            if (sAMRelations == null)
                return result;

            return sAMRelations.ToList();
        }

        public List<SAMRelation> GetSAMRelations(string typeName_Object, string typeName_RelatedObject)
        {
            if (dictionary == null || string.IsNullOrWhiteSpace(typeName_Object) || string.IsNullOrWhiteSpace(typeName_RelatedObject))
                return null;

            List<SAMRelation> result = new List<SAMRelation>();

            Dictionary<string, HashSet<SAMRelation>> dictionary_Type = null;
            if (!dictionary.TryGetValue(typeName_Object, out dictionary_Type))
                return result;

            HashSet<SAMRelation> sAMRelations = null;
            if (!dictionary_Type.TryGetValue(typeName_RelatedObject, out sAMRelations))
                return result;

            if (sAMRelations == null)
                return result;

            return sAMRelations.ToList();
        }

        public List<SAMRelation> GetSAMRelations(System.Type type_Object, System.Type type_RelatedObject)
        {
            if (type_Object == null || type_RelatedObject == null)
                return null;

            return GetSAMRelations(type_Object.FullName, type_RelatedObject.FullName);
        }

        public List<SAMRelation> GetSAMRelations()
        {
            if (dictionary == null)
                return null;

            List<SAMRelation> result = new List<SAMRelation>();
            foreach (KeyValuePair<string, Dictionary<string, HashSet<SAMRelation>>> keyValuePair_1 in dictionary)
            {
                foreach (KeyValuePair<string, HashSet<SAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                    result.AddRange(keyValuePair_2.Value);
            }
            return result;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (!jObject.ContainsKey("Types"))
                return true;

            JArray jArray_Types_1 = jObject.Value<JArray>("Types");
            if (jArray_Types_1 == null)
                return true;

            dictionary = new Dictionary<string, Dictionary<string, HashSet<SAMRelation>>>();

            foreach (JObject jObject_Types_1 in jArray_Types_1)
            {
                if (!jObject_Types_1.ContainsKey("Name"))
                    continue;

                string name_1 = jObject_Types_1.Value<string>("Name");
                if (string.IsNullOrEmpty(name_1))
                    continue;

                if (!jObject_Types_1.ContainsKey("Types"))
                    continue;

                JArray jArray_Types_2 = jObject_Types_1.Value<JArray>("Types");
                if (jArray_Types_2 == null)
                    continue;

                Dictionary<string, HashSet<SAMRelation>> dictionary_SAMRelation;
                if (!dictionary.TryGetValue(name_1, out dictionary_SAMRelation))
                {
                    dictionary_SAMRelation = new Dictionary<string, HashSet<SAMRelation>>();
                    dictionary[name_1] = dictionary_SAMRelation;
                }

                foreach (JObject jObject_Types_2 in jArray_Types_2)
                {
                    if (!jObject_Types_2.ContainsKey("Name"))
                        continue;

                    string name_2 = jObject_Types_2.Value<string>("Name");
                    if (string.IsNullOrEmpty(name_2))
                        continue;

                    if (!jObject_Types_2.ContainsKey("Relations"))
                        continue;

                    JArray jArray_Relations = jObject_Types_2.Value<JArray>("Relations");
                    if (jArray_Relations == null)
                        continue;

                    HashSet<SAMRelation> sAMRelations;
                    if (!dictionary_SAMRelation.TryGetValue(name_2, out sAMRelations))
                    {
                        sAMRelations = new HashSet<SAMRelation>();
                        dictionary_SAMRelation[name_2] = sAMRelations;
                    }

                    foreach (JObject jObject_Relation in jArray_Relations)
                    {
                        SAMRelation sAMRelation = Create.IJSAMObject<SAMRelation>(jObject_Relation);
                        if (sAMRelation != null)
                            sAMRelations.Add(sAMRelation);
                    }
                }
            }
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            if (dictionary != null)
            {
                JArray jArray_Type_1 = new JArray();
                foreach (KeyValuePair<string, Dictionary<string, HashSet<SAMRelation>>> keyValuePair_1 in dictionary)
                {
                    JObject jObject_Type_1 = new JObject();
                    jObject_Type_1.Add("Name", keyValuePair_1.Key);

                    JArray jArray_Type_2 = new JArray();
                    foreach (KeyValuePair<string, HashSet<SAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                    {
                        JObject jObject_Type_2 = new JObject();
                        jObject_Type_2.Add("Name", keyValuePair_2.Key);

                        JArray jArray_Relations = new JArray();
                        foreach (SAMRelation sAMRelation in keyValuePair_2.Value)
                        {
                            if (sAMRelation == null)
                                continue;

                            jArray_Relations.Add(sAMRelation.ToJObject());
                        }

                        jObject_Type_2.Add("Relations", jArray_Relations);
                        jArray_Type_2.Add(jObject_Type_2);
                    }
                    jObject_Type_1.Add("Types", jArray_Type_2);

                    jArray_Type_1.Add(jObject_Type_1);
                }
                jObject.Add("Types", jArray_Type_1);
            }
            return jObject;
        }
    }
}