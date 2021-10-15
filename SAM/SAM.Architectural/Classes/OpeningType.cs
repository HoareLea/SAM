using Newtonsoft.Json.Linq;

using System.Collections.Generic;

namespace SAM.Architectural
{
    public abstract class OpeningType : BuildingElementType
    {
        private List<MaterialLayer> frameMaterialLayers;
        private List<MaterialLayer> paneMaterialLayers;

        public OpeningType(OpeningType openingType)
            : base(openingType)
        {

        }

        public OpeningType(JObject jObject)
            : base(jObject)
        {

        }

        public OpeningType(string name)
            : base(name)
        {

        }

        public OpeningType(System.Guid guid, string name)
            : base(guid, name)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();

            if (jObject == null)
                return jObject;

            return jObject;
        }

        public List<MaterialLayer> FrameMaterialLayers
        {
            get
            {
                if(frameMaterialLayers == null)
                {
                    return null;
                }

                return frameMaterialLayers.ConvertAll(x => new MaterialLayer(x));
            }
        }

        public List<MaterialLayer> PaneMaterialLayers
        {
            get
            {
                if (paneMaterialLayers == null)
                {
                    return null;
                }

                return paneMaterialLayers.ConvertAll(x => new MaterialLayer(x));
            }
        }

    }
}
