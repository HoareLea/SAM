using System;
using System.Collections;
using System.Linq;

namespace SAM.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class Operator : Attribute
    {
        private string value;

        public Operator(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get
            {
                return value;
            }
        }
    }
}
