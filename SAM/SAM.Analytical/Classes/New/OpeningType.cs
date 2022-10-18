using Newtonsoft.Json.Linq;

using System.Collections.Generic;
using SAM.Architectural;
using System.Linq;

namespace SAM.Analytical
{
    public abstract class OpeningType : BuildingElementType
    {
        private List<MaterialLayer> frameMaterialLayers;
        private List<MaterialLayer> paneMaterialLayers;

        public OpeningType(OpeningType openingType)
            : base(openingType)
        {

        }

        public OpeningType(OpeningType openingType, string name)
            : base(openingType, name)
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

        public OpeningType(string name, IEnumerable<MaterialLayer> paneMaterialLayers, IEnumerable<MaterialLayer> frameMaterialLayers = null)
            : base(name)
        {
            if (paneMaterialLayers != null)
            {
                this.paneMaterialLayers = new List<MaterialLayer>(paneMaterialLayers).ConvertAll(x => new MaterialLayer(x));
            }

            if(frameMaterialLayers != null)
            {
                this.frameMaterialLayers = new List<MaterialLayer>(frameMaterialLayers).ConvertAll(x => new MaterialLayer(x));
            }
        }

        public OpeningType(System.Guid guid, string name, IEnumerable<MaterialLayer> paneMaterialLayers, IEnumerable<MaterialLayer> frameMaterialLayers = null)
            : base(guid, name)
        {
            if (paneMaterialLayers != null)
            {
                this.paneMaterialLayers = new List<MaterialLayer>(paneMaterialLayers).ConvertAll(x => new MaterialLayer(x));
            }

            if (frameMaterialLayers != null)
            {
                this.frameMaterialLayers = new List<MaterialLayer>(frameMaterialLayers).ConvertAll(x => new MaterialLayer(x));
            }
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

            set
            {
                if (value == null)
                {
                    return;
                }

                frameMaterialLayers = value?.ConvertAll(x => new MaterialLayer(x));
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

            set
            {
                if (value == null)
                {
                    return;
                }

                paneMaterialLayers = value?.ConvertAll(x => new MaterialLayer(x));
            }
        }

        public List<MaterialLayer> GetMaterialLayers(OpeningPart openingPart)
        {
            if(openingPart == OpeningPart.Undefined)
            {
                return null;
            }

            switch(openingPart)
            {
                case OpeningPart.Frame:
                    return FrameMaterialLayers;

                case OpeningPart.Pane:
                    return PaneMaterialLayers;
            }

            return null;
        }

        public double GetThickness(OpeningPart openingPart)
        {
            if(openingPart == OpeningPart.Undefined)
            {
                return double.NaN;
            }

            List<MaterialLayer> materialLayers = GetMaterialLayers(openingPart);
            if(materialLayers == null || materialLayers.Count == 0)
            {
                return 0;
            }

            return materialLayers.ConvertAll(x => x.Thickness).Sum();
        }

        public double GetFrameThickness()
        {
            return GetThickness(OpeningPart.Frame);
        }

        public double GetPaneThickness()
        {
            return GetThickness(OpeningPart.Pane);
        }

    }
}
