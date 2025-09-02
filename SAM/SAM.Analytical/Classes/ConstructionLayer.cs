﻿using Newtonsoft.Json.Linq;
using SAM.Architectural;

namespace SAM.Analytical
{
    public class ConstructionLayer : MaterialLayer, IAnalyticalObject
    {
        public ConstructionLayer(string name, double thickness)
            : base(name, thickness)
        {
        }

        public ConstructionLayer(ConstructionLayer constructionLayer)
            : base(constructionLayer)
        {

        }

        public ConstructionLayer(JObject jObject)
            : base(jObject)
        {

        }

        public bool IsSoil
        {
            get
            {
                if(string.IsNullOrWhiteSpace(Name))
                {
                    return false;
                }

                return Name.ToLower().Contains("soil");
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
            {
                return jObject;
            }

            return jObject;
        }
    }
}