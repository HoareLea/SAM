using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class ConstructionLayer : SAMObject
    {
        private double thickness;

        public ConstructionLayer(string name, double thickness)
            : base(name)
        {
            this.thickness = thickness;
        }

        public ConstructionLayer(Guid guid, string name, double thickness)
        : base(guid, name)
        {
            this.thickness = thickness;
        }

        public ConstructionLayer(ConstructionLayer constructionLayer)
            : base(constructionLayer)
        {
            thickness = constructionLayer.thickness;
        }

        public ConstructionLayer(JObject jObject)
            : base(jObject)
        {
        }

        public double Thickness
        {
            get
            {
                return thickness;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if(jObject.ContainsKey("Thickness"))
            thickness = jObject.Value<double>("Thickness");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (!double.IsNaN(thickness))
                jObject.Add("Thickness", thickness);

            return jObject;
        }
    }
}