using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class IndexedComplexModifier : IndexedModifier, IIndexedComplexModifier
    {
        public IndexedComplexModifier()
            :base()
        {

        }

        public IndexedComplexModifier(IEnumerable<IIndexedModifier> indexedModifiers)
            : base()
        {
            Modifiers = indexedModifiers == null ? null : indexedModifiers.ToList().ConvertAll(x => x?.Clone());
        }

        public IndexedComplexModifier(IndexedComplexModifier complexModifier)
            : base(complexModifier)
        {
            if(complexModifier != null)
            {
                Modifiers = complexModifier.Modifiers.ConvertAll(x => x.Clone());
            }
        }

        public IndexedComplexModifier(JObject jObject)
            : base(jObject)
        {

        }

        public virtual bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("Modifiers"))
            {
                JArray jArray = jObject.Value<JArray>("Modifiers");
                if(jArray != null)
                {
                    Modifiers = new List<IIndexedModifier>();
                    foreach(JObject jObject_Modifier in jArray)
                    {
                        Modifiers.Add(Query.IJSAMObject<IIndexedModifier>(jObject_Modifier));
                    }
                }
            }

            return result;
        }

        public List<IIndexedModifier> Modifiers { get; set; }

        public virtual JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return result;
            }

            if (Modifiers != null)
            {
                JArray jArray = new JArray();
                foreach (IModifier modifier in Modifiers)
                {
                    jArray.Add(modifier.ToJObject());
                }

                result.Add("Modifiers", jArray);
            }

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            if(Modifiers == null)
            {
                return false;
            }

            foreach(IIndexedModifier indexedModifier in Modifiers)
            {
                if(indexedModifier.ContainsIndex(index))
                {
                    return true;
                }
            }

            return false;
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (Modifiers == null)
            {
                return value;
            }

            foreach (IIndexedModifier indexedModifier in Modifiers)
            {
                if (indexedModifier.ContainsIndex(index))
                {
                    return indexedModifier.GetCalculatedValue(index, value);
                }
            }

            return value;
        }
    }
}