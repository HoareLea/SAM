namespace SAM.Core.Attributes
{
    public abstract class NullableParameterValue : ParameterValue
    {
        private bool nullable = true;

        public NullableParameterValue(ParameterType parameterType)
            : base(parameterType)
        {

        }

        public NullableParameterValue(ParameterType parameterType, bool nullable)
            : base(parameterType)
        {
            this.nullable = nullable;
        }

        public bool Nullable
        {
            get
            {
                return nullable;
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return nullable;

            return base.IsValid(value);
        }
    }
}
