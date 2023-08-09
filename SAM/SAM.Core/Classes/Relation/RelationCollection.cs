using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class RelationCollection : IEnumerable<Relation>, IJSAMObject
    {
        private List<Relation> relations = new List<Relation>();

        public RelationCollection()
        {

        }

        public RelationCollection(IEnumerable<Relation> relations)
        {
            if(relations != null)
            {
                this.relations = relations.ToList().ConvertAll(x => x == null ? null : new Relation(x));
            }
        }

        public RelationCollection(RelationCollection relationCollection)
        {
            relations = relationCollection?.relations == null ? null : relationCollection.relations.ConvertAll(x => x == null ? null : new Relation(x));
        }

        public RelationCollection(JObject jObject)
        {
            FromJObject(jObject);
        }

        public IEnumerator<Relation> GetEnumerator()
        {
            return relations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return relations.GetEnumerator();
        }

        public void Clear()
        {
            relations.Clear();
        }

        public bool Add(Relation relation)
        {
            if(relation == null)
            {
                return false;
            }

            relations.Add(relation);
            return true;
        }

        public Relation Add(string id, Reference reference_1, Reference reference_2)
        {
            Relation result = new Relation(id, reference_1, reference_2);

            relations.Add(result);

            return result;
            
        }

        public Relation Add(string id, IEnumerable<Reference> references_1, IEnumerable<Reference> references_2)
        {
            Relation result = new Relation(id, references_1, references_2);

            relations.Add(result);

            return result;
        }

        public RelationCollection FindAll(string id)
        {
            if(relations == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            RelationCollection result = new RelationCollection();
            foreach (Relation relation in relations)
            {
                if(relation == null)
                {
                    continue;
                }

                if(relation.Id == id)
                {
                    result.Add(relation);
                }
            }

            return result;
        }

        public RelationCollection FindAll(RelationType relationType)
        {
            RelationCollection result = new RelationCollection();
            foreach (Relation relation in relations)
            {
                if (relation.RelationType == relationType)
                {
                    result.Add(relation);
                }
            }

            return result;
        }

        public RelationCollection FindAll(Reference reference)
        {
            RelationCollection result = new RelationCollection();
            foreach (Relation relation in relations)
            {
                if (relation.Contains(reference))
                {
                    result.Add(relation);
                }
            }

            return result;
        }

        public RelationCollection FindAll_1(Reference reference)
        {
            RelationCollection result = new RelationCollection();
            foreach (Relation relation in relations)
            {
                if(relation.Contains_1(reference))
                {
                    result.Add(relation);
                }
            }

            return result;
        }

        public RelationCollection FindAll_2(Reference reference)
        {
            RelationCollection result = new RelationCollection();
            foreach (Relation relation in relations)
            {
                if (relation.Contains_2(reference))
                {
                    result.Add(relation);
                }
            }

            return result;
        }

        public RelationCollection FindAll(Func<Relation, bool> func)
        {
            if(func == null)
            {
                return new RelationCollection(relations);
            }

            return new RelationCollection(relations.FindAll(x => func.Invoke(x)));
        }

        public Relation Find(string id)
        {
            if (relations == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (Relation relation in relations)
            {
                if (relation == null)
                {
                    continue;
                }

                if (relation.Id == id)
                {
                    return relation;
                }
            }

            return default;
        }

        public Relation Find(string id, Reference reference)
        {
            if (relations == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            foreach (Relation relation in relations)
            {
                if (relation == null)
                {
                    continue;
                }

                if (relation.Id == id && relation.Contains(reference))
                {
                    return relation;
                }
            }

            return default;
        }

        public bool Remove(Relation relation)
        {
            if(relation == null)
            {
                return false;
            }

            return relations.Remove(relation);
        }

        public bool Remove(Reference reference)
        {
            bool result = false;

            for (int i = relations.Count - 1; i >= 0; i--)
            {
                Relation relation = relations[i];

                if (relation == null)
                {
                    continue;
                }

                if (relation.Contains(reference))
                {
                    HashSet<Reference> references_1 = relation.References_1;
                    references_1.Remove(reference);

                    HashSet<Reference> references_2 = relation.References_2;
                    references_2.Remove(reference);

                    string id = relation.Id;

                    if (references_1.Count == 0 || references_2.Count == 0)
                    {
                        relations.RemoveAt(i);
                    }
                    else
                    {
                        relations[i] = new Relation(id, references_1, references_2);
                    }

                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Removes relations between two references. All relations will be removed if id not provided
        /// </summary>
        /// <param name="reference_1">First reference</param>
        /// <param name="reference_2">Second reference</param>
        /// <param name="id">Relation Id</param>
        /// <returns>True if any references have been removed</returns>
        public bool Remove(Reference reference_1, Reference reference_2, string id = null)
        {
            if(relations == null || relations.Count == 0)
            {
                return false;
            }

            bool result = false;

            for(int i = relations.Count -1; i >= 0; i--)
            {
                Relation relation = relations[i];
                
                if(id != null && relation.Id != id)
                {
                    continue;
                }
                
                if((relation.Contains_1(reference_1) && relation.Contains_2(reference_2)) || (relation.Contains_1(reference_2) && relation.Contains_2(reference_1)))
                {
                    HashSet<Reference> references_1 = relation.References_1;
                    HashSet<Reference> references_2 = relation.References_2;

                    bool removed = false;
                    
                    if (references_1.Contains(reference_1) && references_2.Contains(reference_2))
                    {
                        references_1.Remove(reference_1);
                        references_2.Remove(reference_2);
                        removed = true;
                    }
                    
                    if(references_1.Contains(reference_2) && references_2.Contains(reference_1))
                    {
                        references_1.Remove(reference_2);
                        references_2.Remove(reference_1);
                        removed = true;
                    }

                    if(!removed)
                    {
                        continue;
                    }

                    result = true;

                    if (references_1 == null || references_2 == null || references_1.Count == 0 || references_2.Count == 0)
                    {
                        relations.RemoveAt(i);
                    }
                    else
                    {
                        relations[i] = new Relation(relation.Id, references_1, references_2);
                    }
                }
            }

            return result;
        }

        public bool Replace(Reference reference_ToBeReplaced, Reference reference)
        {
            if(!reference_ToBeReplaced.IsValid() || !reference.IsValid())
            {
                return false;
            }

            bool result = false;

            for (int i = relations.Count - 1; i >= 0; i--)
            {
                Relation relation = relations[i];

                if (relation == null)
                {
                    continue;
                }

                if (relation.Contains(reference_ToBeReplaced))
                {
                    HashSet<Reference> references_1 = relation.References_1;
                    if(references_1.Remove(reference_ToBeReplaced))
                    {
                        references_1.Add(reference);
                    }

                    HashSet<Reference> references_2 = relation.References_2;
                    if(references_2.Remove(reference_ToBeReplaced))
                    {
                        references_2.Add(reference);
                    }

                    relations[i] = new Relation(relation.Id, references_1, references_2);

                    result = true;
                }
            }

            return result;
        }

        public bool Copy(Reference reference_ToBeCopied, Reference reference)
        {
            if (!reference_ToBeCopied.IsValid() || !reference.IsValid())
            {
                return false;
            }

            bool result = false;

            for (int i = relations.Count - 1; i >= 0; i--)
            {
                Relation relation = relations[i];

                if (relation == null)
                {
                    continue;
                }

                if (relation.Contains(reference_ToBeCopied))
                {
                    HashSet<Reference> references_1 = relation.References_1;
                    if (references_1.Remove(reference_ToBeCopied))
                    {
                        references_1.Add(reference);
                    }

                    HashSet<Reference> references_2 = relation.References_2;
                    if (references_2.Remove(reference_ToBeCopied))
                    {
                        references_2.Add(reference);
                    }

                    Relation relation_New = new Relation(relation.Id, references_1, references_2);

                    relations.Add(relation_New);

                    result = true;
                }
            }

            return result;
        }

        //public bool Insert(Reference reference_ToBeInserted, Reference reference_1, Reference reference_2, string id = null)
        //{
        //    if (relations == null || relations.Count == 0)
        //    {
        //        return false;
        //    }

        //    bool result = false;

        //    for (int i = relations.Count - 1; i >= 0; i--)
        //    {
        //        Relation relation = relations[i];

        //        if (id != null && relation.Id != id)
        //        {
        //            continue;
        //        }

        //        if ((relation.Contains_1(reference_1) && relation.Contains_2(reference_2)) || (relation.Contains_1(reference_2) && relation.Contains_2(reference_1)))
        //        {
        //            HashSet<Reference> references_1 = relation.References_1;
        //            HashSet<Reference> references_2 = relation.References_2;

        //            bool added = false;

        //            if (references_1.Contains(reference_1) && references_2.Contains(reference_2))
        //            {
        //                references_1.Remove(reference_1);
        //                references_1.Add(reference_ToBeInserted);

        //                references_2.Remove(reference_2);
        //                references_2.Add(reference_ToBeInserted);

        //                added = true;
        //            }

        //            if (references_1.Contains(reference_2) && references_2.Contains(reference_1))
        //            {
        //                references_1.Remove(reference_2);
        //                references_1.Add(reference_ToBeInserted);

        //                references_2.Remove(reference_1);
        //                references_2.Add(reference_ToBeInserted);
        //                added = true;
        //            }

        //            if (!added)
        //            {
        //                continue;
        //            }

        //            result = true;

        //            relations[i] = new Relation(relation.Id, references_1, references_2);
        //        }
        //    }

        //    return result;
        //}

        public HashSet<Reference> GetRelatedReferences(Reference reference, string relationId = null)
        {
            if (!reference.IsValid())
            {
                return null;
            }

            RelationCollection relationCollection_Temp = FindAll(reference);
            if (relationCollection_Temp == null)
            {
                return null;
            }

            HashSet<Reference> result = new HashSet<Reference>();

            foreach (Relation relation in relationCollection_Temp)
            {
                if (relationId != null && relation.Id != relationId)
                {
                    continue;
                }

                if (relation.Contains_1(reference))
                {
                    foreach (Reference reference_2 in relation.References_2)
                    {
                        result.Add(reference_2);
                    }
                }

                if (relation.Contains_2(reference))
                {
                    foreach (Reference reference_1 in relation.References_1)
                    {
                        result.Add(reference_1);
                    }
                }
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Relations"))
            {
                JArray jArray = jObject.Value<JArray>("Relations");
                if(jArray != null)
                {
                    relations = new List<Relation>();
                    foreach(JObject jObject_Relation in jArray)
                    {
                        Relation relation = Query.IJSAMObject<Relation>(jObject);
                        if (relation != null)
                        {
                            relations.Add(relation);
                        }
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Query.FullTypeName(this));

            if(relations != null)
            {
                JArray jArray = new JArray();
                foreach (Relation relation in relations)
                {
                    jArray.Add(relation.ToJObject());
                }

                result.Add("Relations", jArray);
            }

            return result;
        }


        public static implicit operator RelationCollection(List<Relation> relations)
        {
            if(relations == null)
            {
                return null;
            }

            RelationCollection result = new RelationCollection();
            result.relations = relations.ToList().ConvertAll(x => x == null ? null : new Relation(x));

            return result;
        }
        
        public static implicit operator RelationCollection(Relation[] relations)
        {
            if (relations == null)
            {
                return null;
            }

            return new RelationCollection(relations);
        }
    }
}
