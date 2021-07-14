namespace SAM.Core.Attributes
{
    public class DoubleParameterValue : ParameterValue
    {
        private double min = double.NaN;
        private double max = double.NaN;

        public DoubleParameterValue()
            : base(ParameterType.Double)
        {

        }

        public DoubleParameterValue(double min, double max)
            : base(ParameterType.Double)
        {
            this.min = min;
            this.max = max;
        }

        public DoubleParameterValue(double min)
            : base(ParameterType.Double)
        {
            this.min = min;
        }

        public override bool TryConvert(object object_In, out object object_Out)
        {
            if (!base.TryConvert(object_In, out object_Out))
                return false;

            if (double.IsNaN(min) && double.IsNaN(max))
                return true;

            double @double = (double)object_Out;

            if (!double.IsNaN(min) && @double < min)
                return false;

            if (!double.IsNaN(max) && @double > max)
                return false;

            return true;
        }
    }
}
