namespace SAM.Core.Attributes
{
    public class DoubleParameterType : ParameterType
    {
        private double min = double.NaN;
        private double max = double.NaN;

        public DoubleParameterType()
            : base(Core.ParameterType.Double)
        {

        }

        public DoubleParameterType(double min, double max)
            : base(Core.ParameterType.Double)
        {
            this.min = min;
            this.max = max;
        }

        public DoubleParameterType(double min)
            : base(Core.ParameterType.Double)
        {
            this.min = min;
        }

        public override bool IsValid(object value)
        {
            bool result = base.IsValid(value);
            if (!result)
                return result;

            if (double.IsNaN(min) && double.IsNaN(max))
                return true;
            
            double @double = System.Convert.ToDouble(value);

            if (!double.IsNaN(min) && @double < min)
                return false;

            if (!double.IsNaN(max) && @double > max)
                return false;

            return true;
        }
    }
}
