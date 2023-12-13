using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class RelationCluster_NEW<X> : SAMObject, IJSAMObject
    {
        private Dictionary<string, Dictionary<Guid, X>> dictionary_Objects;
        private Dictionary<string, Dictionary<Guid, HashSet<Guid>>> dictionary_Relations;

        public RelationCluster_NEW()
        {
            dictionary_Objects = new Dictionary<string, Dictionary<Guid, X>>();
            dictionary_Relations = new Dictionary<string, Dictionary<Guid, HashSet<Guid>>>();
        }

        public RelationCluster_NEW(JObject jObject)
        {
            FromJObject(jObject);
        }

        public RelationCluster_NEW(RelationCluster_NEW<X> relationCluster)
            : base(relationCluster)
        {
            dictionary_Objects = new Dictionary<string, Dictionary<Guid, X>>();
            foreach (KeyValuePair<string, Dictionary<Guid, X>> keyValuePair_1 in relationCluster.dictionary_Objects)
            {
                Dictionary<Guid, X> dictionary = new Dictionary<Guid, X>();
                foreach (KeyValuePair<Guid, X> keyValuePair_2 in keyValuePair_1.Value)
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
        }

        /// <summary>
        /// Adds two objects to RelationCluster and creates relation between them
        /// </summary>
        /// <param name="object_1">First Object</param>
        /// <param name="object_2">Second Object</param>
        /// <returns>true if objects and relations sucessfully added</returns>
        public bool Add(X object_1, X object_2)
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

        public bool AddRelation(X object_1, X object_2)
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

        public bool AddObject(X @object)
        {
            if (!IsValid(@object))
                return false;

            return TryAddObject(@object, out string typeName, out Guid guid);
        }

        public bool AddObjects<T>(IEnumerable<T> objects) where T : X
        {
            if (objects == null)
            {
                return false;
            }

            foreach (T @object in objects)
            {
                AddObject(@object);
            }

            return true;
        }

        public bool Join(RelationCluster_NEW<X> relationCluster)
        {
            if (relationCluster == null)
                return false;

            foreach (KeyValuePair<string, Dictionary<Guid, X>> keyValuePair_1 in relationCluster.dictionary_Objects)
            {
                foreach (KeyValuePair<Guid, X> keyValuePair_2 in keyValuePair_1.Value)
                {
                    if (!AddObject(keyValuePair_2.Value))
                        continue;

                    List<X> relatedObjects = relationCluster.GetRelatedObjects(keyValuePair_2.Value);
                    if (relatedObjects == null)
                        continue;

                    foreach (X relatedObject in relatedObjects)
                    {
                        if (!AddObject(relatedObject))
                            continue;

                        AddRelation(keyValuePair_2.Value, relatedObject);
                    }

                }
            }  

            return true;
        }

        public bool Contains(X @object)
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

        public bool TryAddObject(X @object, out string typeName, out Guid guid)
        {
            typeName = null;
            guid = Guid.Empty;

            if (!IsValid(@object))
                return false;

            typeName = @object.GetType().FullName;

            if (!dictionary_Objects.TryGetValue(typeName, out Dictionary<Guid, X> dictionary))
            {
                dictionary = new Dictionary<Guid, X>();
                dictionary_Objects[typeName] = dictionary;
            }

            guid = Guid.Empty;
            if (@object is ISAMObject)
                guid = ((ISAMObject)@object).Guid;

            if (guid == Guid.Empty)
            {
                foreach (KeyValuePair<Guid, X> keyValuePair in dictionary)
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

        public bool TryRemoveObject(X @object, out string typeName, out Guid guid)
        {
            typeName = null;
            guid = Guid.Empty;

            if (!IsValid(@object))
                return false;

            typeName = @object.GetType().FullName;

            Dictionary<Guid, X> dictionary = null;
            if (!dictionary_Objects.TryGetValue(typeName, out dictionary))
            {
                dictionary = new Dictionary<Guid, X>();
                dictionary_Objects[typeName] = dictionary;
            }

            guid = Guid.Empty;
            if (@object is ISAMObject)
                guid = ((ISAMObject)@object).Guid;

            if (guid == Guid.Empty)
            {
                foreach (KeyValuePair<Guid, X> keyValuePair in dictionary)
                    if (@object.Equals(keyValuePair.Value))
                    {
                        guid = keyValuePair.Key;
                        break;
                    }
            }

            if (guid == Guid.Empty)
                return false;

            return dictionary.Remove(guid);
        }

        public Guid GetGuid(X @object)
        {
            if (!IsValid(@object))
            {
                return Guid.Empty;
            }

            Dictionary<Guid, X> dictionary;
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
                foreach (KeyValuePair<Guid, X> keyValuePair in dictionary)
                    if (@object.Equals(keyValuePair.Value))
                        return keyValuePair.Key;
            }

            return Guid.Empty;
        }

        public HashSet<Guid> Guids
        {
            get
            {
                if(dictionary_Objects == null)
                {
                    return null;
                }

                HashSet<Guid> result = new HashSet<Guid>();
                foreach(KeyValuePair< string, Dictionary<Guid, X>> keyValuePair_1 in dictionary_Objects)
                {
                    foreach(KeyValuePair<Guid, X> keyValuePair_2 in keyValuePair_1.Value)
                    {
                        result.Add(keyValuePair_2.Key);
                    }
                }

                return result;
            }
        }

        public List<X> GetObjects(IEnumerable<Guid> guids)
        {
            if (guids == null)
                return null;

            List<Guid> guids_Temp = new List<Guid>(guids);
            guids_Temp.RemoveAll(x => x == Guid.Empty);

            List<X> result = new List<X>();
            foreach (Dictionary<Guid, X> dictionary_Temp in dictionary_Objects.Values)
            {
                List<Guid> guids_Remove = new List<Guid>();
                foreach (Guid guid_Temp in guids_Temp)
                {
                    if (!dictionary_Temp.TryGetValue(guid_Temp, out X object_Temp))
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

        private List<X> GetObjects(ObjectReference objectReference, X parent = default(X))
        {
            if (objectReference == null)
            {
                return null;
            }

            Type type = objectReference.Type;

            List<X> objects = null;
            if(parent == null)
            {
                objects = type != null ? GetObjects(type) : GetObjects();
            }
            else
            {
                objects = type != null ? GetRelatedObjects(parent, type) : GetRelatedObjects(parent);
            }

            if(objects == null || objects.Count == 0)
            {
                return objects;
            }

            Reference? reference = objectReference.Reference;
            if(reference != null && reference.HasValue)
            {
                string referenceString = reference.ToString();
                if(!string.IsNullOrWhiteSpace(referenceString))
                {
                    if(Guid.TryParse(referenceString, out Guid guid))
                    {
                        foreach(X @object in objects)
                        {
                            Guid guid_Temp = GetGuid(@object);
                            if(guid_Temp == guid)
                            {
                                return new List<X>() { @object };
                            }
                        }
                    }
                    else if(int.TryParse(referenceString, out int @int))
                    {
                        if(@int != -1 && objects.Count > @int)
                        {
                            return new List<X>() { objects[@int] };
                        }
                    }
                    else if(referenceString.StartsWith(@"""") && referenceString.EndsWith(@""""))
                    {
                        referenceString = referenceString.Substring(1, referenceString.Length - 2);

                        List<X> result = new List<X>();
                        foreach (X @object in objects)
                        {
                            SAMObject sAMObject = @object as SAMObject;
                            if(sAMObject == null)
                            {
                                continue;
                            }

                            if(referenceString.Equals(sAMObject.Name))
                            {
                                result.Add(@object);
                            }
                        }
                        return result;
                    }

                    return null;
                }
            }
          
            return objects;
        }

        public List<X> GetObjects(string typeName)
        {
            if (!dictionary_Objects.ContainsKey(typeName))
                return null;

            List<X> result = new List<X>();
            foreach (KeyValuePair<Guid, X> keyValuePair in dictionary_Objects[typeName])
                result.Add(keyValuePair.Value);

            return result;
        }

        public List<T> GetObjects<T>(string typeName) where T: X
        {
            if (!dictionary_Objects.ContainsKey(typeName))
            {
                return null;
            }

            List<X> objects = GetObjects(typeName);
            if(objects == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (object @object in objects)
            {
                if(@object is T)
                {
                    result.Add((T)@object);
                }
            }

            return result;
        }

        public List<X> GetObjects(Type type)
        {
            if (dictionary_Objects == null)
                return null;

            if (!IsValid(type))
                return null;

            List<string> typeNames = GetTypeNames(type);
            if (typeNames == null || typeNames.Count == 0)
            {
                return null;
            }

            List<X> result = new List<X>();
            foreach (string typeName in typeNames)
            {
                List<X> objects = GetObjects<X>(typeName);
                if (objects != null && objects.Count != 0)
                {
                    result.AddRange(objects);
                }
            }

            return result;
        }

        public List<X> GetObjects()
        {
            if (dictionary_Objects == null)
                return null;

            List<X> result = new List<X>();
            foreach (Dictionary<Guid, X> dictionary in dictionary_Objects.Values)
            {
                foreach (KeyValuePair<Guid, X> keyValuePair in dictionary)
                {
                    result.Add(keyValuePair.Value);
                }
            }

            return result;
        }

        public List<T> GetObjects<T>() where T: X
        {
            List<X> objects = GetObjects(typeof(T));
            if (objects == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (X @object in objects)
            {
                if (@object is T)
                {
                    result.Add((T)@object);
                }

            }

            return result;
        }

        public List<T> GetObjects<T>(params Func<T, bool>[] functions)
        {
            if (functions == null)
            {
                return GetObjects<T>();
            }

            List<T> result = GetObjects<T>();
            if (result == null)
            {
                return null;
            }

            for (int i = result.Count - 1; i >= 0; i--)
            {
                bool remove = false;
                foreach (Func<T, bool> function in functions)
                {
                    if (!function(result[i]))
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                {
                    result.RemoveAt(i);
                }
            }

            return result;
        }

        public T GetObject<T>(params Func<T, bool>[] functions) where T:X
        {
            List<T> objects = null;

            if (functions == null)
            {
                objects = GetObjects<T>();
                if(objects == null || objects.Count == 0)
                {
                    return default;
                }

                return objects.FirstOrDefault();
            }

            objects = GetObjects<T>();
            if (objects == null)
            {
                return default;
            }

            foreach(T @object in objects)
            {
                foreach (Func<T, bool> function in functions)
                {
                    if (function(@object))
                    {
                        return @object;
                    }
                }
            }

            return default;
        }

        public X GetObject(Guid guid)
        {
            if (guid == Guid.Empty)
                return default(X);

            foreach (Dictionary<Guid, X> dictionary_Temp in dictionary_Objects.Values)
            {
                if (dictionary_Temp.TryGetValue(guid, out X object_Temp) && object_Temp != null)
                {
                    return object_Temp;
                }
            }

            return default(X);
        }

        public T GetObject<T>(Guid guid) where T: X
        {
            object @object = GetObject(typeof(T), guid);
            if (@object == null)
                return default(T);

            return (T)@object;
        }

        public X GetObject(Type type, Guid guid)
        {
            return GetObject(type, guid, out Dictionary<Guid, X> dictionary, out string typeName);
        }

        private X GetObject(Type type, Guid guid, out Dictionary<Guid, X> dictionary, out string typeName)
        {
            dictionary = null;
            typeName = null;

            if (!IsValid(type))
            {
                return default(X);
            }

            if (dictionary_Objects == null)
            {
                return default(X);
            }

            string fullName = type.FullName;
            if (fullName != null)
            {
                if (dictionary_Objects.TryGetValue(fullName, out Dictionary<Guid, X> dictionary_Temp) && dictionary_Temp != null)
                {
                    if (dictionary_Temp.TryGetValue(guid, out X result) && result != null)
                    {
                        if (type.IsAssignableFrom(result.GetType()))
                        {
                            dictionary = dictionary_Temp;
                            typeName = fullName;
                            return result;
                        }
                    }
                }
            }

            List<string> typeNames = GetTypeNames(type);
            if (typeNames != null && typeNames.Count != 0)
            {
                foreach (string typeName_Temp in typeNames)
                {
                    if (dictionary_Objects.TryGetValue(typeName_Temp, out Dictionary<Guid, X> dictionary_Temp) && dictionary_Temp != null)
                    {
                        if (dictionary_Temp.TryGetValue(guid, out X result) && result != null)
                        {
                            if (type.IsAssignableFrom(result.GetType()))
                            {
                                dictionary = dictionary_Temp;
                                typeName = typeName_Temp;
                                return result;
                            }
                        }
                    }
                }
            }

            return default;
        }

        public string GetTypeName(Guid guid)
        {
            foreach (KeyValuePair<string, Dictionary<Guid, X>> keyValuePair in dictionary_Objects)
                if (keyValuePair.Value.ContainsKey(guid))
                    return keyValuePair.Key;

            return null;
        }

        public List<X> GetRelatedObjects(Guid guid)
        {
            X @object = GetObject(guid);
            if (@object == null)
                return null;

            return GetRelatedObjects(@object);
        }

        public List<X> GetRelatedObjects(LogicalOperator logicalOperator, params Guid[] guids)
        {
            if(guids == null)
            {
                return null;
            }

            if(guids.Length == 0)
            {
                return new List<X>();
            }

            Dictionary<Guid, X> dictionary = new Dictionary<Guid, X>();
            List<X> relatedObjects = null;

            switch (logicalOperator)
            {
                case LogicalOperator.Not:

                    HashSet<Guid> guids_All = Guids;
                    if (guids_All == null)
                    {
                        return null;
                    }

                    foreach (Guid guid in guids_All)
                    {
                        if (guids.Contains(guid))
                        {
                            continue;
                        }

                        relatedObjects = GetRelatedObjects(guid);
                        if (relatedObjects == null || relatedObjects.Count == 0)
                        {
                            continue;
                        }

                        foreach (X relatedObject in relatedObjects)
                        {
                            Guid guid_RelatedObject = GetGuid(relatedObject);
                            if (dictionary.ContainsKey(guid_RelatedObject))
                            {
                                continue;
                            }

                            dictionary[guid_RelatedObject] = relatedObject;
                        }
                    }

                    break;

                case LogicalOperator.And:

                    relatedObjects = GetRelatedObjects(guids[0]);
                    if (relatedObjects == null || relatedObjects.Count == 0)
                    {
                        return new List<X>();
                    }

                    foreach (X relatedObject in relatedObjects)
                    {
                        Guid guid_RelatedObject = GetGuid(relatedObject);
                        if (dictionary.ContainsKey(guid_RelatedObject))
                        {
                            continue;
                        }

                        dictionary[guid_RelatedObject] = relatedObject;
                    }

                    for (int i = 1; i < guids.Length; i++)
                    {
                        relatedObjects = GetRelatedObjects(guids[i]);
                        if (relatedObjects == null || relatedObjects.Count == 0)
                        {
                            return new List<X>();
                        }

                        HashSet<Guid> guids_RelatedObject = new HashSet<Guid>();
                        foreach (X relatedObject in relatedObjects)
                        {
                            guids_RelatedObject.Add(GetGuid(relatedObject));
                        }

                        List<Guid> guids_Temp = new List<Guid>(dictionary.Keys);
                        foreach(Guid guid in guids_Temp)
                        {
                            if(!guids_RelatedObject.Contains(guid))
                            {
                                dictionary.Remove(guid);
                            }
                        }

                        if(dictionary.Count == 0)
                        {
                            return new List<X>();
                        }
                    }

                    break;

                case LogicalOperator.Or:

                    foreach (Guid guid in guids)
                    {
                        relatedObjects = GetRelatedObjects(guid);
                        if (relatedObjects == null || relatedObjects.Count == 0)
                        {
                            continue;
                        }

                        foreach (X relatedObject in relatedObjects)
                        {
                            Guid guid_RelatedObject = GetGuid(relatedObject);
                            if (dictionary.ContainsKey(guid_RelatedObject))
                            {
                                continue;
                            }

                            dictionary[guid_RelatedObject] = relatedObject;
                        }
                    }

                    break;
            }

            return dictionary.Values?.ToList();
        }

        public List<T> GetRelatedObjects<T>(LogicalOperator logicalOperator, params Guid[] guids)
        {
            List<X> objects = GetRelatedObjects(logicalOperator, guids);
            if (objects == null)
                return null;

            List<T> result = new List<T>();
            foreach (object object_Temp in objects)
                if (object_Temp is T)
                    result.Add((T)object_Temp);

            return result;
        }

        public List<X> GetRelatedObjects(X @object)
        {
            if (@object is Guid)
                return GetRelatedObjects((Guid)(object)@object);

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

        public List<X> GetRelatedObjects(X @object, Type type)
        {
            List<X> objects = GetRelatedObjects(@object);
            if (objects == null || objects.Count == 0)
                return objects;

            return objects.FindAll(x => x != null && type.IsAssignableFrom(x.GetType()));
        }

        public List<T> GetRelatedObjects<T>(X @object) where T : X
        {
            List<X> objects = GetRelatedObjects(@object);
            if (objects == null)
                return null;

            List<T> result = new List<T>();
            foreach (X object_Temp in objects)
                if (object_Temp is T)
                    result.Add((T)object_Temp);

            return result;
        }

        public virtual int GetIndex(X @object)
        {
            if (@object == null)
            {
                return -1;
            }

            X object_Temp = @object;
            if (@object is Guid)
            {
                object_Temp = GetObject(Guid);
            }

            if (object_Temp == null)
            {
                return -1;
            }

            Guid guid = GetGuid(object_Temp);
            if (guid.Equals(Guid.Empty))
            {
                return -1;
            }

            List<string> typeNames = GetTypeNames(object_Temp.GetType());
            if (typeNames == null || typeNames.Count == 0)
            {
                return -1;
            }

            int count = 0;
            foreach (string typeName in typeNames)
            {
                if (!dictionary_Objects.TryGetValue(typeName, out Dictionary<Guid, object> dictionary) || dictionary == null)
                {
                    continue;
                }

                foreach (Guid guid_Temp in dictionary.Keys)
                {
                    if (guid_Temp.Equals(guid))
                        return count;

                    count++;
                }
            }

            return -1;
        }

        public virtual bool TryGetObject<T>(int index, T @object)
        {
            @object = default;

            List<string> typeNames = GetTypeNames(typeof(T));
            if (typeNames == null || typeNames.Count == 0)
            {
                return false;
            }

            int count = 0;
            foreach (string typeName in typeNames)
            {
                if (!dictionary_Objects.TryGetValue(typeName, out Dictionary<Guid, object> dictionary) || dictionary == null)
                {
                    continue;
                }

                foreach (Guid guid_Temp in dictionary.Keys)
                {
                    if(count == index)
                    {
                        object @object_Temp = dictionary[guid_Temp];
                        if(object_Temp is T)
                        {
                            @object = (T)object_Temp;
                            return true;
                        }

                        return false;
                    }

                    count++;
                }
            }

            return false;
        }

        public virtual int Count<T>()
        {
            List<string> typeNames = GetTypeNames(typeof(T));
            if (typeNames == null || typeNames.Count == 0)
            {
                return -1;
            }

            int count = 0;
            foreach (string typeName in typeNames)
            {
                if (!dictionary_Objects.TryGetValue(typeName, out Dictionary<Guid, object> dictionary) || dictionary == null)
                {
                    continue;
                }

                count += dictionary.Count;
            }

            return count;
        }

        public List<string> GetTypeNames(Type type)
        {
            if (dictionary_Objects == null || type == null)
            {
                return null;
            }

            List<string> result = new List<string>();
            foreach(string typeName in dictionary_Objects.Keys)
            {
                if(type.FullName.Equals(typeName))
                {
                    result.Add(typeName);
                    continue;
                }
                
                Type type_Temp = Query.Type(typeName);
                if(type_Temp == null || !type.IsAssignableFrom(type_Temp))
                {
                    continue;
                }

                result.Add(typeName);
            }

            return result;
        }

        public List<Type> GetTypes()
        {
            if (dictionary_Objects == null)
            {
                return null;
            }

            List<Type> result = new List<Type>();
            foreach (string typeName in dictionary_Objects.Keys)
            {
                Type type_Temp = Query.Type(typeName);
                if (type_Temp == null)
                {
                    continue;
                }

                result.Add(type_Temp);
            }

            return result;
        }

        public bool Contains(Type type)
        {
            if (type == null)
                return false;

            return dictionary_Objects.ContainsKey(type.FullName);
        }

        public bool Contains<T>(Guid guid)
        {
            return  GetObject(typeof(T), guid) != null;
        }

        public virtual bool IsValid(Type type)
        {
            return type != null;
        }

        public virtual RelationCluster_NEW<X> Clone()
        {
            return new RelationCluster_NEW<X>(this);
        }

        public bool IsValid(X @object)
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
            foreach (KeyValuePair<string, Dictionary<Guid, X>> keyValuePair_Type in dictionary_Objects)
            {
                JObject jObject_Objects = new JObject();
                jObject_Objects.Add("Key", keyValuePair_Type.Key);
                JArray jArray = new JArray();
                foreach (KeyValuePair<Guid, X> keyValuePair in keyValuePair_Type.Value)
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

            return jObject;
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            dictionary_Objects = new Dictionary<string, Dictionary<Guid, X>>();
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

                    List<Tuple<Guid, X>> tuples = Enumerable.Repeat(new Tuple<Guid, X>(Guid.Empty, default(X)), jArray.Count).ToList();
                    Parallel.For(0, jArray.Count, (int i) => 
                    {
                        JObject jObject_Temp = jArray[i] as JObject;
                        if (jObject_Temp == null)
                        {
                            return;
                        }

                        Guid guid = jObject_Temp.Guid("Key");
                        if (guid == Guid.Empty)
                        {
                            return;
                        }

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
                                @object = jToken.Value<DateTime>();
                                break;
                        }

                        if(@object == null && @object is X)
                        {
                            return;
                        }

                        tuples[i] = new Tuple<Guid, X>(guid, (X)@object);

                    });

                    Dictionary<Guid, X> dictionary = new Dictionary<Guid, X>();
                    foreach(Tuple<Guid, X> tuple in tuples)
                    {
                        if(tuple.Item1 == Guid.Empty && tuple.Item2 == null)
                        {
                            continue;
                        }

                        dictionary[tuple.Item1] = tuple.Item2;
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

            return true;
        }

        public bool RemoveObject(Type type, Guid guid)
        {
            if (guid == Guid.Empty || type == null)
            {
                return false;
            }

            X @object = GetObject(type, guid, out Dictionary<Guid, X> dictionary_Object, out string typeName);
            if(@object == null)
            {
                return false;
            }

            List<X> relatedObjects = GetRelatedObjects(@object);
            if (relatedObjects != null && relatedObjects.Count != 0)
            {
                foreach (X relatedObject in relatedObjects)
                {
                    RemoveRelation(@object, relatedObject);
                }
            }

            if(dictionary_Relations.TryGetValue(typeName, out Dictionary<Guid, HashSet<Guid>> dictionary_Relation))
            {
                dictionary_Relation.Remove(guid);
            }

            return dictionary_Object.Remove(guid);
        }

        public bool RemoveAll<T>() where T: X
        {
            List<T> ts = GetObjects<T>();
            if(ts == null || ts.Count == 0)
            {
                return false;
            }

            bool result = false;
            foreach(T t in ts)
            {
                Guid guid = GetGuid(t);
                if(RemoveObject<T>(guid))
                {
                    result = true;
                }
            }

            return result;
        }

        public bool RemoveObject<T>(Guid guid)
        {
            return RemoveObject(typeof(T), guid);
        }

        public bool RemoveRelation(X object_1, X object_2)
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

        public List<object> GetValues(IComplexReference complexReference)
        {
            return GetValues(complexReference, default);
        }

        private List<object> GetValues(IComplexReference complexReference, X parent)
        {
            if (complexReference == null)
            {
                return null;
            }

            ObjectReference objectReference = complexReference is PathReference ? ((PathReference)complexReference).FirstOrDefault() : complexReference as ObjectReference;
            if (objectReference == null)
            {
                return null;
            }

            List<object> objects = null;
            if (objectReference is PropertyReference)
            {
                if (parent == null)
                {
                    objects = GetObjects(objectReference)?.ConvertAll(x => x as object);
                }
                else if (string.IsNullOrEmpty(objectReference.TypeName) && (objectReference.Reference == null || !objectReference.Reference.HasValue))
                {
                    objects = new List<object>() { parent };
                }
                else if (objectReference.Type == parent.GetType())
                {
                    objects = new List<object>() { parent };
                }
                else
                {
                    objects = GetObjects(objectReference, parent)?.ConvertAll(x => x as object); ;
                }

                if (objects == null || objects.Count == 0)
                {
                    return objects;
                }

                string propertyName = ((PropertyReference)objectReference).PropertyName;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    List<object> objects_Temp = new List<object>();
                    foreach (object @object in objects)
                    {
                        if (!Query.TryGetValue(@object, propertyName, out object value))
                        {
                            continue;
                        }

                        objects_Temp.Add(value);
                    }

                    objects = objects_Temp;
                }
            }
            else
            {
                objects = GetObjects(objectReference, parent)?.ConvertAll(x => x as object); ;
            }

            if (objects == null || objects.Count == 0)
            {
                return objects;
            }

            if (complexReference is PathReference)
            {
                List<ObjectReference> objectReferences = new List<ObjectReference>((PathReference)complexReference);
                objectReferences?.Remove(objectReference);

                if (objectReferences != null && objectReferences.Count != 0)
                {
                    PathReference pathReference = new PathReference(objectReferences);

                    List<object> objects_Temp = new List<object>();
                    foreach (object @object in objects)
                    {
                        if(@object is X)
                        {
                            List<object> objects_Temp_Temp = GetValues(pathReference, (X)@object);
                            if (objects_Temp_Temp != null)
                            {
                                objects_Temp.AddRange(objects_Temp_Temp);
                            }
                        }

                    }

                    objects = objects_Temp;
                }
            }

            return objects;
        }

        public bool TryGetValues(X @object, IComplexReference complexReference, out List<object> values)
        {
            values = null;
            if (complexReference == null || @object == null)
            {
                return false;
            }

            Guid guid = GetGuid(@object);
            if (guid == null || guid == System.Guid.Empty)
            {
                return false;
            }

            ObjectReference objectReference_First = null;
            if (complexReference is ObjectReference)
            {
                objectReference_First = (ObjectReference)complexReference;
            }
            if (complexReference is PathReference)
            {
                PathReference pathReference = (PathReference)complexReference;
                if (pathReference.Count() != 0)
                {
                    objectReference_First = pathReference.First();
                }
            }

            if (objectReference_First == null)
            {
                return false;
            }

            ObjectReference objectReference_Temp = objectReference_First;
            if (objectReference_Temp is PropertyReference)
            {
                objectReference_Temp = new ObjectReference(objectReference_Temp);
            }

            List<object> objects = GetValues(objectReference_Temp);
            if (objects == null || objects.Count == 0)
            {
                return false;
            }

            if (objects.Find(x => x is X && GetGuid((X)x) == guid) == null)
            {
                return false;
            }

            if (objectReference_First is PropertyReference)
            {
                PropertyReference propertyReference = (PropertyReference)objectReference_First;
                objectReference_First = new PropertyReference(propertyReference.TypeName, new Reference(guid), propertyReference.PropertyName);

            }
            else if (objectReference_First is ObjectReference)
            {
                objectReference_Temp = (ObjectReference)objectReference_First;
                objectReference_First = new ObjectReference(objectReference_Temp.TypeName, new Reference(guid));
            }

            IComplexReference complexReference_Temp = objectReference_First;
            if (complexReference is PathReference)
            {
                PathReference pathReference_Temp = (PathReference)complexReference;
                List<ObjectReference> objectReferences = new List<ObjectReference>(pathReference_Temp);
                objectReferences[0] = objectReference_First;

                complexReference_Temp = new PathReference(objectReferences);
            }

            values = GetValues(complexReference_Temp);
            return true;
        }
    }
}
