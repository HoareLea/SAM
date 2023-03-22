using Newtonsoft.Json.Linq;
using SAM.Core;
using System;

namespace SAM.Analytical
{
    public class SurfaceSimulationResult : Result
    {
        public SurfaceSimulationResult(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public SurfaceSimulationResult(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public SurfaceSimulationResult(SurfaceSimulationResult surfaceSimulationResult)
            : base(surfaceSimulationResult)
        {

        }

        public SurfaceSimulationResult(Guid guid, SurfaceSimulationResult surfaceSimulationResult)
            : base(guid, surfaceSimulationResult)
        {

        }

        public SurfaceSimulationResult(JObject jObject)
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