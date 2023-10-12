using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core
{
    /// <summary>
    /// Reference to the object format {TypeName}::[{Reference}] where TypeName - full typeName, Reference - reference to the object 
    /// 
    /// examples: 
    /// SAM.Analytical.Space,SAM.Analytical::[0]
    /// Space::[3cc94341-70ba-47b7-831e-581345b85bd3]
    /// Space
    /// Space::["Cell 1.2"]
    /// </summary>
    public class ObjectReference : IComplexReference
    {
        private string typeName;
        private Reference? reference;

        public ObjectReference(string typeName, Reference? reference = null)
        {
            this.typeName = typeName;
            this.reference = reference;
        }

        public ObjectReference(ObjectReference objectReference)
        {
            typeName = objectReference?.typeName;
            reference = objectReference?.reference;
        }

        public ObjectReference (JObject jObject)
        {
            FromJObject(jObject);
        }

        public Reference? Reference
        {
            get
            {
                return reference;
            }
        }

        public virtual bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(this.typeName) || (reference != null && reference.HasValue && reference.Value.IsValid());
        }

        public override string ToString()
        {
            List<string> values = new List<string>();
            if(!string.IsNullOrWhiteSpace(typeName))
            {
                values.Add(typeName);
            }

            if(reference != null && reference.HasValue)
            {
                values.Add(string.Format("[{0}]", reference.Value.ToString()));
            }

            return string.Join("::", values);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("TypeName"))
            {
                typeName = jObject.Value<string>("TypeName");
            }

            if(jObject.ContainsKey("Reference"))
            {
                reference = new Reference(jObject.Value<JObject>("Reference"));
            }

            return true;
        }

        public virtual JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Query.FullTypeName(this));

            if (typeName != null)
            {
                result.Add("TypeName", typeName);
            }

            if(reference != null && reference.HasValue)
            {
                result.Add("Reference", reference.Value.ToJObject());
            }

            return result;
        }

        public string TypeName
        {
            get
            {
                return typeName;
            }
        }

        public System.Type Type
        {
            get
            {
                if(string.IsNullOrWhiteSpace(typeName))
                {
                    return null;
                }

                System.Type result = Query.Type(typeName);
                if(result == null)
                {
                    result = Query.Type(string.Format(".{0}", typeName), TextComparisonType.EndsWith, false, (System.Reflection.Assembly x) => x.GetName().Name.StartsWith("SAM."));
                    if(result == null)
                    {
                        result = Query.Type(typeName, TextComparisonType.EndsWith);
                    }
                }

                return result;
            }
        }
    }
}
