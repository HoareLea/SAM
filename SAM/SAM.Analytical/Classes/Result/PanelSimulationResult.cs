using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class PanelSimulationResult : Result
    {
        public PanelSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public PanelSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public PanelSimulationResult(PanelSimulationResult panelSimulationResult)
            : base(panelSimulationResult)
        {

        }

        public PanelSimulationResult(Guid guid, PanelSimulationResult panelSimulationResult)
            : base(guid, panelSimulationResult)
        {

        }

        public PanelSimulationResult(JObject jObject)
            : base(jObject)
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
                return null;

            return jObject;
        }
    }
}