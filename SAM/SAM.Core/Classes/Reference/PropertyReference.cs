using System.Collections.Generic;

namespace SAM.Core
{
    public class PropertyReference : ObjectReference
    {
        private string propertyName;

        public PropertyReference(string typeName, string propertyName)
            : base(typeName)
        {
            this.propertyName = propertyName;
        }

        public PropertyReference(ObjectReference objectReference, string propertyName)
            :base(objectReference)
        {
            this.propertyName = propertyName;
        }

        public PropertyReference(string typeName, Reference? reference, string propertyName)
            :base(typeName, reference)
        {
            this.propertyName = propertyName;
        }

        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrEmpty(propertyName);
        }

        public override string ToString()
        {
            List<string> values = new List<string>();

            string value = base.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }

            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                values.Add(string.Format(@"""{0}""", propertyName));
            }

            return string.Join("::", values);
        }
    }
}
