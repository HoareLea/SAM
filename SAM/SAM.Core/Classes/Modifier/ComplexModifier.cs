using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class ComplexModifier : Modifier, IComplexModifier<IModifier>
    {
        public ComplexModifier()
            :base()
        {

        }

        public ComplexModifier(IEnumerable<IModifier> modifiers)
            : base()
        {
            Modifiers = modifiers == null ? null : modifiers.ToList().ConvertAll(x => x?.Clone());
        }

        public ComplexModifier(ComplexModifier complexModifier)
            : base(complexModifier)
        {
            if(complexModifier != null)
            {
                Modifiers = complexModifier.Modifiers.ConvertAll(x => x.Clone());
            }
        }

        public ComplexModifier(JObject jObject)
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
                    Modifiers = new List<IModifier>();
                    foreach(JObject jObject_Modifier in jArray)
                    {
                        Modifiers.Add(Query.IJSAMObject<IModifier>(jObject_Modifier));
                    }
                }
            }

            return result;
        }

        public List<IModifier> Modifiers { get; set; }

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
    }
}