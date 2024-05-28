using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core
{
    public class ComplexModifier : Modifier, IComplexModifier
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
            if (jObject == null)
            {
                return false;
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

            return true;
        }

        public List<IModifier> Modifiers { get; set; }

        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));

            if (Modifiers != null)
            {
                JArray jArray = new JArray();
                foreach (IModifier modifier in Modifiers)
                {
                    jArray.Add(modifier.ToJObject());
                }

                jObject.Add("Modifiers", jArray);
            }

            return jObject;
        }

        public override bool ContainsIndex(int index)
        {
            if(Modifiers == null)
            {
                return false;
            }

            foreach(IModifier modifier in Modifiers)
            {
                if(modifier.ContainsIndex(index))
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

            foreach (IModifier modifier in Modifiers)
            {
                if (modifier.ContainsIndex(index))
                {
                    return modifier.GetCalculatedValue(index, value);
                }
            }

            return value;
        }
    }
}