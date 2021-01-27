namespace SAM.Core.Attributes
{
    public class IntegerParameterValue : ParameterValue
    {
        private int min = int.MinValue;
        private int max = int.MaxValue;

        public IntegerParameterValue()
            : base(ParameterType.Integer)
        {

        }

        public IntegerParameterValue(int min, int max)
            : base(ParameterType.Integer)
        {
            this.min = min;
            this.max = max;
        }

        public IntegerParameterValue(int min)
            : base(ParameterType.Integer)
        {
            this.min = min;
        }

        public override bool TryConvert(object object_In, out object object_Out)
        {
            if (!base.TryConvert(object_In, out object_Out))
                return false;

            if (int.MinValue == min && int.MaxValue == max)
                return true;

            int @int = (int)object_Out;

            if (min != int.MinValue && @int < min)
                return false;

            if (max != int.MaxValue && @int > max)
                return false;

            return true;
        }
    }
}
