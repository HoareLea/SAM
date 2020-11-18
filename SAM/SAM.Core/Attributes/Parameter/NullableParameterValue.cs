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

        public override bool TryConvert(object object_In, out object object_Out)
        {
            object_Out = default;

            if (object_In == null || !nullable)
                return false;

            return base.TryConvert(object_In, out object_Out);
        }
    }
}
