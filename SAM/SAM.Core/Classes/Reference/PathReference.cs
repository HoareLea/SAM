using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Core
{
    /// <summary>
    /// Parh reference in format {ObjectReference_1}->{ObjectRefernce_2}->{ObjectReference_3}
    /// 
    /// example:
    /// Space::"InternalCondition"->InternalCondition::"Name"
    /// Space->"InternalCondition"->"Name"
    /// Space->Zone::"Name"
    /// </summary>
    public class PathReference: IComplexReference, IEnumerable<ObjectReference>
    {
        private List<ObjectReference> objectReferences;

        public PathReference(IEnumerable<ObjectReference> objectReferences)
        {
            if(objectReferences != null)
            {
                this.objectReferences = objectReferences == null ? null : new List<ObjectReference>(objectReferences);
            }
        }

        public PathReference(IEnumerable<ObjectReference> objectReferences, ObjectReference objectReference)
        {
            if (objectReferences != null)
            {
                this.objectReferences = objectReferences == null ? null : new List<ObjectReference>(objectReferences);
            }

            if(objectReference != null)
            {
                if(this.objectReferences == null)
                {
                    this.objectReferences = new List<ObjectReference>();
                }

                this.objectReferences.Add(objectReference);
            }
        }

        public override string ToString()
        {
            List<string> values = objectReferences?.ConvertAll(x => x?.ToString()).ConvertAll(x => x == null ? string.Empty : x);
            if(values == null || values.Count == 0)
            {
                return string.Empty;
            }

            return string.Join("->", values);
        }

        public bool IsValid()
        {
            return objectReferences != null && objectReferences.Count != 0 && objectReferences.TrueForAll(x => x.IsValid());
        }

        public IEnumerator<ObjectReference> GetEnumerator()
        {
            return objectReferences == null ? null : objectReferences.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
            {
                return false;
            }

            if (jObject.ContainsKey("ObjectReferences"))
            {
                JArray jArray = jObject.Value<JArray>("ObjectReferences");
                if(jArray != null)
                {
                    objectReferences = new List<ObjectReference>();
                    foreach(JObject jObject_ObjectReference in jArray)
                    {
                        ObjectReference objectReference = Query.IJSAMObject<ObjectReference>(jObject_ObjectReference);
                        if(objectReference != null)
                        {
                            objectReferences.Add(objectReference);
                        }
                    }
                }
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Query.FullTypeName(this));

            if (objectReferences != null)
            {
                JArray jArray = new JArray();

                foreach(ObjectReference objectReference in objectReferences)
                {
                    jArray.Add(objectReference.ToJObject());
                }

                result.Add("ObjectReferences", jArray);
            }

            return result;
        }
    }
}
