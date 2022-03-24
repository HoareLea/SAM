using SAM.Core.Attributes;
using System;

namespace SAM.Core
{
    public static partial class Create
    {
        public static ParameterData ParameterData(Enum @enum)
        {
            ParameterProperties parameterProperties = Query.CustomAttribute<ParameterProperties>(@enum);
            if (parameterProperties == null)
            {
                return null;
            }

            ParameterValue parameterValue = Query.CustomAttribute<ParameterValue>(@enum);

            return new ParameterData(parameterProperties, parameterValue);
        }
    }
}