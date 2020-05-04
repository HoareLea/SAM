using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class ApertureConstruction : SAMType
    {
        private ApertureType apertureType;
        private Construction frameConstruction;
        private Construction paneConstruction;

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

        public ApertureConstruction(ApertureConstruction apertureConstruction)
            : base(apertureConstruction)
        {
            apertureType = apertureConstruction.apertureType;
            frameConstruction = apertureConstruction.frameConstruction;
            paneConstruction = apertureConstruction.paneConstruction;
        }

        public ApertureConstruction(JObject jObject)
            : base(jObject)
        {
            FromJObject(jObject);
        }

        public Construction FrameConstruction
        {
            get
            {
                if (frameConstruction == null)
                    return null;

                return new Construction(frameConstruction);
            }
        }

        public Construction PaneConstruction
        {
            get
            {
                if (paneConstruction == null)
                    return null;

                return new Construction(paneConstruction);
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

            if (jObject.ContainsKey("FrameConstruction"))
                frameConstruction = new Construction(jObject.Value<JObject>("FrameConstruction"));

            if (jObject.ContainsKey("PaneConstruction"))
                paneConstruction = new Construction(jObject.Value<JObject>("PaneConstruction"));

            if (jObject.ContainsKey("ApertureType"))
                Enum.TryParse(jObject.Value<string>("ApertureType"), out apertureType);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (frameConstruction != null)
                jObject.Add("FrameConstruction", frameConstruction.ToJObject());

            if (paneConstruction != null)
                jObject.Add("PaneConstruction", paneConstruction.ToJObject());

            jObject.Add("ApertureType", apertureType.ToString());

            return jObject;
        }
    }
}