using SAM.Core.Attributes;

namespace SAM.Core
{
    public class ParameterData
    {
        private ParameterProperties parameterProperties;
        private ParameterValue parameterValue;

        public ParameterData(ParameterProperties parameterProperties, ParameterValue parameterValue)
        {
            this.parameterProperties = parameterProperties;
            this.parameterValue = parameterValue;
        }

        public ParameterProperties ParameterProperties
        {
            get
            {
                return parameterProperties;
            }
        }

        public ParameterValue ParameterValue
        {
            get
            {
                return parameterValue;
            }
        }
    }
}
