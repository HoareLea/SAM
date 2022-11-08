﻿using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Analytical
{
    /// <summary>
    /// Opening Properties
    /// </summary>
    public class PartLOpeningProperties : ParameterizedSAMObject, IOpeningProperties
    {
        private double width;
        private double height;
        private double openingAngle;

        public PartLOpeningProperties()
        {

        }

        public PartLOpeningProperties(double width, double height, double openingAngle)
        {
            this.width = width;
            this.height = height;
            this.openingAngle = openingAngle;
        }

        public PartLOpeningProperties(JObject jObject)
            :base(jObject)
        {

        }

        public PartLOpeningProperties(PartLOpeningProperties partLOpeningProperties)
            : base(partLOpeningProperties)
        {
            if(partLOpeningProperties != null)
            {
                width = partLOpeningProperties.width;
                height = partLOpeningProperties.height;
                openingAngle = partLOpeningProperties.openingAngle;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("Width"))
            {
                width = jObject.Value<double>("Width");
            }

            if (jObject.ContainsKey("Height"))
            {
                height = jObject.Value<double>("Height");
            }

            if (jObject.ContainsKey("OpeningAngle"))
            {
                openingAngle = jObject.Value<double>("OpeningAngle");
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if(jObject == null)
            {
                return null;
            }

            if(!double.IsNaN(width))
            {
                jObject.Add("Width", width);
            }

            if (!double.IsNaN(height))
            {
                jObject.Add("Height", height);
            }

            if (!double.IsNaN(openingAngle))
            {
                jObject.Add("OpeningAngle", openingAngle);
            }

            return jObject;
        }

        public double GetDischargeCoefficient()
        {
            if(double.IsNaN(width) || double.IsNaN(height) || double.IsNaN(openingAngle) || height == 0 || width == 0)
            {
                return double.NaN;
            }

            double lengthRatio = width / height;
            if(lengthRatio == 0)
            {
                return double.NaN;
            }

            double gradient = double.NaN;
            double maxDischargeCoefficient = double.NaN;
            if (lengthRatio < 0.5)
            {
                gradient = 0.0604762544204005;
                maxDischargeCoefficient = 0.612341772151899;
            }
            else if(lengthRatio < 1.0)
            {
                gradient = 0.588607594936709;
                maxDischargeCoefficient = 0.588607594936709;
            }
            else if(lengthRatio < 2.0)
            {
                gradient = 0.0404635490792875;
                maxDischargeCoefficient = 0.5625;
            }
            else
            {
                gradient = 0.0381420632257139;
                maxDischargeCoefficient = 0.548259493670886;
            }

            if(double.IsNaN(gradient) || double.IsNaN(maxDischargeCoefficient))
            {
                return double.NaN;
            }

            return maxDischargeCoefficient * (1 - System.Math.Exp(-gradient * openingAngle));
        }
    }
}