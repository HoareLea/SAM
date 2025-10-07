using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    public class FeatureShade : SAMObject, IAnalyticalObject
    {
        private string description = null;
        private double surfaceHeight = double.NaN;
        private double surfaceWidth = double.NaN;
        private double leftFinDepth = double.NaN;
        private double leftFinOffset = double.NaN;
        private double leftFinTransmittance = double.NaN;
        private double rightFinDepth = double.NaN;
        private double rightFinOffset = double.NaN;
        private double rightFinTransmittance = double.NaN;
        private double overhangDepth = double.NaN;
        private double overhangOffset = double.NaN;
        private double overhangTransmittance = double.NaN;

        public FeatureShade(string name)
            : base(name)
        {

        }

        public FeatureShade(string name, string description, double surfaceHeight, double surfaceWidth, double leftFinDepth, double leftFinOffset, double leftFinTransmittance, double rightFinDepth, double rightFinOffset, double rightFinTransmittance, double overhangDepth, double overhangOffset, double overhangTransmittance)
            : base(name)
        {
            this.description = description;
            this.surfaceHeight = surfaceHeight;
            this.surfaceWidth = surfaceWidth;
            this.leftFinDepth = leftFinDepth;
            this.leftFinOffset = leftFinOffset;
            this.leftFinTransmittance = leftFinTransmittance;
            this.rightFinDepth = rightFinDepth;
            this.rightFinOffset = rightFinOffset;
            this.rightFinTransmittance = rightFinTransmittance;
            this.overhangDepth = overhangDepth;
            this.overhangOffset = overhangOffset;
            this.overhangTransmittance = overhangTransmittance;
        }

        public FeatureShade(FeatureShade featureShade)
            : base(featureShade)
        {
            if(featureShade is not null)
            {
                description = featureShade.description;
                surfaceHeight = featureShade.surfaceHeight;
                surfaceWidth = featureShade.surfaceWidth;
                leftFinDepth = featureShade.leftFinDepth;
                leftFinOffset = featureShade.leftFinOffset;
                leftFinTransmittance = featureShade.leftFinTransmittance;
                rightFinDepth = featureShade.rightFinDepth;
                rightFinOffset = featureShade.rightFinOffset;
                rightFinTransmittance = featureShade.rightFinTransmittance;
                overhangDepth = featureShade.overhangDepth;
                overhangOffset = featureShade.overhangOffset;
                overhangTransmittance = featureShade.overhangTransmittance;
            }
        }

        public FeatureShade(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Description"))
            {
                description = jObject.Value<string>("Description");
            }

            if (jObject.ContainsKey("SurfaceHeight"))
            {
                surfaceHeight = jObject.Value<double>("SurfaceHeight");
            }

            if (jObject.ContainsKey("SurfaceWidth"))
            {
                surfaceWidth = jObject.Value<double>("SurfaceWidth");
            }

            if (jObject.ContainsKey("LeftFinDepth"))
            {
                leftFinDepth = jObject.Value<double>("LeftFinDepth");
            }

            if (jObject.ContainsKey("LeftFinDepth"))
            {
                leftFinDepth = jObject.Value<double>("LeftFinDepth");
            }

            if (jObject.ContainsKey("LeftFinOffset"))
            {
                leftFinOffset = jObject.Value<double>("LeftFinOffset");
            }

            if (jObject.ContainsKey("RightFinTransmittance"))
            {
                rightFinTransmittance = jObject.Value<double>("RightFinTransmittance");
            }

            if (jObject.ContainsKey("OverhangDepth"))
            {
                overhangDepth = jObject.Value<double>("OverhangDepth");
            }

            if (jObject.ContainsKey("OverhangOffset"))
            {
                overhangOffset = jObject.Value<double>("OverhangOffset");
            }

            if (jObject.ContainsKey("OverhangTransmittance"))
            {
                overhangTransmittance = jObject.Value<double>("OverhangTransmittance");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return null;
            }

            if(description is not null)
            {
                jObject.Add("Description", description);
            }

            if (!double.IsNaN(surfaceHeight))
            {
                jObject.Add("SurfaceHeight", surfaceHeight);
            }

            if (!double.IsNaN(surfaceWidth))
            {
                jObject.Add("SurfaceWidth", surfaceWidth);
            }

            if (!double.IsNaN(leftFinDepth))
            {
                jObject.Add("LeftFinDepth", leftFinDepth);
            }

            if (!double.IsNaN(leftFinOffset))
            {
                jObject.Add("LeftFinOffset", leftFinOffset);
            }

            if (!double.IsNaN(leftFinTransmittance))
            {
                jObject.Add("LeftFinTransmittance", leftFinTransmittance);
            }

            if (!double.IsNaN(rightFinDepth))
            {
                jObject.Add("RightFinDepth", rightFinDepth);
            }

            if (!double.IsNaN(rightFinOffset))
            {
                jObject.Add("RightFinOffset", rightFinOffset);
            }

            if (!double.IsNaN(rightFinTransmittance))
            {
                jObject.Add("RightFinTransmittance", rightFinTransmittance);
            }

            if (!double.IsNaN(overhangDepth))
            {
                jObject.Add("OverhangDepth", overhangDepth);
            }

            if (!double.IsNaN(overhangOffset))
            {
                jObject.Add("OverhangOffset", overhangOffset);
            }

            if (!double.IsNaN(overhangTransmittance))
            {
                jObject.Add("OverhangTransmittance", overhangTransmittance);
            }

            return jObject;
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        public double SurfaceHeight
        {
            get
            {
                return surfaceHeight;
            }
        }

        public double SurfaceWidth
        {
            get
            {
                return surfaceWidth;
            }
        }

        public double LeftFinDepth
        {
            get
            {
                return leftFinDepth;
            }
        }

        public double LeftFinOffset
        {
            get
            {
                return leftFinOffset;
            }
        }

        public double LeftFinTransmittance
        {
            get
            {
                return leftFinTransmittance;
            }
        }

        public double RightFinDepth
        {
            get
            {
                return rightFinDepth;
            }
        }

        public double RightFinOffset
        {
            get
            {
                return rightFinOffset;
            }
        }

        public double RightFinTransmittance
        {
            get
            {
                return rightFinTransmittance;
            }
        }

        public double OverhangDepth
        {
            get
            {
                return overhangDepth;
            }
        }

        public double OverhangOffset
        {
            get
            {
                return overhangOffset;
            }
        }

        public double OverhangTransmittance
        {
            get
            {
                return overhangTransmittance;
            }
        }
    }
}