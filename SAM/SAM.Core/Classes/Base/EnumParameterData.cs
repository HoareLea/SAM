using SAM.Core.Attributes;
using System;

namespace SAM.Core
{
    public class EnumParameterData : IParameterData
    {
        private Enum @enum;

        public EnumParameterData(Enum @enum)
        {
            this.@enum = @enum;
        }

        public Enum Enum
        {
            get
            {
                return @enum;
            }
        }

        public ParameterProperties ParameterProperties
        {
            get
            {
                return Query.CustomAttribute<ParameterProperties>(@enum);
            }
        }

        public ParameterValue ParameterValue
        {
            get
            {
                return Query.CustomAttribute<ParameterValue>(@enum);
            }
        }
    }
}
