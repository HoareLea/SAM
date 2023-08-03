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

        
        public Reference? AddObject(T @object)
        {
            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            dictionary[reference.Value] = @object;
            return reference;
        }

        
        public Relation AddRelation(string id, T @object_1, T @object_2)
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

        public Relation AddRelation(string id, Reference reference_1, Reference reference_2)
        {
            if(!dictionary.ContainsKey(reference_1) || !dictionary.ContainsKey(reference_2))
            {
                return null;
            }

            return relationCollection.Add(id, reference_1, reference_2);
        }


        public T GetObject(Reference reference)
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

        
        public bool RemoveObject(T @object)
        {
            Reference? reference = GetReference(@object);
            if(reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return false;
            }

            return RemoveObject(reference.Value);
        }

        public bool RemoveObject(Reference reference)
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

        
        public RelationCollection GetRelations(Reference reference)
        {
            if(!reference.IsValid())
            {
                return null;
            }

            return relationCollection.FindAll(reference);
        }

        public RelationCollection GetRelations(T @object)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelations(reference.Value);
        }

        
        public HashSet<Reference> GetRelatedReferences(Reference reference, string relationId = null)
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

        
        public List<T> GetRelatedObjects(Reference reference, string relationId = null)
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

        public List<T> GetRelatedObjects(T @object, string relationId = null)
        {
            Reference? reference = GetReference(@object);
            if (reference == null || !reference.HasValue || !reference.Value.IsValid())
            {
                return null;
            }

            return GetRelatedObjects(reference.Value, relationId);
        }


        public abstract Reference? GetReference(T @object);
    }
}
