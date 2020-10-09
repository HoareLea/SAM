using System;

namespace SAM.Core.Attributes
{
    public class ParameterDisplayName : Attribute
    {
        private string value;

        public ParameterDisplayName(string value)
        {
            this.value = value;
        }
    }
}
