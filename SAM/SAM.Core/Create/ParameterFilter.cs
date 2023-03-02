using System;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ParameterFilter ParameterFilter(string name, string value, TextComparisonType textComparisonType)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return new ParameterFilter(name, value, textComparisonType);
        }

        public static ParameterFilter ParameterFilter(Enum @enum, string value, TextComparisonType textComparisonType)
        {
            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if(parameterProperties == null)
            {
                return null;
            }

            return ParameterFilter(parameterProperties.Name, value, textComparisonType);
        }

        public static ParameterFilter ParameterFilter(string name, double value, NumberComparisonType numberComparisonType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            return new ParameterFilter(name, value, numberComparisonType);
        }

        public static ParameterFilter ParameterFilter(Enum @enum, double value, NumberComparisonType numberComparisonType)
        {
            Attributes.ParameterProperties parameterProperties = Attributes.ParameterProperties.Get(@enum);
            if (parameterProperties == null)
            {
                return null;
            }

            return ParameterFilter(parameterProperties.Name, value, numberComparisonType);
        }
    }
}