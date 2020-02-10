using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public class SAMRelationCluster<T, Y>
    {
        private Dictionary<Type, Dictionary<Type, HashSet<ISAMRelation>>> dictionary;

        public SAMRelationCluster(IEnumerable<ISAMRelation> sAMRelations)
        {
            dictionary = new Dictionary<Type, Dictionary<Type, HashSet<ISAMRelation>>>();

            if (sAMRelations != null)
            {
                foreach (ISAMRelation sAMRelation in sAMRelations)
                    Add(sAMRelation);
            }
        }

        public bool Add(ISAMRelation sAMRelation)
        {
            object @object = sAMRelation.GetObject<object>();
            if (@object == null)
                return false;

            object relatedObject = sAMRelation.GetRelatedObject<object>();
            if (relatedObject == null)
                return false;

            Dictionary<Type, HashSet<ISAMRelation>> dictionary_Temp;

            if (!dictionary.TryGetValue(@object.GetType(), out dictionary_Temp))
            {
                dictionary_Temp = new Dictionary<Type, HashSet<ISAMRelation>>();
                dictionary[@object.GetType()] = dictionary_Temp;
            }

            HashSet<ISAMRelation> sAMRelations_Temp;
            if (!dictionary_Temp.TryGetValue(relatedObject.GetType(), out sAMRelations_Temp))
            {
                sAMRelations_Temp = new HashSet<ISAMRelation>();
                dictionary_Temp[relatedObject.GetType()] = sAMRelations_Temp;
            }

            sAMRelations_Temp.Add(sAMRelation);
            return true;
        }

        public List<SAMRelation<X, Z>> GetSAMRelations<X, Z>() where X : T where Z : Y
        {
            if (dictionary == null)
                return null;

            List<SAMRelation<X, Z>> result = new List<SAMRelation<X, Z>>();
            foreach (KeyValuePair<Type, Dictionary<Type, HashSet<ISAMRelation>>> keyValuePair_1 in dictionary)
            {
                if (!keyValuePair_1.Key.IsAssignableFrom(typeof(X)))
                    continue;

                foreach (KeyValuePair<Type, HashSet<ISAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                {
                    if (!keyValuePair_2.Key.IsAssignableFrom(typeof(Y)))
                        continue;

                    foreach (ISAMRelation sAMRelation in keyValuePair_2.Value)
                        result.Add(new SAMRelation<X, Z>(sAMRelation));
                }

            }
            return result;
        }

        public List<ISAMRelation> GetSAMRelations()
        {
            if (dictionary == null)
                return null;

            List<ISAMRelation> result = new List<ISAMRelation>();
            foreach (KeyValuePair<Type, Dictionary<Type, HashSet<ISAMRelation>>> keyValuePair_1 in dictionary)
            {
                foreach (KeyValuePair<Type, HashSet<ISAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                    result.AddRange(keyValuePair_2.Value);
            }
            return result;
        }
    }
}
