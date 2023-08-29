using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public abstract class RelationModel<T>
    {
        private Dictionary<Reference, T> dictionary;
        private RelationCollection relationCollection;

        public RelationModel()
        {
            dictionary = new Dictionary<Reference, T>();
            relationCollection = new RelationCollection();
        }

        public RelationModel(IEnumerable<T> objects, IEnumerable<Relation> relations)
        {
            if(objects != null)
            {
                foreach(T @object in objects)
                {
                    AddObject(@object);
                }

                if(relations != null)
                {
                    relationCollection = new RelationCollection(relations);
                }
            }
        }

        public RelationModel(RelationModel<T> relationModel)
        {
            if(relationModel != null && relationModel.dictionary != null)
            {
                if(relationModel.dictionary != null)
                {
                    dictionary = new Dictionary<Reference, T>();
                    foreach (KeyValuePair<Reference, T> keyValuePair in relationModel.dictionary)
                    {
                        T @object = keyValuePair.Value;
                        dictionary[keyValuePair.Key] = @object;
                    }
                }

                if(relationModel.relationCollection != null)
                {
                    relationCollection = new RelationCollection(relationModel.relationCollection);

                }
            }
        }
        
        
        protected Reference? AddObject(T @object)
        {
            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            dictionary[reference.Value] = @object;
            return reference;
        }

        
        protected Relation AddRelation(string id, T @object_1, T @object_2)
        {
            Reference? reference_1 = GetReference(@object_1);
            if(reference_1 == null || !reference_1.HasValue || !reference_1.Value.IsValid())
            {
                return null;
            }

            Reference? reference_2 = GetReference(@object_2);
            if (reference_2 == null || !reference_2.HasValue || !reference_2.Value.IsValid())
            {
                return null;
            }

            if(!dictionary.ContainsKey(reference_1.Value))
            {
                dictionary[reference_1.Value] = @object_1;
            }

            if (!dictionary.ContainsKey(reference_2.Value))
            {
                dictionary[reference_2.Value] = @object_2;
            }

            return AddRelation(id, reference_1.Value, reference_2.Value);
        }

        protected Relation AddRelation(string id, Reference reference_1, Reference reference_2)
        {
            if(!dictionary.ContainsKey(reference_1) || !dictionary.ContainsKey(reference_2))
            {
                return null;
            }

            return relationCollection.Add(id, reference_1, reference_2);
        }

        protected Relation AddRelation(Relation relation)
        {
            if(relation == null)
            {
                return null;
            }

            HashSet<Reference> references_1 = relation.References_1;
            if(references_1 != null)
            {
                HashSet<Reference> references_Temp = new HashSet<Reference>();
                foreach(Reference reference in references_1)
                {
                    if(dictionary.ContainsKey(reference))
                    {
                        references_Temp.Add(reference);
                    }
                }
                references_1 = references_Temp;
            }

            HashSet<Reference> references_2 = relation.References_2;
            if (references_2 != null)
            {
                HashSet<Reference> references_Temp = new HashSet<Reference>();
                foreach (Reference reference in references_2)
                {
                    if (dictionary.ContainsKey(reference))
                    {
                        references_Temp.Add(reference);
                    }
                }
                references_2 = references_Temp;
            }

            if(references_1 != null && references_1.Count != 0 && references_2 != null && references_2.Count != 0)
            {
                Relation result = new Relation(relation.Id, references_1, references_2);
                relationCollection.Add(result);
                return result;
            }

            return null;
        }


        protected T GetObject(Reference reference)
        {
            if(!reference.IsValid())
            {
                return default;
            }

            if(!dictionary.ContainsKey(reference))
            {
                return default;
            }

            return dictionary[reference];
        }

        protected List<T> GetObjects(Func<T, bool> func = null)
        {
            if(dictionary == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(T @object in dictionary.Values)
            {
                if(func != null && !func.Invoke(@object))
                {
                    continue;
                }

                result.Add(@object);
            }

            return result;
        }


        protected bool RemoveObject(T @object)
        {
            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return false;
            }

            return RemoveObject(reference.Value);
        }

        protected bool RemoveObject(Reference reference)
        {
            if(!reference.IsValid())
            {
                return false;
            }

            bool result = dictionary.Remove(reference);
            if(result)
            {
                relationCollection.Remove(reference);
            }

            return result;
        }


        protected bool RemoveRelations(string id)
        {
            return relationCollection == null ? false : relationCollection.Remove(id);
        }

        
        protected RelationCollection GetRelations(Func<Relation, bool> func = null)
        {
            return relationCollection?.FindAll(func);
        }

        protected RelationCollection GetRelations(Reference reference)
        {
            if(!reference.IsValid())
            {
                return null;
            }

            return relationCollection.FindAll(reference);
        }


        protected RelationCollection GetRelations_1(Reference reference)
        {
            if (!reference.IsValid())
            {
                return null;
            }

            return relationCollection.FindAll_1(reference);
        }

        protected RelationCollection GetRelations_2(Reference reference)
        {
            if (!reference.IsValid())
            {
                return null;
            }

            return relationCollection.FindAll_2(reference);
        }

        protected RelationCollection GetRelations(T @object)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelations(reference.Value);
        }

        protected RelationCollection GetRelations_1(T @object)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelations_1(reference.Value);
        }

        protected RelationCollection GetRelations_2(T @object)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelations_2(reference.Value);
        }


        protected HashSet<Reference> GetRelatedReferences(Reference reference, string relationId = null)
        {
            if(!reference.IsValid())
            {
                return null;
            }

            if(!dictionary.ContainsKey(reference))
            {
                return null;
            }

            return relationCollection.GetRelatedReferences(reference, relationId);
        }

        protected HashSet<Reference> GetRelatedReferences_1(Reference reference, string relationId = null)
        {
            if (!reference.IsValid())
            {
                return null;
            }

            if (!dictionary.ContainsKey(reference))
            {
                return null;
            }

            return relationCollection.GetRelatedReferences_1(reference, relationId);
        }

        protected HashSet<Reference> GetRelatedReferences_2(Reference reference, string relationId = null)
        {
            if (!reference.IsValid())
            {
                return null;
            }

            if (!dictionary.ContainsKey(reference))
            {
                return null;
            }

            return relationCollection.GetRelatedReferences_2(reference, relationId);
        }

        protected HashSet<Reference> GetRelatedReferences(string relationId)
        {
            HashSet<Reference> result = new HashSet<Reference>();
            foreach (Relation relation in relationCollection)
            {
                if(relation == null || relation.Id != relationId)
                {
                    continue;
                }

                HashSet<Reference> references = relation.References;
                if(references != null)
                {
                    foreach(Reference reference in references)
                    {
                        result.Add(reference);
                    }
                }
            }

            return result;
        }

        
        protected List<T> GetRelatedObjects(string relationId)
        {
            HashSet<Reference> references = GetRelatedReferences(relationId);
            if(references == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(Reference reference in references)
            {
                if(dictionary.TryGetValue(reference, out T @object))
                {
                    result.Add(@object);
                }
            }

            return result;
        }

        protected List<T> GetRelatedObjects(Reference reference, string relationId = null)
        {
            HashSet<Reference> references = GetRelatedReferences(reference, relationId);
            if(references == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach(Reference reference_Temp in references)
            {
                if(dictionary.TryGetValue(reference_Temp, out T @object))
                {
                    result.Add(@object);
                }
            }

            return result;
        }

        protected List<T> GetRelatedObjects_1(Reference reference, string relationId = null)
        {
            HashSet<Reference> references = GetRelatedReferences_1(reference, relationId);
            if (references == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (Reference reference_Temp in references)
            {
                if (dictionary.TryGetValue(reference_Temp, out T @object))
                {
                    result.Add(@object);
                }
            }

            return result;
        }

        protected List<T> GetRelatedObjects_2(Reference reference, string relationId = null)
        {
            HashSet<Reference> references = GetRelatedReferences_2(reference, relationId);
            if (references == null)
            {
                return null;
            }

            List<T> result = new List<T>();
            foreach (Reference reference_Temp in references)
            {
                if (dictionary.TryGetValue(reference_Temp, out T @object))
                {
                    result.Add(@object);
                }
            }

            return result;
        }

        protected List<T> GetRelatedObjects(T @object, string relationId = null)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelatedObjects(reference.Value, relationId);
        }

        protected List<T> GetRelatedObjects_1(T @object, string relationId = null)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelatedObjects_1(reference.Value, relationId);
        }

        protected List<T> GetRelatedObjects_2(T @object, string relationId = null)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelatedObjects_2(reference.Value, relationId);
        }


        protected bool Contains(T @object)
        {
            if(dictionary == null || dictionary.Count == 0)
            {
                return false;
            }

            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue)
            {
                return false;
            }

            return Contains(reference.Value);
        }

        protected bool Contains(Reference reference)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return false;
            }

            return dictionary.ContainsKey(reference);
        }


        protected bool Replace(T @object_ToBeReplaced, T @object)
        {
            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return false;
            }

            Reference? reference_ToBeReplaced = GetReference(@object_ToBeReplaced);
            if (reference_ToBeReplaced == null || !reference_ToBeReplaced.HasValue || !reference_ToBeReplaced.Value.IsValid())
            {
                return false;
            }

            if(!dictionary.ContainsKey(reference_ToBeReplaced.Value))
            {
                return false;
            }

            dictionary.Remove(reference_ToBeReplaced.Value);
            dictionary[reference.Value] = @object;

            relationCollection.Replace(reference_ToBeReplaced.Value, reference.Value);
            return true;
        }


        protected bool Copy(T object_ToBeCopied, T @object)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return false;
            }

            Reference? reference_ToBeReplaced = GetReference(object_ToBeCopied);
            if (reference_ToBeReplaced == null || !reference_ToBeReplaced.HasValue || !reference_ToBeReplaced.Value.IsValid())
            {
                return false;
            }

            if (!dictionary.ContainsKey(reference_ToBeReplaced.Value))
            {
                return false;
            }

            dictionary[reference.Value] = @object;

            relationCollection.Copy(reference_ToBeReplaced.Value, reference.Value);
            return true;
        }


        public abstract Reference? GetReference(T @object);
    }
}
