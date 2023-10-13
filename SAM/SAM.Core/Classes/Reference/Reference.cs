using Newtonsoft.Json.Linq;
using System;

namespace SAM.Core
{
    public struct Reference: IReference
    {
        private string value;

        private Reference(string value)
        {
            this.value = value;
        }

        public Reference(Reference reference)
        {
            value = reference.value;
        }

        public Reference(JObject jObject)
        {
            value = null;

            FromJObject(jObject);
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public override string ToString()
        {
            return value == null ? null : value.ToString();
        }

        public override bool Equals(object @object)
        {
            if (@object is Reference reference)
            {
                return value == reference.value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value == null ? 0 : value.GetHashCode();
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            value = null;
            if(jObject.ContainsKey("Value"))
            {
                value = jObject.Value<string>("Value");
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Query.FullTypeName(this));

            if(value != null)
            {
                result.Add("Value", value);
            }

            return result;
        }

        public static implicit operator Reference(string value)
        {
            return new Reference(value);
        }

        public static implicit operator Reference(Guid value)
        {
            return new Reference(value.ToString("N"));
        }

        public static implicit operator Reference(SAMObject value)
        {
            string reference = null;
            if(value != null)
            {
                reference = value.Guid.ToString("N");
            }

            return new Reference(reference);
        }

        public static implicit operator Reference(int value)
        {
            return new Reference(value.ToString());
        }

        public static bool operator ==(Reference reference_1, Reference reference_2)
        {
            return reference_1.value == reference_2.value;
        }

        public static bool operator !=(Reference reference_1, Reference reference_2)
        {
            return reference_1.value != reference_2.value;
        }
    }
}
