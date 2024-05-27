using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class IndexedModifier : SimpleModifier
    {
        public IndexedDoubles IndexedDoubles { get; set; }

        public IndexedModifier(ArithmeticOperator arithmeticOperator, IndexedDoubles indexedDoubles)
        {
            ArithmeticOperator = arithmeticOperator;
            IndexedDoubles = indexedDoubles == null ? null : new IndexedDoubles(indexedDoubles);
        }

        public IndexedModifier(IndexedModifier indexedModifier)
            : base(indexedModifier)
        {
            if(indexedModifier != null)
            {
                IndexedDoubles = indexedModifier?.IndexedDoubles == null ? null : new IndexedDoubles(indexedModifier.IndexedDoubles);
            }
        }

        public IndexedModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("IndexedDoubles"))
            {
                IndexedDoubles = Query.IJSAMObject<IndexedDoubles>(jObject.Value<JObject>("IndexedDoubles"));
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(IndexedDoubles != null)
            {
                result.Add("IndexedDoubles", IndexedDoubles.ToJObject());
            }

            return result;
        }

        public override bool ContainsIndex(int index)
        {
            return IndexedDoubles != null && IndexedDoubles.ContainsIndex(index);
        }

        public override double GetCalculatedValue(int index, double value)
        {
            if (IndexedDoubles == null)
            {
                return value;
            }

            if (IndexedDoubles.ContainsIndex(index))
            {
                return Query.Calculate(ArithmeticOperator, value, IndexedDoubles[index]);
            }

            return value;
        }
    }
}