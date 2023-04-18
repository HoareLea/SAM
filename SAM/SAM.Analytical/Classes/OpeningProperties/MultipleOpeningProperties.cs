using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    /// <summary>
    /// Multiple Opening Properties
    /// </summary>
    public class MultipleOpeningProperties : ParameterizedSAMObject, IOpeningProperties
    {
        private List<ISingleOpeningProperties> singleOpeningProperties;

        public MultipleOpeningProperties()
        {

        }

        public MultipleOpeningProperties(JObject jObject)
            :base(jObject)
        {

        }

        public MultipleOpeningProperties(IEnumerable<ISingleOpeningProperties> singleOpeningProperties)
            : base()
        {
            this.singleOpeningProperties = singleOpeningProperties == null ? null : singleOpeningProperties.ToList().ConvertAll(x => Core.Query.Clone(x));
        }

        public MultipleOpeningProperties(MultipleOpeningProperties multipleOpeningProperties)
            : base(multipleOpeningProperties)
        {
            singleOpeningProperties = multipleOpeningProperties?.singleOpeningProperties?.ConvertAll(x => Core.Query.Clone(x));
        }

        public MultipleOpeningProperties(MultipleOpeningProperties multipleOpeningProperties, IEnumerable<ISingleOpeningProperties> singleOpeningProperties)
            : base(multipleOpeningProperties)
        {
            this.singleOpeningProperties = singleOpeningProperties == null ? null : singleOpeningProperties.ToList().ConvertAll(x => Core.Query.Clone(x));
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if(jObject.ContainsKey("SingleOpeningProperties"))
            {
                JArray jArray = jObject.Value<JArray>("SingleOpeningProperties");
                if(jArray != null)
                {
                    singleOpeningProperties = new List<ISingleOpeningProperties>();
                    foreach(JObject jObject_OpeningProperties in jArray)
                    {
                        ISingleOpeningProperties openingProperties = Core.Query.IJSAMObject<ISingleOpeningProperties>(jObject_OpeningProperties);
                        if(openingProperties == null)
                        {
                            continue;
                        }

                        this.singleOpeningProperties.Add(openingProperties);
                    }

                }
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

            if(singleOpeningProperties != null)
            {
                JArray jArray = new JArray();
                foreach(ISingleOpeningProperties singleOpeningProperties in singleOpeningProperties)
                {
                    if(singleOpeningProperties == null)
                    {
                        continue;
                    }

                    jArray.Add(singleOpeningProperties.ToJObject());
                }

                jObject.Add("SingleOpeningProperties", jArray);
            }

            return jObject;
        }

        public List<ISingleOpeningProperties> SingleOpeningProperties
        {
            get
            {
                return singleOpeningProperties?.ConvertAll(x => Core.Query.Clone(x));
            }
        }

        public double GetDischargeCoefficient()
        {
            ISingleOpeningProperties singleOpeningProperties = this.SingleOpeningProperties();
            if(singleOpeningProperties == null)
            {
                return double.NaN;
            }

            return singleOpeningProperties.GetDischargeCoefficient();
        }

        public double GetFactor()
        {
            ISingleOpeningProperties singleOpeningProperties = this.SingleOpeningProperties();
            if (singleOpeningProperties == null)
            {
                return double.NaN;
            }

            return singleOpeningProperties.GetFactor();
        }
    }
}