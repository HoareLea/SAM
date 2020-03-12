using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public class SAMRelationCluster : SAMObject
    {
        private Dictionary<Type, Dictionary<Type, HashSet<SAMRelation>>> dictionary;

        public SAMRelationCluster(SAMRelationCluster sAMRelationCluster)
            : base(sAMRelationCluster)
        {
            if (sAMRelationCluster == null || sAMRelationCluster.dictionary == null)
                return;

            dictionary = new Dictionary<Type, Dictionary<Type, HashSet<SAMRelation>>>();

            foreach(KeyValuePair<Type, Dictionary<Type, HashSet<SAMRelation>>> keyValuePair in sAMRelationCluster.dictionary)
            {
                dictionary[keyValuePair.Key] = new Dictionary<Type, HashSet<SAMRelation>>();
            }
        }

        public SAMRelationCluster(IEnumerable<SAMRelation> sAMRelations)
        {
            dictionary = new Dictionary<Type, Dictionary<Type, HashSet<SAMRelation>>>();

            if (sAMRelations != null)
            {
                foreach (SAMRelation sAMRelation in sAMRelations)
                    Add(sAMRelation);
            }
        }

        public virtual bool Add(SAMRelation sAMRelation)
        {
            object @object = sAMRelation.GetObject<object>();
            if (@object == null)
                return false;

            object relatedObject = sAMRelation.GetRelatedObject<object>();
            if (relatedObject == null)
                return false;

            Dictionary<Type, HashSet<SAMRelation>> dictionary_Temp;

            if (!dictionary.TryGetValue(@object.GetType(), out dictionary_Temp))
            {
                dictionary_Temp = new Dictionary<Type, HashSet<SAMRelation>>();
                dictionary[@object.GetType()] = dictionary_Temp;
            }

            HashSet<SAMRelation> sAMRelations_Temp;
            if (!dictionary_Temp.TryGetValue(relatedObject.GetType(), out sAMRelations_Temp))
            {
                sAMRelations_Temp = new HashSet<SAMRelation>();
                dictionary_Temp[relatedObject.GetType()] = sAMRelations_Temp;
            }

            sAMRelations_Temp.Add(sAMRelation);
            return true;
        }

        public List<SAMRelation> GetSAMRelations<X, Z>()
        {
            if (dictionary == null)
                return null;

            List<SAMRelation> result = new List<SAMRelation>();
            foreach (KeyValuePair<Type, Dictionary<Type, HashSet<SAMRelation>>> keyValuePair_1 in dictionary)
            {
                if (!keyValuePair_1.Key.IsAssignableFrom(typeof(X)))
                    continue;

                foreach (KeyValuePair<Type, HashSet<SAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                {
                    if (!keyValuePair_2.Key.IsAssignableFrom(typeof(Z)))
                        continue;

                    foreach (SAMRelation sAMRelation in keyValuePair_2.Value)
                        result.Add(new SAMRelation(sAMRelation));
                }

            }
            return result;
        }

        public List<SAMRelation> GetSAMRelations()
        {
            if (dictionary == null)
                return null;

            List<SAMRelation> result = new List<SAMRelation>();
            foreach (KeyValuePair<Type, Dictionary<Type, HashSet<SAMRelation>>> keyValuePair_1 in dictionary)
            {
                foreach (KeyValuePair<Type, HashSet<SAMRelation>> keyValuePair_2 in keyValuePair_1.Value)
                    result.AddRange(keyValuePair_2.Value);
            }
            return result;
        }
    }
}
