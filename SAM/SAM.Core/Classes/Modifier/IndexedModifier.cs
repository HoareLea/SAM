using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public abstract class IndexedModifier : Modifier, IIndexedModifier
    {
        public IndexedModifier()
        {

        }

        public IndexedModifier(IndexedModifier indexedModifier)
            :base(indexedModifier)
        {

        }

        public IndexedModifier(JObject jObject)
            : base(jObject)
        {

        }

        public abstract bool ContainsIndex(int index);

        public abstract double GetCalculatedValue(int index, double value);
    }
}