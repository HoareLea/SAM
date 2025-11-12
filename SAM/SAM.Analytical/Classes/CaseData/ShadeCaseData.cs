using Newtonsoft.Json.Linq;

namespace SAM.Analytical
{
    public class ShadeCaseData : BuiltInCaseData
    {
        private double overhangDepth;
        private double leftFinDepth;
        private double rightFinDepth;

        public ShadeCaseData(double overhangDepth, double leftFinDepth, double rightFinDepth)
            : base(nameof(ShadeCaseData))
        {
            this.overhangDepth = overhangDepth;
            this.leftFinDepth = leftFinDepth;
            this.rightFinDepth = rightFinDepth;
        }

        public ShadeCaseData(JObject jObject)
            : base(jObject)
        {

        }

        public ShadeCaseData(ShadeCaseData shadeCaseData)
            : base(shadeCaseData)
        {
            if(shadeCaseData != null)
            {
                overhangDepth = shadeCaseData.overhangDepth;
                leftFinDepth = shadeCaseData.leftFinDepth;
                rightFinDepth = shadeCaseData.rightFinDepth;
            }
        }

        public double OverhangDepth
        {
            get 
            { 
                return overhangDepth; 
            }
        }

        public double LeftFinDepth
        {
            get
            {
                return leftFinDepth;
            }
        }

        public double RightFinDepth
        {
            get
            {
                return rightFinDepth;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return false;
            }

            if(jObject.ContainsKey("OverhangDepth"))
            {
                overhangDepth = jObject.Value<double>("OverhangDepth");
            }

            if (jObject.ContainsKey("LeftFinDepth"))
            {
                leftFinDepth = jObject.Value<double>("LeftFinDepth");
            }

            if (jObject.ContainsKey("RightFinDepth"))
            {
                rightFinDepth = jObject.Value<double>("RightFinDepth");
            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result is null)
            {
                return result;
            }

            if(!double.IsNaN(overhangDepth))
            {
                result.Add("OverhangDepth", overhangDepth);
            }

            if (!double.IsNaN(leftFinDepth))
            {
                result.Add("LeftFinDepth", leftFinDepth);
            }

            if (!double.IsNaN(rightFinDepth))
            {
                result.Add("RightFinDepth", rightFinDepth);
            }

            return result;
        }
    }
}