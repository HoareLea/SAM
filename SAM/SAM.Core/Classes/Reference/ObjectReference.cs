using System.Collections.Generic;

namespace SAM.Core
{
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
    }
}
