using System;
using System.Reflection;

namespace SAM.Core.Attributes
{
    public class ParameterName : Attribute
    {
        private string value;

        public ParameterName(string value)
        {
            this.value = value;
        }

        public static string Get(Type type, string text)
        {
            ParameterName parameterName = Query.CustomAttribute<ParameterName>(type, text);
            if (parameterName == null)
                return null;

            return parameterName.value;
        }

        public static string Get(Enum @enum)
        {
            ParameterName parameterName = Query.CustomAttribute<ParameterName>(@enum);
            if (parameterName == null)
                return null;

            return parameterName.value;
        }
    }
}
