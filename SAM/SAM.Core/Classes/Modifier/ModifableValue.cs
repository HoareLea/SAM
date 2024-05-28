using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    public class ModifiableValue : IModifiableValue
    {
        public ModifiableValue(IModifier modifier, double value)
        {
            Value = value;
            Modifier = modifier;
        }

        public ModifiableValue(double value)
        {
            Value = value;
        }

        public ModifiableValue(ModifiableValue modifiableValue)
        {
            if (modifiableValue != null)
            {
                Value = modifiableValue.Value;
                Modifier = modifiableValue.Modifier;
            }
        }

        public ModifiableValue(JObject jObject)
        {
            FromJObject(jObject);
        }

        public IModifier Modifier { get; set; }
        
        public double Value { get; set; }
        
        public static implicit operator ModifiableValue(double value)
        {
            return new ModifiableValue(value);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("Value"))
            {
                Value = jObject.Value<double>("Value");
            }

            if (jObject.ContainsKey("Modifier"))
            {
                Modifier = Query.IJSAMObject<ISimpleModifier>(jObject.Value<JObject>("Modifier"));
            }

            return true;
        }

        public double GetCalculatedValue(int index)
        {
            return Modifier == null ? Value : Modifier.GetCalculatedValue(index, Value);
        }
        
        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Query.FullTypeName(this));
            
            if(!double.IsNaN(Value))
            {
                jObject.Add("Value", Value);
            }

            if (Modifier != null)
            {
                jObject.Add("Modifier", Modifier.ToJObject());
            }

            return jObject;
        }
    }
}