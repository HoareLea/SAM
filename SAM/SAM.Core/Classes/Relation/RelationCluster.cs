using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class RelationCluster : SAMObject, IJSAMObject
    {
        private Dictionary<string, Dictionary<Guid, object>> dictionary_Objects;
        private Dictionary<string, Dictionary<Guid, HashSet<Guid>>> dictionary_Relations;

        private List<GuidCollection> groups;

        public RelationCluster()
        {
            dictionary_Objects = new Dictionary<string, Dictionary<Guid, object>>();
            dictionary_Relations = new Dictionary<string, Dictionary<Guid, HashSet<Guid>>>();
        }

        public RelationCluster(JObject jObject)
        {
            FromJObject(jObject);
        }

        public RelationCluster(RelationCluster relationCluster)
            : base(relationCluster)
        {
            dictionary_Objects = new Dictionary<string, Dictionary<Guid, object>>();
            foreach (KeyValuePair<string, Dictionary<Guid, object>> keyValuePair_1 in relationCluster.dictionary_Objects)
            {
                Dictionary<Guid, object> dictionary = new Dictionary<Guid, object>();
                foreach (KeyValuePair<Guid, object> keyValuePair_2 in keyValuePair_1.Value)
                    dictionary[keyValuePair_2.Key] = keyValuePair_2.Value;

                dictionary_Objects[keyValuePair_1.Key] = dictionary;
            }

            dictionary_Relations = new Dictionary<string, Dictionary<Guid, HashSet<Guid>>>();
            foreach (KeyValuePair<string, Dictionary<Guid, HashSet<Guid>>> keyValuePair_1 in relationCluster.dictionary_Relations)
            {
                Dictionary<Guid, HashSet<Guid>> dictionary = new Dictionary<Guid, HashSet<Guid>>();
                foreach (KeyValuePair<Guid, HashSet<Guid>> keyValuePair_2 in keyValuePair_1.Value)
                {
                    HashSet<Guid> guids = new HashSet<Guid>();
                    foreach (Guid guid in keyValuePair_2.Value)
                        guids.Add(guid);

                    dictionary[keyValuePair_2.Key] = guids;
                }

                dictionary_Relations[keyValuePair_1.Key] = dictionary;
            }

            groups = relationCluster.groups?.ConvertAll(x => new GuidCollection(x));
        }

        /// <summary>
        /// Adds two objects to RelationCluster and creates relation between them
        /// </summary>
        /// <param name="object_1">First Object</param>
        /// <param name="object_2">Second Object</param>
        /// <returns>true if objects and relations sucessfully added</returns>
        public bool Add(object object_1, object object_2)
        {
            if (!IsValid(object_1) || !IsValid(object_2))
                return false;

            //dictionary_Objects object_1
            string typeName_1 = null;
            Guid guid_1 = Guid.Empty;
            if (!TryAddObject(object_1, out typeName_1, out guid_1))
                return false;

            //dictionary_Objects object_2
            string typeName_2 = null;
            Guid guid_2 = Guid.Empty;
            if (!TryAddObject(object_2, out typeName_2, out guid_2))
                return false;

            //dictionary_Relations object_1
            if (!AddRelation(typeName_1, guid_1, guid_2))
                return false;

            //dictionary_Relations object_1
            if (!AddRelation(typeName_2, guid_2, guid_1))
                return false;

            return true;
        }

        /// <summary>
        /// Creates relation between two objects. Objects with provided guids needs to be added before adding relation
        /// </summary>
        /// <param name="guid_1">Guid of first element</param>
        /// <param name="guid_2">Guid of second element</param>
        /// <returns>true if relation sucessfully added</returns>
        public bool AddRelation(Guid guid_1, Guid guid_2)
        {
            string typeName_1 = GetTypeName(guid_1);
            if (string.IsNullOrEmpty(typeName_1))
                return false;

            string typeName_2 = GetTypeName(guid_2);
            if (string.IsNullOrEmpty(typeName_2))
                return false;

            if (!AddRelation(typeName_1, guid_1, guid_2))
                return false;

            if (!AddRelation(typeName_2, guid_2, guid_1))
                return false;

            return true;
        }

        public bool AddRelation(object object_1, object object_2)
        {
            if (!IsValid(object_1) || !IsValid(object_2))
                return false;

            Guid guid_1 = GetGuid(object_1);
            if (guid_1 == Guid.Empty)
                return false;

            Guid guid_2 = GetGuid(object_2);
            if (guid_2 == Guid.Empty)
                return false;

            string typeName_1 = object_1.GetType().FullName;

            string typeName_2 = object_2.GetType().FullName;

            if (!AddRelation(typeName_1, guid_1, guid_2))
                return false;

            if (!AddRelation(typeName_2, guid_2, guid_1))
                return false;

            return true;
        }

        private bool AddRelation(string typeName, Guid guid_1, Guid guid_2)
        {
            if (string.IsNullOrWhiteSpace(typeName) || guid_1 == Guid.Empty || guid_2 == Guid.Empty)
                return false;

            Dictionary<Guid, HashSet<Guid>> dictionary = null;
            if (!dictionary_Relations.TryGetValue(typeName, out dictionary))
            {
                dictionary = new Dictionary<Guid, HashSet<Guid>>();
                dictionary_Relations[typeName] = dictionary;
            }

            HashSet<Guid> guids = null;
            if (!dictionary.TryGetValue(guid_1, out guids))
            {
                guids = new HashSet<Guid>();
                dictionary[guid_1] = guids;
            }

            guids.Add(guid_2);
            return true;
        }

        private bool RemoveRelation(string typeName, Guid guid_1, Guid guid_2)
        {
            if (string.IsNullOrWhiteSpace(typeName) || guid_1 == Guid.Empty || guid_2 == Guid.Empty)
                return false;

            Dictionary<Guid, HashSet<Guid>> dictionary = null;
            if (!dictionary_Relations.TryGetValue(typeName, out dictionary))
            {
                dictionary = new Dictionary<Guid, HashSet<Guid>>();
                dictionary_Relations[typeName] = dictionary;
            }

            HashSet<Guid> guids = null;
            if (!dictionary.TryGetValue(guid_1, out guids))
            {
                guids = new HashSet<Guid>();
                dictionary[guid_1] = guids;
            }

            return guids.Remove(guid_2);
        }

        public bool AddObject(object @object)
        {
            if (!IsValid(@object))
                return false;

            string typeName = null;
            Guid guid = Guid.Empty;

            return TryAddObject(@object, out typeName, out guid);
        }

        public bool AddObjects<T>(IEnumerable<T> objects)
        {
            if (objects == null)
                return false;

            foreach (T @object in objects)
                AddObject(@object);

            return true;
        }

        public GuidCollection AddGroup<T>(IEnumerable<T> objects, string name = null, ParameterSet parameterSet = null)
        {
            GuidCollection result = new GuidCollection(name, parameterSet);
            if(objects != null)
            {
                foreach(T @object in objects)
                {
                    Guid guid = GetGuid(@object);
                    if (guid == Guid.Empty)
                        continue;

                    result.Add(guid);
                }
            }

            if (groups == null)
                groups = new List<GuidCollection>();

            groups.Add(result);

            return new GuidCollection(result);
        }

        public GuidCollection UpdateGroup(GuidCollection guidCollection)
        {
            if (guidCollection == null)
                return null;

            GuidCollection guidCollection_Temp = guidCollection.Clone();
            foreach(Guid guid in guidCollection.ToList())
            {
                if (GetObject(guid) == null)
                    guidCollection_Temp.Remove(guid);
            }
             
            if(groups == null)
            {
                groups = new List<GuidCollection>() { guidCollection_Temp };
                return guidCollection_Temp.Clone();
            }

            int index = groups.FindIndex(x => x.Guid == guidCollection_Temp.Guid);
            if(index == -1)
            {
                groups.Add(guidCollection_Temp);
                return guidCollection_Temp.Clone();
            }

            groups[index] = guidCollection_Temp;
            return guidCollection_Temp.Clone();
        }

        public GuidCollection AddGroup(string name)
        {
            GuidCollection result = new GuidCollection(name);
            
            if (groups == null)
                groups = new List<GuidCollection>();

            groups.Add(result);

            return new GuidCollection(result);
        }

        public List<bool> AddToGroup<T>(IEnumerable<T> objects, Guid guid, bool allowDuplicates = true)
        {
            if (groups == null || groups.Count == 0 || objects == null || objects.Count() == 0)
                return null;

            GuidCollection guidCollection = groups.Find(x => x.Guid == guid);
            if (guidCollection == null)
                return null;

            List<bool> result = new List<bool>();
            foreach (T @object in objects)
            {
                Guid guid_Object = GetGuid(@object);
                if (guid_Object == Guid.Empty)
                {
                    result.Add(false);
                    continue;
                }
                
                if(!allowDuplicates && guidCollection.Contains(guid_Object))
                {
                    result.Add(false);
                    continue;
                }

                guidCollection.Add(guid_Object);
                result.Add(true);
            }

            return result;
        }

        public List<bool> RemoveFromGroup<T>(IEnumerable<T> objects, Guid guid)
        {
            if (groups == null || groups.Count == 0 || objects == null || objects.Count() == 0)
                return null;

            GuidCollection guidCollection = groups.Find(x => x.Guid == guid);
            if (guidCollection == null)
                return null;

            List<bool> result = new List<bool>();
            foreach(T @object in objects)
            {
                Guid guid_Object = GetGuid(@object);
                if (guid_Object == Guid.Empty)
                {
                    result.Add(false);
                    continue;
                }

                result.Add(guidCollection.Remove(guid_Object));
            }

            return result;
        }

        public GuidCollection GetGroup(Guid guid)
        {
            if (groups == null || guid == Guid.Empty)
                return null;

            foreach (GuidCollection group in groups)
            {
                if (group?.Guid == guid)
                    return new GuidCollection(group);
            }

            return null;
        }

        public GuidCollection GetGroup(string name)
        {
            if (groups == null || name == null)
                return null;

            foreach (GuidCollection group in groups)
            {
                if (group?.Name == name)
                    return new GuidCollection(group);
            }

            return null;
        }

        public List<GuidCollection> GetGroups(string name)
        {
            if (groups == null || name == null)
                return null;

            List<GuidCollection> result = new List<GuidCollection>();
            foreach (GuidCollection group in groups)
            {
                if (group?.Name == name)
                    result.Add(new GuidCollection(group));
            }

            return result;
        }

        public List<T> GetGroups<T>(string name) where T: GuidCollection
        {
            return GetGroups(name)?.FindAll(x => x is T).Cast<T>().ToList();
        }

        public List<GuidCollection> GetGroups(object @object)
        {
            if (groups == null)
                return null;

            Guid guid = GetGuid(@object);
            if (guid == Guid.Empty)
                return null;

            List<GuidCollection> result = new List<GuidCollection>();
            foreach(GuidCollection group in groups)
            {
                if (group == null || !group.Contains(guid))
                    continue;

                result.Add(new GuidCollection(group));
            }

            return result;
        }

        public List<T> GetGroups<T>(object @object) where T : GuidCollection
        {
            return GetGroups(@object)?.FindAll(x => x is T).Cast<T>().ToList();
        }

        public bool RemoveGroup(Guid guid)
        {
            if (groups == null || guid == Guid.Empty)
                return false;

            foreach(GuidCollection group in groups)
            {
                if (group?.Guid == guid)
                    return groups.Remove(group);
            }

            return false;
        }

        public bool RemoveGroup(string name)
        {
            if (groups == null)
                return false;

            foreach (GuidCollection group in groups)
            {
                if (group == null)
                    continue;

                string name_Group = group.Name;
                if(name_Group == null && name  == null)
                    return groups.Remove(group);

                if (name_Group.Equals(name))
                    return groups.Remove(group);
            }

            return false;
        }

        public List<GuidCollection> Groups
        {
            get
            {
                if (groups == null)
                    return null;

                return groups.ConvertAll(x => x.Clone());
            }
        }

        public bool Join(RelationCluster relationCluster)
        {
            if (relationCluster == null)
                return false;

            foreach (KeyValuePair<string, Dictionary<Guid, object>> keyValuePair_1 in relationCluster.dictionary_Objects)
            {
                foreach (KeyValuePair<Guid, object> keyValuePair_2 in keyValuePair_1.Value)
                {
                    if (!AddObject(keyValuePair_2.Value))
                        continue;

                    List<object> relatedObjects = relationCluster.GetRelatedObjects(keyValuePair_2.Value);
                    if (relatedObjects == null)
                        continue;

                    foreach (object relatedObject in relatedObjects)
                    {
                        if (!AddObject(relatedObject))
                            continue;

                        AddRelation(keyValuePair_2.Value, relatedObject);
                    }

                }
            }

            if(relationCluster.groups != null)
            {
                if (groups == null)
                    groups = new List<GuidCollection>();

                groups.AddRange(relationCluster.groups.ConvertAll(x => new GuidCollection(x)));
            }    

            return true;
        }

        public bool Contains(object @object)
        {
            if (!IsValid(@object))
                return false;

            string typeName = @object.GetType().FullName;

            Dictionary<Guid, object> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
                return false;

            Guid guid = Guid.Empty;
            if (@object is ISAMObject)
                guid = ((ISAMObject)@object).Guid;

            if (guid != Guid.Empty)
                return dictionary.ContainsKey(guid);

            foreach (KeyValuePair<Guid, object> keyValuePair in dictionary)
                if (@object.Equals(keyValuePair.Value))
                    return true;

            return false;
        }

        public bool TryAddObject(object @object, out string typeName, out Guid guid)
        {
            typeName = null;
            guid = Guid.Empty;

            if (!IsValid(@object))
                return false;

            typeName = @object.GetType().FullName;

            Dictionary<Guid, object> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
            {
                dictionary = new Dictionary<Guid, object>();
                dictionary_Objects[typeName] = dictionary;
            }

            guid = Guid.Empty;
            if (@object is ISAMObject)
                guid = ((ISAMObject)@object).Guid;

            if (guid == Guid.Empty)
            {
                foreach (KeyValuePair<Guid, object> keyValuePair in dictionary)
                    if (@object.Equals(keyValuePair.Value))
                    {
                        guid = keyValuePair.Key;
                        break;
                    }
            }

            if (guid == Guid.Empty)
                guid = Guid.NewGuid();

            dictionary[guid] = @object;
            return true;
        }

        public bool TryRemoveObject(object @object, out string typeName, out Guid guid)
        {
            typeName = null;
            guid = Guid.Empty;

            if (!IsValid(@object))
                return false;

            typeName = @object.GetType().FullName;

            Dictionary<Guid, object> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
            {
                dictionary = new Dictionary<Guid, object>();
                dictionary_Objects[typeName] = dictionary;
            }

            guid = Guid.Empty;
            if (@object is ISAMObject)
                guid = ((ISAMObject)@object).Guid;

            if (guid == Guid.Empty)
            {
                foreach (KeyValuePair<Guid, object> keyValuePair in dictionary)
                    if (@object.Equals(keyValuePair.Value))
                    {
                        guid = keyValuePair.Key;
                        break;
                    }
            }

            if (guid == Guid.Empty)
                return false;

            if (groups != null)
            {
                foreach (GuidCollection guidCollection in groups)
                    guidCollection?.Remove(guid);
            }

            return dictionary.Remove(guid);
        }

        public Guid GetGuid(object @object)
        {
            if (!IsValid(@object))
                return Guid.Empty;

            Dictionary<Guid, object> dictionary;
            if (!dictionary_Objects.TryGetValue(@object.GetType().FullName, out dictionary))
                return Guid.Empty;

            if (dictionary == null)
                return Guid.Empty;

            if (@object is ISAMObject)
            {
                Guid guid = ((ISAMObject)@object).Guid;
                if (dictionary.ContainsKey(guid))
                    return guid;
            }
            else
            {
                foreach (KeyValuePair<Guid, object> keyValuePair in dictionary)
                    if (@object.Equals(keyValuePair.Value))
                        return keyValuePair.Key;
            }

            return Guid.Empty;
        }

        public List<object> GetObjects(IEnumerable<Guid> guids)
        {
            if (guids == null)
                return null;

            List<Guid> guids_Temp = new List<Guid>(guids);
            guids_Temp.RemoveAll(x => x == Guid.Empty);

            List<object> result = new List<object>();
            foreach (Dictionary<Guid, object> dictionary_Temp in dictionary_Objects.Values)
            {
                List<Guid> guids_Remove = new List<Guid>();
                foreach (Guid guid_Temp in guids_Temp)
                {
                    object object_Temp = null;
                    if (!dictionary_Temp.TryGetValue(guid_Temp, out object_Temp))
                        continue;

                    if (object_Temp == null)
                        continue;

                    guids_Remove.Add(guid_Temp);
                    result.Add(object_Temp);
                }

                guids_Temp.RemoveAll(x => guids_Remove.Contains(x));
                if (guids_Temp.Count == 0)
                    break;
            }

            return result;
        }

        public List<T> GetObjects<T>(GuidCollection guidCollection)
        {
            if (guidCollection == null)
                return null;

            List<T> result = new List<T>();
            foreach(Guid guid in guidCollection)
            {
                object @object = GetObject(guid);
                if (@object is T)
                    result.Add((T)@object);
                else
                    result.Add(default);
            }

            return result;
        }

        public List<T> GetObjects<T>(GuidCollection guidCollection, Type type)
        {
            if (guidCollection == null)
                return null;

            string typeName = type.FullName;

            Dictionary<Guid, object> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
                return null;

            List<T> result = new List<T>();
            foreach(Guid guid in guidCollection)
            {
                object @object = null;
                if (dictionary.TryGetValue(guid, out @object) && @object is T)
                    result.Add((T)@object);
                else
                    result.Add(default);
            }

            return result;
        }

        public List<object> GetObjects(Type type)
        {
            if (!IsValid(type))
                return null;

            string typeName = type.FullName;
            if (!dictionary_Objects.ContainsKey(typeName))
                return null;

            List<object> result = new List<object>();
            foreach (KeyValuePair<Guid, object> keyValuePair in dictionary_Objects[typeName])
                result.Add(keyValuePair.Value);

            return result;
        }

        public List<T> GetObjects<T>()
        {
            List<object> objects = GetObjects(typeof(T));
            if (objects == null)
                return null;

            List<T> result = new List<T>();
            foreach (object @object in objects)
                if (@object is T)
                    result.Add((T)@object);

            return result;
        }

        public object GetObject(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            foreach (Dictionary<Guid, object> dictionary_Temp in dictionary_Objects.Values)
            {
                if (dictionary_Temp.TryGetValue(guid, out object object_Temp) && object_Temp != null)
                    return object_Temp;
            }

            return null;
        }

        public T GetObject<T>(Guid guid)
        {
            object @object = GetObject(typeof(T), guid);
            if (@object == null)
                return default(T);

            return (T)@object;
        }

        public object GetObject(Type type, Guid guid)
        {
            if (!IsValid(type))
                return null;

            object @object = dictionary_Objects[type.FullName]?[guid];
            if (type.IsAssignableFrom(@object.GetType()))
                return @object;

            return null;
        }

        public string GetTypeName(Guid guid)
        {
            foreach (KeyValuePair<string, Dictionary<Guid, object>> keyValuePair in dictionary_Objects)
                if (keyValuePair.Value.ContainsKey(guid))
                    return keyValuePair.Key;

            return null;
        }

        public List<object> GetRelatedObjects(Guid guid)
        {
            object @object = GetObject(guid);
            if (@object == null)
                return null;

            return GetRelatedObjects(@object);
        }

        public List<object> GetRelatedObjects(object @object)
        {
            if (@object is Guid)
                return GetRelatedObjects((Guid)@object);

            if (!IsValid(@object))
                return null;

            Guid guid = GetGuid(@object);
            if (guid == Guid.Empty)
                return null;

            string typeName = @object.GetType().FullName;

            Dictionary<Guid, HashSet<Guid>> dictionary = null;
            if (!dictionary_Relations.TryGetValue(typeName, out dictionary))
                return null;

            HashSet<Guid> guids = null;
            if (!dictionary.TryGetValue(guid, out guids))
                return null;

            return GetObjects(guids);
        }

        public List<T> GetRelatedObjects<T>(object @object)
        {
            List<object> objects = GetRelatedObjects(@object);
            if (objects == null)
                return null;

            List<T> result = new List<T>();
            foreach (object object_Temp in objects)
                if (object_Temp is T)
                    result.Add((T)object_Temp);

            return result;
        }

        public int GetIndex(object @object)
        {
            if (@object == null)
                return -1;

            object object_Temp = @object;
            if (@object is Guid)
                object_Temp = GetObject(Guid);

            if (object_Temp == null)
                return -1;

            Guid guid = GetGuid(object_Temp);
            if (guid.Equals(Guid.Empty))
                return -1;

            string typeName = object_Temp.GetType().FullName;

            Dictionary<Guid, object> dictionary;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
                return -1;

            int count = 0;
            foreach(Guid guid_Temp in dictionary.Keys)
            {
                if (guid_Temp.Equals(guid))
                    return count;

                count++;
            }

            return -1;
        }

        public bool Contains(Type type)
        {
            if (type == null)
                return false;

            return dictionary_Objects.ContainsKey(type.FullName);
        }

        public virtual bool IsValid(Type type)
        {
            return type != null;
        }

        public virtual RelationCluster Clone()
        {
            return new RelationCluster(this);
        }

        public bool IsValid(object @object)
        {
            if (@object == null)
                return false;

            return IsValid(@object.GetType());
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            JArray jArray_Objects = new JArray();
            foreach (KeyValuePair<string, Dictionary<Guid, object>> keyValuePair_Type in dictionary_Objects)
            {
                JObject jObject_Objects = new JObject();
                jObject_Objects.Add("Key", keyValuePair_Type.Key);
                JArray jArray = new JArray();
                foreach (KeyValuePair<Guid, object> keyValuePair in keyValuePair_Type.Value)
                {
                    JObject jObject_Temp = new JObject();
                    jObject_Temp.Add("Key", keyValuePair.Key);

                    object @object = keyValuePair.Value;

                    if (@object is IJSAMObject)
                        jObject_Temp.Add("Value", ((IJSAMObject)@object).ToJObject());
                    else if (@object is double)
                        jObject_Temp.Add("Value", (double)@object);
                    else if (@object is string)
                        jObject_Temp.Add("Value", (string)@object);
                    else if (@object is int)
                        jObject_Temp.Add("Value", (int)@object);
                    else if (@object is bool)
                        jObject_Temp.Add("Value", (bool)@object);

                    jArray.Add(jObject_Temp);
                }
                jObject_Objects.Add("Value", jArray);

                jArray_Objects.Add(jObject_Objects);
            }

            jObject.Add("Objects", jArray_Objects);

            JArray jArray_Relations = new JArray();
            foreach (KeyValuePair<string, Dictionary<Guid, HashSet<Guid>>> keyValuePair_Type in dictionary_Relations)
            {
                JObject jObject_Objects = new JObject();
                jObject_Objects.Add("Key", keyValuePair_Type.Key);
                JArray jArray = new JArray();
                foreach (KeyValuePair<Guid, HashSet<Guid>> keyValuePair in keyValuePair_Type.Value)
                {
                    JObject jObject_Temp = new JObject();
                    jObject_Temp.Add("Key", keyValuePair.Key);
                    JArray jArray_Guids = new JArray();
                    foreach (Guid guid in keyValuePair.Value)
                        jArray_Guids.Add(guid);
                    jObject_Temp.Add("Value", jArray_Guids);

                    jArray.Add(jObject_Temp);
                }
                jObject_Objects.Add("Value", jArray);

                jArray_Relations.Add(jObject_Objects);
            }
            jObject.Add("Relations", jArray_Relations);

            if(groups != null)
            {
                JArray jArray_Groups = new JArray();
                foreach (GuidCollection group in groups)
                    jArray_Groups.Add(group.ToJObject());

                jObject.Add("Groups", jArray_Groups);
            }

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            dictionary_Objects = new Dictionary<string, Dictionary<Guid, object>>();
            dictionary_Relations = new Dictionary<string, Dictionary<Guid, HashSet<Guid>>>();

            JArray jArray_Objects = jObject.Value<JArray>("Objects");
            if (jArray_Objects != null)
            {
                foreach (JObject jObject_Objects in jArray_Objects)
                {
                    string typeName = jObject_Objects.Value<string>("Key");
                    if (string.IsNullOrWhiteSpace((typeName)))
                        continue;

                    JArray jArray = jObject_Objects.Value<JArray>("Value");
                    if (jArray == null)
                        continue;

                    Dictionary<Guid, object> dictionary = new Dictionary<Guid, object>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        Guid guid = jObject_Temp.Guid("Key");
                        if (guid == Guid.Empty)
                            continue;

                        object @object = null;
                        JToken jToken = jObject_Temp.GetValue("Value");
                        switch (jToken.Type)
                        {
                            case JTokenType.Object:
                                @object = Create.IJSAMObject((JObject)jToken);
                                break;
                            case JTokenType.Boolean:
                                @object = jToken.Value<bool>();
                                break;
                            case JTokenType.Integer:
                                @object = jToken.Value<int>();
                                break;
                            case JTokenType.String:
                                @object = jToken.Value<string>();
                                break;
                            case JTokenType.Float:
                                @object = jToken.Value<double>();
                                break;
                            case JTokenType.Date:
                                @object = jToken.Value<System.DateTime>();
                                break;
                        }

                        if (@object == null)
                            continue;

                        dictionary[guid] = @object;
                    }
                    dictionary_Objects[typeName] = dictionary;
                }
            }

            JArray jArray_Relations = jObject.Value<JArray>("Relations");
            if (jArray_Relations != null)
            {
                foreach (JObject jObject_Relations in jArray_Relations)
                {
                    string typeName = jObject_Relations.Value<string>("Key");
                    if (string.IsNullOrWhiteSpace((typeName)))
                        continue;

                    JArray jArray = jObject_Relations.Value<JArray>("Value");
                    if (jArray == null)
                        continue;

                    Dictionary<Guid, HashSet<Guid>> dictionary = new Dictionary<Guid, HashSet<Guid>>();
                    foreach (JObject jObject_Temp in jArray)
                    {
                        Guid guid = jObject_Temp.Guid("Key");
                        if (guid == Guid.Empty)
                            continue;

                        JArray jArray_Guids = jObject_Temp.Value<JArray>("Value");
                        if (jArray_Guids == null)
                            continue;

                        HashSet<Guid> guids = new HashSet<Guid>();
                        foreach (JToken jToken in jArray_Guids)
                        {
                            Guid guid_Temp = Query.Guid(jToken);
                            if (guid_Temp != Guid.Empty)
                                guids.Add(guid_Temp);
                        }
                        dictionary[guid] = guids;
                    }
                    dictionary_Relations[typeName] = dictionary;
                }
            }

            if(jObject.ContainsKey("Groups"))
            {
                JArray jArray_Groups = jObject.Value<JArray>("Groups");
                if(jArray_Groups != null)
                {
                    groups = new List<GuidCollection>();
                    foreach(JObject jObject_Group in jArray_Groups)
                        if (jObject_Group != null)
                            groups.Add(new GuidCollection(jObject_Group));
                }
            }

            return true;
        }

        public bool RemoveObject(Type type, Guid guid)
        {
            if (guid == Guid.Empty || type == null)
                return false;

            if (!IsValid(type))
                return false;

            string typeName = type.FullName;

            Dictionary<Guid, object> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
                return false;

            object @object = null;
            if (!dictionary.TryGetValue(guid, out @object))
                return false;

            List<object> relatedObjects = GetRelatedObjects(@object);
            if (relatedObjects != null && relatedObjects.Count != 0)
            {
                foreach (object relatedObject in relatedObjects)
                    RemoveRelation(@object, relatedObject);
            }

            if(groups != null)
            {
                foreach (GuidCollection group in groups)
                    group?.Remove(guid);
            }

            return dictionary.Remove(guid);
        }

        public bool RemoveObject<T>(Guid guid)
        {
            return RemoveObject(typeof(T), guid);
        }

        public bool RemoveRelation(object object_1, object object_2)
        {
            if (!IsValid(object_1) || !IsValid(object_2))
                return false;

            Guid guid_1 = GetGuid(object_1);
            if (guid_1 == Guid.Empty)
                return false;

            Guid guid_2 = GetGuid(object_2);
            if (guid_2 == Guid.Empty)
                return false;

            string typeName_1 = object_1.GetType().FullName;

            string typeName_2 = object_2.GetType().FullName;

            if (!RemoveRelation(typeName_1, guid_1, guid_2))
                return false;

            if (!RemoveRelation(typeName_2, guid_2, guid_1))
                return false;

            return true;
        }
    }
}
