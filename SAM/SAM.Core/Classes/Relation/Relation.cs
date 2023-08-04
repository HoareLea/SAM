using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class Relation : IJSAMObject
    {
        private string id = null;
        private HashSet<Reference> references_1 = null;
        private HashSet<Reference> references_2 = null;

        public Relation(string id, IEnumerable<Reference> references_1, IEnumerable<Reference> references_2)
        {
            this.id = id;
            this.references_1 = references_1 == null ? null : new HashSet<Reference>(references_1);
            this.references_2 = references_2 == null ? null : new HashSet<Reference>(references_2);
        }

        public Relation(string id, Reference reference, IEnumerable<Reference> references)
        {
            this.id = id;
            references_1 = new HashSet<Reference>();
            references_1.Add(reference);
            references_2 = references == null ? null : new HashSet<Reference>(references);
        }

        public Relation(string id, IEnumerable<Reference> references, Reference reference)
        {
            this.id = id;
            references_2 = new HashSet<Reference>();
            references_2.Add(reference);
            references_1 = references == null ? null : new HashSet<Reference>(references);
        }

        public Relation(string id, Reference reference_1, Reference reference_2)
        {
            this.id = id;

            references_1 = new HashSet<Reference>();
            references_1.Add(reference_1);

            references_2 = new HashSet<Reference>();
            references_2.Add(reference_2);
        }

        public Relation(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Relation(Relation relation)
        {
            if(relation != null)
            {
                id = relation.Id;
                references_1 = relation.references_1 == null ? null : new HashSet<Reference>(relation.references_1);
                references_2 = relation.references_2 == null ? null : new HashSet<Reference>(relation.references_2);
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public HashSet<Reference> References
        {
            get
            {
                if(references_1 == null && references_2 == null)
                {
                    return null;
                }

                HashSet<Reference> result = new HashSet<Reference>();
                if(references_1 != null)
                {
                    foreach(Reference reference in references_1)
                    {
                        result.Add(reference);
                    }
                }

                if (references_2 != null)
                {
                    foreach (Reference reference in references_2)
                    {
                        result.Add(reference);
                    }
                }

                return result;
            }
        }

        public HashSet<Reference> References_1
        {
            get
            {
                return references_1 == null ? null : new HashSet<Reference>(references_1);
            }
        }

        public HashSet<Reference> References_2
        {
            get
            {
                return references_2 == null ? null : new HashSet<Reference>(references_2);
            }
        }

        public bool Contains(Reference reference)
        {
            return Contains_1(reference) || Contains_2(reference);
        }

        public bool Contains_1(Reference reference)
        {
            return references_1 == null ? false : references_1.Contains(reference);
        }

        public bool Contains_2(Reference reference)
        {
            return references_2 == null ? false : references_2.Contains(reference);
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("Id"))
            {
                id = jObject.Value<string>("Id");
            }

            if(jObject.ContainsKey("References_1"))
            {
                JArray jArray = jObject.Value<JArray>("References_1");
                if(jArray != null)
                {
                    references_1 = new HashSet<Reference>();
                    foreach(string value in jArray)
                    {
                        references_1.Add(value);
                    }
                }
            }

            if (jObject.ContainsKey("References_2"))
            {
                JArray jArray = jObject.Value<JArray>("References_2");
                if (jArray != null)
                {
                    references_2 = new HashSet<Reference>();
                    foreach (string value in jArray)
                    {
                        references_2.Add(value);
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Query.FullTypeName(this));
            
            if(id != null)
            {
                result.Add("Id", id);
            }

            if(references_1 != null)
            {
                JArray jArray = new JArray();
                foreach(Reference reference in references_1)
                {
                    jArray.Add(reference.ToString());
                }
                result.Add("References_1", jArray);
            }

            if (references_2 != null)
            {
                JArray jArray = new JArray();
                foreach (Reference reference in references_2)
                {
                    jArray.Add(reference.ToString());
                }
                result.Add("References_2", jArray);
            }

            return result;
        }

        public RelationType RelationType
        {
            get
            {
                return Query.RelationType(references_1 == null ? 0 : references_1.Count, references_2 == null ? 0 : references_2.Count);
            }
        }
    }
}
