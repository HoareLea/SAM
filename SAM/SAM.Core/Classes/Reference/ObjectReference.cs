using Newtonsoft.Json.Linq;
using System;
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

        public ObjectReference(Type type)
        {
            typeName= Query.FullTypeName(type);
        }

        public ObjectReference(Type type, Reference? reference = null)
        {
            typeName = Query.FullTypeName(type);
            this.reference = reference;
        }

        public ObjectReference(SAMObject sAMObject)
        {
            typeName = Query.FullTypeName(sAMObject);
            reference = sAMObject?.Guid;
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

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(obj, null))
            {
                return false;
            }

            ObjectReference objectReference = obj as ObjectReference;
            if(objectReference == null)
            {
                return false;
            }

            if (typeName != objectReference.typeName)
            {
                return false;
            }

            if (reference != objectReference.reference)
            {
                return false;
            }

            return true;
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

        public Type Type
        {
            get
            {
                if(string.IsNullOrWhiteSpace(typeName))
                {
                    return null;
                }

                Type result = Query.Type(typeName);
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

            set
            {
                typeName = Query.FullTypeName(value);
            }
        }

        public static bool operator ==(ObjectReference objectReference_1, ObjectReference objectReference_2)
        {
            if(ReferenceEquals( objectReference_1, null) && ReferenceEquals( objectReference_2, null))
            {
                return true;
            }

            if (ReferenceEquals(objectReference_1, null) || ReferenceEquals(objectReference_2, null))
            {
                return false;
            }

            return objectReference_1.Equals(objectReference_2);
        }

        public static bool operator !=(ObjectReference objectReference_1, ObjectReference objectReference_2)
        {
            return !(objectReference_1 == objectReference_2);
        }
    }
}
