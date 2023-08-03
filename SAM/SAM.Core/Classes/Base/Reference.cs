using System;
using System.Collections.Generic;
using System.Drawing;

namespace SAM.Core
{
    public struct Reference
    {
        private string value;

        private Reference(string value)
        {
            this.value = value;
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

        public static implicit operator Reference(string value)
        {
            return new Reference(value);
        }
    }
}
