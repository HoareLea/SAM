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
    }
}
