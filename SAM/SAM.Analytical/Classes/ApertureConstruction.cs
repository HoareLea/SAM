using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class ApertureConstruction : SAMType
    {
        private ApertureType apertureType;
        private List<ConstructionLayer> frameConstructionLayers;
        private List<ConstructionLayer> paneConstructionLayers;

        public ApertureConstruction(string name, ApertureType apertureType)
            : base(name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(Guid guid, string name, ApertureType apertureType)
            : base(guid, name)
        {
            this.apertureType = apertureType;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction, string name)
            : base(apertureConstruction, name)
        {
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(Guid guid, string name, ApertureType apertureType, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers = null)
             : base(guid, name)
        {
            this.apertureType = apertureType;

            if (paneConstructionLayers != null)
                this.paneConstructionLayers = new List<ConstructionLayer>(paneConstructionLayers);

            if (frameConstructionLayers != null)
                this.frameConstructionLayers = new List<ConstructionLayer>(frameConstructionLayers);
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;
            frameConstructionLayers = apertureConstruction.FrameConstructionLayers;
            paneConstructionLayers = apertureConstruction.PaneConstructionLayers;
        }

        public ApertureConstruction(ApertureConstruction apertureConstruction, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers = null)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;

            this.frameConstructionLayers = frameConstructionLayers?.ToList().ConvertAll(x => x.Clone());
            this.paneConstructionLayers = paneConstructionLayers?.ToList().ConvertAll(x => x.Clone());
        }

        public ApertureConstruction(JObject jObject)
            : base(jObject)
        {

        }

        public List<ConstructionLayer> FrameConstructionLayers
        {
            get
            {
                if (frameConstructionLayers == null)
                    return null;

                return frameConstructionLayers.ConvertAll(x => x.Clone());
            }
        }

        public List<ConstructionLayer> PaneConstructionLayers
        {
            get
            {
                if (paneConstructionLayers == null)
                    return null;

                return paneConstructionLayers.ConvertAll(x => x.Clone());
            }
        }

        public ApertureType ApertureType
        {
            get
            {
                return apertureType;
            }
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("FrameConstructionLayers"))
                frameConstructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(jObject.Value<JArray>("FrameConstructionLayers"));

            if (jObject.ContainsKey("PaneConstructionLayers"))
                paneConstructionLayers = Core.Create.IJSAMObjects<ConstructionLayer>(jObject.Value<JArray>("PaneConstructionLayers"));

            if (jObject.ContainsKey("ApertureType"))
                Enum.TryParse(jObject.Value<string>("ApertureType"), out apertureType);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (paneConstructionLayers != null)
                jObject.Add("PaneConstructionLayers", Core.Create.JArray(paneConstructionLayers));

            if (frameConstructionLayers != null)
                jObject.Add("FrameConstructionLayers", Core.Create.JArray(frameConstructionLayers));

            jObject.Add("ApertureType", apertureType.ToString());

            return jObject;
        }
    }
}